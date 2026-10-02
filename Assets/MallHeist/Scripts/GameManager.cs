using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

public enum GameState { Playing, Busted, Escaped, TimeUp }

public class GameManager : MonoBehaviour
{
    public static GameManager I;

    public PlayerController Player;
    public readonly List<Guard> Guards = new List<Guard>();
    public readonly List<Vector3> PatrolPoints = new List<Vector3>();
    public Bounds ExitArea;
    public float TimeLeft = 300f;
    public int Goal = 500000;

    public GameState State { get; private set; } = GameState.Playing;
    public bool AlarmActive => Time.time < alarmUntil;

    float alarmUntil, toastUntil, introUntil;
    string toast;
    bool warnedEmpty;
    AudioSource siren;
    Texture2D white;

    void Awake()
    {
        I = this;
        introUntil = Time.time + 12f;
        siren = gameObject.AddComponent<AudioSource>();
        siren.clip = MakeSiren();
        siren.loop = true;
        siren.volume = 0.2f;
        siren.spatialBlend = 0f;
        white = Texture2D.whiteTexture;
    }

    void Start() => LockCursor(true);

    void OnDestroy()
    {
        if (I == this) I = null;
    }

    static void LockCursor(bool on)
    {
        Cursor.lockState = on ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !on;
    }

    void Update()
    {
        if (GameInput.EscapePressed()) LockCursor(false);

        if (State != GameState.Playing)
        {
            if (GameInput.RestartPressed()) MallBootstrap.Restart();
            return;
        }

        if (GameInput.ClickPressed() && Cursor.lockState != CursorLockMode.Locked) LockCursor(true);

        TimeLeft -= Time.deltaTime;
        if (TimeLeft <= 0f) { TimeLeft = 0f; End(GameState.TimeUp); return; }

        if (ExitArea.Contains(Player.transform.position + Vector3.up))
        {
            if (Player.Bag.Count > 0) { End(GameState.Escaped); return; }
            if (!warnedEmpty) Toast("Ты ещё ничего не спиздил. Куда собрался?");
            warnedEmpty = true;
        }
        else warnedEmpty = false;

        bool chase = false;
        foreach (var g in Guards) chase |= g.IsChasing;
        bool loud = chase || AlarmActive;
        if (loud && !siren.isPlaying) siren.Play();
        else if (!loud && siren.isPlaying) siren.Stop();
    }

    public void Toast(string msg)
    {
        toast = msg;
        toastUntil = Time.time + 3f;
    }

    // Тревога: два ближайших свободных охранника бегут проверять.
    public void RaiseAlarm(Vector3 pos, string msg)
    {
        alarmUntil = Time.time + 25f;
        if (!string.IsNullOrEmpty(msg)) Toast(msg);
        var sorted = new List<Guard>(Guards);
        sorted.Sort((a, b) => (a.transform.position - pos).sqrMagnitude.CompareTo((b.transform.position - pos).sqrMagnitude));
        int sent = 0;
        foreach (var g in sorted)
        {
            if (g.IsChasing) continue;
            g.Investigate(pos);
            if (++sent >= 2) break;
        }
    }

    public void KeepAlarm(Vector3 pos)
    {
        alarmUntil = Mathf.Max(alarmUntil, Time.time + 25f);
    }

    public void Bust() => End(GameState.Busted);

    void End(GameState s)
    {
        if (State != GameState.Playing) return;
        State = s;
        siren.Stop();
        LockCursor(false);
    }

    public static string Money(int v) =>
        v.ToString("#,0", CultureInfo.InvariantCulture).Replace(",", " ") + " руб.";

    static string Rank(int v, int goal)
    {
        if (v >= goal * 2) return "Ограбление века. Про тебя снимут сериал на НТВ.";
        if (v >= goal) return "Легенда ТРЦ. Охрана до сих пор в шоке.";
        if (v >= goal / 3) return "Крепкий середнячок. Мама бы гордилась (нет).";
        if (v >= 5000) return "Мелкий несун. Ну хоть что-то.";
        return "Спиздил сникерс. Позорище.";
    }

    // ---------- HUD ----------

    void OnGUI()
    {
        if (Player == null) return;
        float s = Screen.height / 1080f;
        var label = new GUIStyle(GUI.skin.label) { fontSize = Mathf.RoundToInt(26 * s), richText = true };
        label.normal.textColor = Color.white;
        var center = new GUIStyle(label) { alignment = TextAnchor.MiddleCenter };
        var big = new GUIStyle(center) { fontSize = Mathf.RoundToInt(72 * s), fontStyle = FontStyle.Bold };

        if (State == GameState.Playing)
        {
            // Панель слева сверху
            GUI.Box(new Rect(10 * s, 10 * s, 560 * s, 230 * s), GUIContent.none);
            float y = 18 * s, lh = 34 * s, x = 24 * s;
            string vc = Player.BagValue >= Goal ? "#60ff60" : "#ffd040";
            GUI.Label(new Rect(x, y, 540 * s, lh), $"Хабар: <b><color={vc}>{Money(Player.BagValue)}</color></b>  / цель {Money(Goal)}", label); y += lh;
            GUI.Label(new Rect(x, y, 540 * s, lh), $"Рюкзак: {Player.BagWeight:0.0} / {Player.MaxWeight:0} кг   ({Player.Bag.Count} шт.)", label); y += lh;
            int t = Mathf.CeilToInt(TimeLeft);
            string tc = TimeLeft < 60 ? "#ff6060" : "#ffffff";
            GUI.Label(new Rect(x, y, 540 * s, lh), $"До закрытия ТРЦ: <color={tc}>{t / 60}:{t % 60:00}</color>", label); y += lh;

            GUI.Label(new Rect(x, y, 200 * s, lh), "Выносливость", label);
            Bar(new Rect(x + 190 * s, y + 10 * s, 320 * s, 16 * s), Player.Stamina, new Color(0.3f, 0.8f, 1f)); y += lh;

            bool chase = false;
            foreach (var g in Guards) chase |= g.IsChasing;
            var st = new StringBuilder();
            if (chase) st.Append("<color=#ff3030><b>ПОГОНЯ!</b></color>  ");
            else if (AlarmActive) st.Append("<color=#ffa020><b>ТРЕВОГА</b></color>  ");
            if (Player.Hidden) st.Append("<color=#70ff70>СПРЯТАН</color>  ");
            if (Player.Crouching) st.Append("крадёшься  ");
            if (st.Length == 0) st.Append("<color=#aaaaaa>всё спокойно</color>");
            GUI.Label(new Rect(x, y, 540 * s, lh), st.ToString(), label);

            // Прицел
            GUI.Label(new Rect(Screen.width / 2f - 20, Screen.height / 2f - 20, 40, 40), "+", center);

            // Подсказка
            var l = Player.LookedAt;
            if (l != null)
            {
                string txt = $"<b>[E]</b> Спиздить: {l.ItemName} — <color=#ffd040>{Money(l.Price)}</color>, {l.Weight:0.##} кг";
                Rect r = new Rect(Screen.width / 2f - 450 * s, Screen.height * 0.62f, 900 * s, 50 * s);
                GUI.Box(r, GUIContent.none);
                GUI.Label(r, txt, center);
            }

            if (Time.time < toastUntil && !string.IsNullOrEmpty(toast))
            {
                Rect r = new Rect(Screen.width / 2f - 500 * s, 20 * s, 1000 * s, 50 * s);
                GUI.Box(r, GUIContent.none);
                GUI.Label(r, toast, center);
            }

            if (Time.time < introUntil)
            {
                Rect r = new Rect(Screen.width - 620 * s, 10 * s, 610 * s, 330 * s);
                GUI.Box(r, GUIContent.none);
                GUI.Label(new Rect(r.x + 14 * s, r.y + 8 * s, r.width - 20 * s, r.height),
                    "<b>ТРЦ «МЕГА-ХАПУГА»</b>\n" +
                    "Набей рюкзак и свали через главный вход.\n\n" +
                    "WASD — ходить, мышь — смотреть\n" +
                    "Shift — бежать (с хабаром палевно!)\n" +
                    "C / Ctrl — красться\n" +
                    "E — спиздить, G — выкинуть последнее\n" +
                    "Примерочные — спрятаться (если не спалили)\n" +
                    "Esc — отпустить мышь, клик — вернуть", label);
            }
            return;
        }

        // Экран итогов
        GUI.color = new Color(0, 0, 0, 0.75f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), white);
        GUI.color = Color.white;

        string title, sub;
        switch (State)
        {
            case GameState.Escaped:
                title = "<color=#60ff60>СВАЛИЛ!</color>";
                sub = "Вынес на " + Money(Player.BagValue) + "\n" + Rank(Player.BagValue, Goal);
                break;
            case GameState.Busted:
                title = "<color=#ff4040>ПОВЯЗАЛИ!</color>";
                sub = "Охрана скрутила тебя и вызвала ментов.\nИзъято товара на " + Money(Player.BagValue);
                break;
            default:
                title = "<color=#ffb030>ТРЦ ЗАКРЫЛСЯ</color>";
                sub = "Ты застрял внутри. Утром тебя нашла уборщица.\nТовар на " + Money(Player.BagValue) + " вернули на полки.";
                break;
        }

        var list = new StringBuilder();
        int shown = 0;
        foreach (var it in Player.Bag)
        {
            if (shown++ >= 10) { list.Append("...и ещё ").Append(Player.Bag.Count - 10).Append(" шт.\n"); break; }
            list.Append(it.Name).Append(" — ").Append(Money(it.Price)).Append('\n');
        }

        float cy = Screen.height * 0.18f;
        GUI.Label(new Rect(0, cy, Screen.width, 100 * s), title, big);
        GUI.Label(new Rect(0, cy + 110 * s, Screen.width, 90 * s), sub, center);
        GUI.Label(new Rect(0, cy + 210 * s, Screen.width, 420 * s), list.ToString(), center);
        GUI.Label(new Rect(0, Screen.height - 100 * s, Screen.width, 50 * s), "<b>[R]</b> — ещё раз", center);
    }

    void Bar(Rect r, float v, Color c)
    {
        GUI.color = new Color(0, 0, 0, 0.6f);
        GUI.DrawTexture(r, white);
        GUI.color = c;
        GUI.DrawTexture(new Rect(r.x, r.y, r.width * Mathf.Clamp01(v), r.height), white);
        GUI.color = Color.white;
    }

    static AudioClip MakeSiren()
    {
        const int rate = 22050;
        int len = rate * 2;
        var data = new float[len];
        float phase = 0f;
        for (int i = 0; i < len; i++)
        {
            float t = (float)i / rate;
            float f = ((int)(t * 2f)) % 2 == 0 ? 660f : 880f;
            phase += 2f * Mathf.PI * f / rate;
            data[i] = Mathf.Sin(phase) * 0.4f;
        }
        var clip = AudioClip.Create("Siren", len, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
