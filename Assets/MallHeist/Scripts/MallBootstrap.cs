using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Строит весь ТРЦ из кода при запуске любой сцены — ничего настраивать не надо.
public static class MallBootstrap
{
    class ItemDef
    {
        public string Name; public int Price; public float Kg; public Color Color;
        public ItemDef(string name, int price, float kg, Color color) { Name = name; Price = price; Kg = kg; Color = color; }
    }

    class StoreDef
    {
        public string Name; public Color Floor; public bool HasCamera, HasFittingRoom; public ItemDef[] Items;
    }

    static readonly Color WallColor = new Color(0.82f, 0.8f, 0.76f);
    static readonly Color GlassColor = new Color(0.55f, 0.75f, 0.85f);
    static readonly Color ShelfColor = new Color(0.45f, 0.45f, 0.5f);
    static readonly Color Dark = new Color(0.12f, 0.12f, 0.14f);
    static readonly Color Gold = new Color(1f, 0.8f, 0.2f);

    static GameObject root;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Hook()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode != LoadSceneMode.Single) return;
        foreach (var c in Camera.allCameras) Object.Destroy(c.gameObject);
        var sun = GameObject.Find("Directional Light");
        if (sun != null) Object.Destroy(sun);
        Build();
    }

    public static void Restart()
    {
        if (root != null)
        {
            root.SetActive(false); // сразу убираем коллайдеры из физики
            Object.Destroy(root);
        }
        Build();
    }

    static Color C(float r, float g, float b) => new Color(r, g, b);

    static StoreDef[] Stores() => new[]
    {
        // Север (слева направо)
        new StoreDef { Name = "Пятёрка", Floor = C(0.25f, 0.45f, 0.25f), Items = new[] {
            new ItemDef("Сникерс", 89, 0.05f, C(0.4f, 0.25f, 0.1f)),
            new ItemDef("Пельмени «Сибирские»", 549, 1f, Color.white),
            new ItemDef("Икра красная", 3490, 0.2f, C(1f, 0.35f, 0.1f)),
            new ItemDef("Коньяк «Арарат»", 4990, 0.7f, C(0.8f, 0.5f, 0.1f)),
            new ItemDef("Хамон целиком", 12990, 6f, C(0.6f, 0.15f, 0.15f)),
            new ItemDef("Кофе в зёрнах", 1890, 1f, C(0.3f, 0.2f, 0.1f)) } },
        new StoreDef { Name = "Зарина", Floor = C(0.5f, 0.3f, 0.45f), HasFittingRoom = true, Items = new[] {
            new ItemDef("Пуховик", 18990, 1.5f, Dark),
            new ItemDef("Кроссовки", 12990, 1f, Color.white),
            new ItemDef("Джинсы", 5990, 0.8f, C(0.2f, 0.3f, 0.7f)),
            new ItemDef("Худи оверсайз", 4990, 0.7f, C(0.5f, 0.5f, 0.5f)),
            new ItemDef("Шуба (эко)", 39990, 4f, C(0.45f, 0.3f, 0.2f)) } },
        new StoreDef { Name = "М.Видос", Floor = C(0.2f, 0.2f, 0.3f), HasCamera = true, Items = new[] {
            new ItemDef("iPhone 17 Pro", 139990, 0.25f, C(0.8f, 0.8f, 0.85f)),
            new ItemDef("Ноутбук", 99990, 2.2f, C(0.5f, 0.5f, 0.55f)),
            new ItemDef("AirPods", 24990, 0.1f, Color.white),
            new ItemDef("PlayStation 6", 74990, 4.5f, Dark),
            new ItemDef("Телевизор 65\"", 89990, 18f, Dark),
            new ItemDef("Смарт-часы", 34990, 0.1f, C(0.2f, 0.2f, 0.25f)) } },
        new StoreDef { Name = "Золотой Лось", Floor = C(0.35f, 0.3f, 0.15f), HasCamera = true, Items = new[] {
            new ItemDef("Кольцо с бриллиантом", 249990, 0.05f, Gold),
            new ItemDef("Золотая цепь", 119990, 0.15f, Gold),
            new ItemDef("Серьги", 59990, 0.05f, Gold),
            new ItemDef("Часы «Ролекс»", 349990, 0.2f, Gold) } },
        // Юг (слева направо)
        new StoreDef { Name = "Сделай Сам", Floor = C(0.4f, 0.35f, 0.2f), Items = new[] {
            new ItemDef("Перфоратор", 14990, 4f, C(0.1f, 0.5f, 0.5f)),
            new ItemDef("Шуруповёрт", 7990, 1.5f, C(1f, 0.5f, 0f)),
            new ItemDef("Болгарка", 6990, 2.5f, C(0.1f, 0.3f, 0.8f)),
            new ItemDef("Изолента", 99, 0.05f, C(0.1f, 0.3f, 0.9f)),
            new ItemDef("Бензопила", 24990, 7f, C(1f, 0.45f, 0f)) } },
        new StoreDef { Name = "Детский Мирок", Floor = C(0.5f, 0.45f, 0.2f), HasCamera = true, Items = new[] {
            new ItemDef("LEGO «Звезда Смерти»", 79990, 8f, C(0.6f, 0.6f, 0.6f)),
            new ItemDef("Плюшевый медведь", 2990, 0.6f, C(0.55f, 0.35f, 0.2f)),
            new ItemDef("Машинка на пульте", 6990, 1.2f, Color.red),
            new ItemDef("Кукла Барби", 3490, 0.4f, C(1f, 0.4f, 0.7f)) } },
        new StoreDef { Name = "Золотое Яблочко", Floor = C(0.45f, 0.2f, 0.3f), HasCamera = true, Items = new[] {
            new ItemDef("Духи Chanel №5", 17990, 0.3f, C(0.95f, 0.85f, 0.6f)),
            new ItemDef("Крем для лица", 6990, 0.2f, Color.white),
            new ItemDef("Помада", 2490, 0.05f, C(0.8f, 0.05f, 0.1f)),
            new ItemDef("Набор косметики", 12990, 1f, C(1f, 0.6f, 0.8f)) } },
        new StoreDef { Name = "Спортмастерище", Floor = C(0.15f, 0.3f, 0.45f), HasFittingRoom = true, Items = new[] {
            new ItemDef("Велосипед", 59990, 14f, Color.red),
            new ItemDef("Гантели 10 кг", 3990, 10f, Dark),
            new ItemDef("Футбольный мяч", 2990, 0.5f, Color.white),
            new ItemDef("Электросамокат", 34990, 12f, C(0.15f, 0.15f, 0.15f)),
            new ItemDef("Беговые кроссовки", 11990, 1f, C(0.2f, 0.9f, 0.9f)) } },
    };

    static void Build()
    {
        root = new GameObject("MallHeist");
        var t = root.transform;

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.36f, 0.36f, 0.4f);
        var sunGo = new GameObject("Sun");
        sunGo.transform.SetParent(t);
        sunGo.transform.rotation = Quaternion.Euler(55f, -35f, 0f);
        var sun = sunGo.AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.intensity = 0.55f;
        sun.color = new Color(1f, 0.93f, 0.82f);
        sun.shadows = LightShadows.Soft;

        BuildShell(t);

        var patrol = new List<Vector3>();
        var stores = Stores();
        for (int i = 0; i < 4; i++)
        {
            BuildStore(stores[i], i, 1, t, patrol);
            BuildStore(stores[4 + i], i, -1, t, patrol);
        }
        for (float x = -36f; x <= 36f; x += 8f)
        {
            patrol.Add(new Vector3(x, 0f, 2.5f));
            patrol.Add(new Vector3(x, 0f, -2.5f));
        }

        BuildDecor(t);

        // Сетку проходимости строим до того, как появятся люди.
        new NavGrid(new Vector3(-44f, 0f, -22f), new Vector3(44f, 0f, 22f), 1f);

        var gm = new GameObject("GameManager").AddComponent<GameManager>();
        gm.transform.SetParent(t);
        gm.PatrolPoints.AddRange(patrol);
        gm.ExitArea = new Bounds(new Vector3(-47.5f, 1f, 0f), new Vector3(5f, 4f, 7f));
        gm.Player = SpawnPlayer(new Vector3(-41f, 0.05f, 0f), 90f, t);

        Vector3[] guardSpawns =
        {
            new Vector3(-8f, 0.05f, 2f), new Vector3(12f, 0.05f, -2f),
            new Vector3(30f, 0.05f, 2f), new Vector3(27f, 0.05f, 13f),
        };
        for (int i = 0; i < guardSpawns.Length; i++)
            gm.Guards.Add(SpawnGuard(guardSpawns[i], Random.Range(0f, 360f), t, i + 1));
    }

    static void BuildShell(Transform t)
    {
        Prim.Box("Floor", new Vector3(-3f, -0.5f, 0f), new Vector3(106f, 1f, 50f), C(0.55f, 0.55f, 0.58f), t);
        Prim.Box("Street", new Vector3(-50f, 0.005f, 0f), new Vector3(12f, 0.01f, 12f), C(0.25f, 0.25f, 0.27f), t, false);
        Prim.Box("Corridor", new Vector3(0f, 0.005f, 0f), new Vector3(88f, 0.01f, 12f), C(0.78f, 0.76f, 0.7f), t, false);

        const float H = 5f;
        Prim.Wall(new Vector3(-44, 0, -22), new Vector3(44, 0, -22), H, WallColor, t);
        Prim.Wall(new Vector3(-44, 0, 22), new Vector3(44, 0, 22), H, WallColor, t);
        Prim.Wall(new Vector3(44, 0, -22), new Vector3(44, 0, 22), H, WallColor, t);
        // Западная стена с главным входом
        Prim.Wall(new Vector3(-44, 0, -22), new Vector3(-44, 0, -3), H, WallColor, t);
        Prim.Wall(new Vector3(-44, 0, 3), new Vector3(-44, 0, 22), H, WallColor, t);
        Prim.Wall(new Vector3(-44, 0, -3), new Vector3(-44, 0, 3), 2f, WallColor, t, 3f);
        // Забор на улице, чтобы не убежать в пустоту
        Prim.Wall(new Vector3(-52, 0, -6), new Vector3(-44, 0, -6), 1.2f, Dark, t);
        Prim.Wall(new Vector3(-52, 0, 6), new Vector3(-44, 0, 6), 1.2f, Dark, t);
        Prim.Wall(new Vector3(-52, 0, -6), new Vector3(-52, 0, 6), 1.2f, Dark, t);

        Prim.Label("ВЫХОД", new Vector3(-43.7f, 3.8f, 0f), Quaternion.Euler(0f, -90f, 0f), 0.1f, Color.green, t);
        Prim.Label("ТРЦ «МЕГА-ХАПУГА»", new Vector3(-44.3f, 4.2f, 0f), Quaternion.Euler(0f, 90f, 0f), 0.1f, Gold, t);
    }

    static void BuildStore(StoreDef def, int i, int side, Transform parent, List<Vector3> patrol)
    {
        float x0 = -36f + i * 18f, x1 = x0 + 18f, cx = x0 + 9f;
        float zf = 6f * side, zb = 22f * side;
        Quaternion facing = side > 0 ? Quaternion.identity : Quaternion.Euler(0f, 180f, 0f);

        var st = new GameObject(def.Name).transform;
        st.SetParent(parent);

        Prim.Box("StoreFloor", new Vector3(cx, 0.006f, 14f * side), new Vector3(18f, 0.01f, 16f), def.Floor, st, false);

        // Стены
        Prim.Wall(new Vector3(x0, 0, zf), new Vector3(x0, 0, zb), 5f, WallColor, st);
        if (i == 3) Prim.Wall(new Vector3(x1, 0, zf), new Vector3(x1, 0, zb), 5f, WallColor, st);
        Prim.Wall(new Vector3(x0, 0, zf), new Vector3(cx - 3f, 0, zf), 3f, GlassColor, st);
        Prim.Wall(new Vector3(cx + 3f, 0, zf), new Vector3(x1, 0, zf), 3f, GlassColor, st);
        Prim.Wall(new Vector3(x0, 0, zf), new Vector3(x1, 0, zf), 2f, def.Floor * 0.7f, st, 3f);
        Prim.Label(def.Name, new Vector3(cx, 4f, zf - 0.2f * side), facing, 0.09f, Color.white, st);

        // Стеллажи с товаром
        foreach (float row in new[] { 11f, 15f, 19f })
        {
            float z = row * side;
            Prim.Box("Shelf", new Vector3(cx, 0.6f, z), new Vector3(10f, 1.2f, 1.1f), ShelfColor, st);
            for (int k = -2; k <= 2; k++)
            {
                if (Random.value > 0.8f) continue;
                var d = def.Items[Random.Range(0, def.Items.Length)];
                var pos = new Vector3(cx + k * 2f + Random.Range(-0.3f, 0.3f), 1.2f, z);
                SpawnLoot(d.Name, d.Price, d.Kg, d.Color, pos, st, false);
            }
        }

        patrol.Add(new Vector3(cx - 6f, 0f, 13f * side));
        patrol.Add(new Vector3(cx + 6f, 0f, 17f * side));
        patrol.Add(new Vector3(cx, 0f, 8f * side));

        if (def.HasCamera)
        {
            var pos = new Vector3(x0 + 0.8f, 3.4f, 6.8f * side);
            Vector3 look = new Vector3(cx + 3f, 0f, 15f * side) - pos;
            look.y = 0f;
            SpawnCamera(def.Name, pos, Quaternion.LookRotation(look).eulerAngles.y, st);
        }

        if (def.HasFittingRoom)
        {
            Prim.Wall(new Vector3(x1 - 2.5f, 0, 19.5f * side), new Vector3(x1 - 2.5f, 0, zb), 2.5f, C(0.9f, 0.88f, 0.8f), st, 0f, 0.1f);
            // Шторка без коллайдера — сквозь неё можно пройти
            Prim.Box("Curtain", new Vector3(x1 - 1.3f, 1.3f, 19.5f * side), new Vector3(2.4f, 2.2f, 0.05f), C(0.6f, 0.1f, 0.15f), st, false);
            var hs = new GameObject("FittingRoom").AddComponent<HidingSpot>();
            hs.transform.SetParent(st);
            hs.Area = new Bounds(new Vector3(x1 - 1.3f, 1f, 20.75f * side), new Vector3(2.2f, 2.5f, 2.3f));
            Prim.Label("ПРИМЕРОЧНАЯ", new Vector3(x1 - 1.3f, 2.7f, 19.4f * side), facing, 0.035f, Color.white, st);
        }
    }

    static void BuildDecor(Transform t)
    {
        var fountain = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        fountain.name = "Fountain";
        fountain.transform.SetParent(t);
        fountain.transform.position = new Vector3(0f, 0.4f, 0f);
        fountain.transform.localScale = new Vector3(4f, 0.4f, 4f);
        fountain.GetComponent<Renderer>().sharedMaterial = Prim.Mat(C(0.7f, 0.7f, 0.72f));
        Prim.Part(PrimitiveType.Cylinder, "Water", fountain.transform, new Vector3(0f, 0.55f, 0f), new Vector3(0.9f, 0.5f, 0.9f), C(0.2f, 0.5f, 0.9f));

        foreach (float x in new[] { -24f, -12f, 12f, 24f })
        {
            Prim.Box("Bench", new Vector3(x, 0.25f, 4.6f), new Vector3(2.5f, 0.5f, 0.7f), C(0.45f, 0.3f, 0.2f), t);
            Prim.Box("Bench", new Vector3(x, 0.25f, -4.6f), new Vector3(2.5f, 0.5f, 0.7f), C(0.45f, 0.3f, 0.2f), t);
        }
        foreach (float x in new[] { -30f, -18f, 18f, 30f })
        {
            Prim.Box("Pot", new Vector3(x, 0.4f, 0f), new Vector3(1f, 0.8f, 1f), C(0.35f, 0.25f, 0.2f), t);
            Prim.Box("Plant", new Vector3(x, 1.3f, 0f), new Vector3(1.2f, 1.2f, 1.2f), C(0.15f, 0.5f, 0.2f), t, false);
        }
    }

    public static GameObject SpawnLoot(string name, int price, float kg, Color color, Vector3 bottom, Transform parent, bool physics)
    {
        float size = Mathf.Clamp(0.22f + Mathf.Pow(kg, 1f / 3f) * 0.28f, 0.22f, 1f);
        var scale = new Vector3(size, size * 0.8f, size * 0.7f);
        var go = Prim.Box(name, bottom + Vector3.up * scale.y * 0.5f, scale, color, parent);
        var loot = go.AddComponent<Loot>();
        loot.ItemName = name;
        loot.Price = price;
        loot.Weight = kg;
        loot.Color = color;
        if (physics) go.AddComponent<Rigidbody>().mass = Mathf.Max(0.1f, kg);
        return go;
    }

    static void SpawnCamera(string storeName, Vector3 pos, float yaw, Transform parent)
    {
        Prim.Box("CamMount", pos + Vector3.up * 0.35f, new Vector3(0.08f, 0.5f, 0.08f), Dark, parent, false);
        var cam = new GameObject("SecurityCam");
        cam.transform.SetParent(parent);
        cam.transform.position = pos;
        cam.transform.rotation = Quaternion.Euler(22f, yaw, 0f);
        Prim.Part(PrimitiveType.Cube, "Body", cam.transform, Vector3.zero, new Vector3(0.25f, 0.25f, 0.5f), C(0.85f, 0.85f, 0.85f));
        var lens = Prim.Part(PrimitiveType.Cube, "Lens", cam.transform, new Vector3(0f, 0f, 0.26f), new Vector3(0.15f, 0.15f, 0.05f), Color.green);

        var light = new GameObject("Cone").AddComponent<Light>();
        light.transform.SetParent(cam.transform, false);
        light.type = LightType.Spot;
        light.spotAngle = 56f;
        light.range = 16f;
        light.intensity = 1.5f;
        light.color = new Color(1f, 0.9f, 0.75f);

        var sc = cam.AddComponent<SecurityCam>();
        sc.StoreName = storeName;
        sc.Lens = lens.GetComponent<Renderer>();
        sc.Init(yaw);
    }

    static PlayerController SpawnPlayer(Vector3 pos, float yaw, Transform parent)
    {
        var go = new GameObject("Player");
        go.transform.SetParent(parent);
        go.transform.position = pos;
        var cc = go.AddComponent<CharacterController>();
        cc.height = 1.8f;
        cc.radius = 0.35f;
        cc.center = new Vector3(0f, 0.9f, 0f);

        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        camGo.transform.SetParent(go.transform, false);
        camGo.transform.localPosition = new Vector3(0f, 1.6f, 0f);
        var cam = camGo.AddComponent<Camera>();
        cam.nearClipPlane = 0.05f;
        cam.fieldOfView = 70f;
        camGo.AddComponent<AudioListener>();

        var pc = go.AddComponent<PlayerController>();
        pc.Init(cam, yaw);
        return pc;
    }

    static Guard SpawnGuard(Vector3 pos, float yaw, Transform parent, int idx)
    {
        var go = new GameObject("Guard " + idx);
        go.transform.SetParent(parent);
        go.transform.position = pos;
        go.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        var cc = go.AddComponent<CharacterController>();
        cc.height = 1.9f;
        cc.radius = 0.4f;
        cc.center = new Vector3(0f, 0.95f, 0f);

        var body = Prim.Part(PrimitiveType.Capsule, "Body", go.transform, new Vector3(0f, 0.95f, 0f), new Vector3(0.8f, 0.95f, 0.8f), C(0.15f, 0.25f, 0.6f));
        Prim.Part(PrimitiveType.Cube, "Cap", go.transform, new Vector3(0f, 1.88f, 0.08f), new Vector3(0.55f, 0.12f, 0.65f), Dark);
        Prim.Part(PrimitiveType.Cube, "Eyes", go.transform, new Vector3(0f, 1.62f, 0.36f), new Vector3(0.45f, 0.08f, 0.08f), Color.white);
        Prim.Part(PrimitiveType.Cube, "Badge", go.transform, new Vector3(0.18f, 1.25f, 0.4f), new Vector3(0.15f, 0.15f, 0.04f), Gold);

        var light = new GameObject("Flashlight").AddComponent<Light>();
        light.transform.SetParent(go.transform, false);
        light.transform.localPosition = new Vector3(0f, 1.5f, 0.35f);
        light.transform.localRotation = Quaternion.Euler(12f, 0f, 0f);
        light.type = LightType.Spot;
        light.spotAngle = 70f;
        light.range = 14f;
        light.intensity = 2.5f;
        light.color = new Color(1f, 1f, 0.85f);

        var g = go.AddComponent<Guard>();
        g.Body = body.GetComponent<Renderer>();
        return g;
    }
}
