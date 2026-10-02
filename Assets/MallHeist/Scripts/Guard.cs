using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Guard : MonoBehaviour
{
    enum State { Wait, Patrol, Investigate, Chase, Search }

    static readonly string[] Shouts =
    {
        "Э, стоять!", "Стой, кому говорю!", "Положи на место!", "Я тебя запомнил!",
        "Код красный, у нас несун!", "Ну всё, тебе конец!", "Куда побежал?!"
    };

    public float PatrolSpeed = 2.2f, InvestigateSpeed = 3.8f, ChaseSpeed = 5.3f;
    public float ViewDist = 17f, ViewHalfAngle = 55f, CatchDist = 1.4f;
    public Renderer Body;

    State state = State.Wait;
    CharacterController cc;
    readonly List<Vector3> path = new List<Vector3>();
    int pathIdx;
    Vector3 lastKnown;
    float timer, repath, lostFor, stuck, lookPhase;

    public bool IsChasing => state == State.Chase;
    Vector3 Eye => transform.position + Vector3.up * 1.7f;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        timer = Random.Range(0.5f, 2f);
        lookPhase = Random.value * 10f;
    }

    public void Investigate(Vector3 pos)
    {
        if (state == State.Chase) return;
        state = State.Investigate;
        SetDest(pos);
    }

    void Update()
    {
        var gm = GameManager.I;
        if (gm == null || gm.State != GameState.Playing) return;
        var p = gm.Player;
        float dt = Time.deltaTime;

        float range = ViewDist * (p.Crouching ? 0.6f : 1f);
        bool sees = Vision.CanSee(Eye, transform.forward, range, ViewHalfAngle, p);
        if (sees)
        {
            bool alert = state == State.Chase || state == State.Search || state == State.Investigate
                         || gm.AlarmActive || p.LooksSuspicious;
            if (alert)
            {
                lastKnown = p.transform.position;
                lostFor = 0f;
                if (state != State.Chase)
                {
                    state = State.Chase;
                    repath = 0f;
                    gm.RaiseAlarm(p.transform.position, "Охранник: «" + Shouts[Random.Range(0, Shouts.Length)] + "»");
                }
                else gm.KeepAlarm(p.transform.position);
            }
        }

        switch (state)
        {
            case State.Wait:
                timer -= dt;
                LookAround(dt);
                if (timer <= 0f && gm.PatrolPoints.Count > 0)
                {
                    SetDest(gm.PatrolPoints[Random.Range(0, gm.PatrolPoints.Count)]);
                    state = State.Patrol;
                }
                break;

            case State.Patrol:
                if (FollowPath(PatrolSpeed)) { state = State.Wait; timer = Random.Range(1.5f, 4f); }
                break;

            case State.Investigate:
                if (FollowPath(InvestigateSpeed)) { state = State.Search; timer = 5f; }
                break;

            case State.Chase:
                repath -= dt;
                if (repath <= 0f) { SetDest(lastKnown); repath = 0.3f; }
                bool arrived = FollowPath(ChaseSpeed);
                if (!sees)
                {
                    lostFor += dt;
                    if (arrived || lostFor > 7f)
                    {
                        state = State.Search;
                        timer = 5f;
                        gm.Toast("Охранник потерял тебя из виду...");
                    }
                }
                Vector3 toP = p.transform.position - transform.position;
                toP.y = 0f;
                if (toP.magnitude < CatchDist) gm.Bust();
                break;

            case State.Search:
                timer -= dt;
                LookAround(dt * 2.5f);
                if (timer <= 0f) { state = State.Wait; timer = 0.5f; }
                break;
        }

        Color c = state == State.Chase ? new Color(0.85f, 0.1f, 0.1f)
                : state == State.Investigate || state == State.Search ? new Color(0.9f, 0.7f, 0.1f)
                : new Color(0.15f, 0.25f, 0.6f);
        if (Body != null) Body.sharedMaterial = Prim.Mat(c);
    }

    void SetDest(Vector3 d)
    {
        pathIdx = 0;
        stuck = 0f;
        if (NavGrid.I == null || !NavGrid.I.FindPath(transform.position, d, path))
        {
            path.Clear();
            path.Add(d);
        }
    }

    // true — когда дошёл.
    bool FollowPath(float speed)
    {
        if (pathIdx >= path.Count) { cc.SimpleMove(Vector3.zero); return true; }
        Vector3 to = path[pathIdx] - transform.position;
        to.y = 0f;
        if (to.magnitude < 0.35f)
        {
            pathIdx++;
            return pathIdx >= path.Count;
        }
        Vector3 dir = to.normalized;
        cc.SimpleMove(dir * speed);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(dir), 360f * Time.deltaTime);

        Vector3 v = cc.velocity;
        v.y = 0f;
        stuck = v.magnitude < speed * 0.2f ? stuck + Time.deltaTime : 0f;
        if (stuck > 1.5f) { path.Clear(); return true; }
        return false;
    }

    void LookAround(float dt)
    {
        transform.Rotate(0f, Mathf.Sin(Time.time * 1.3f + lookPhase) * 70f * dt, 0f);
    }
}
