using UnityEngine;

// Камера видеонаблюдения: крутится туда-сюда, если видит кражу — поднимает тревогу.
public class SecurityCam : MonoBehaviour
{
    public string StoreName;
    public Renderer Lens;
    public float Sweep = 45f, Range = 20f, HalfAngle = 28f, Pitch = 22f;

    float baseYaw, phase, cooldown;

    public void Init(float yaw)
    {
        baseYaw = yaw;
        phase = Random.value * 10f;
    }

    void Update()
    {
        var gm = GameManager.I;
        if (gm == null || gm.State != GameState.Playing) return;

        float yaw = baseYaw + Mathf.Sin(Time.time * 0.5f + phase) * Sweep;
        transform.rotation = Quaternion.Euler(Pitch, yaw, 0f);

        var p = gm.Player;
        bool sees = Vision.CanSee(transform.position, transform.forward, Range, HalfAngle, p);
        if (Lens != null) Lens.sharedMaterial = Prim.Mat(sees ? Color.red : Color.green);

        cooldown -= Time.deltaTime;
        if (sees && cooldown <= 0f && (p.GrabbedRecently || gm.AlarmActive))
        {
            gm.RaiseAlarm(p.transform.position,
                p.GrabbedRecently ? "Камера в «" + StoreName + "» засекла кражу! Охрана выдвигается." : null);
            cooldown = 4f;
        }
    }
}
