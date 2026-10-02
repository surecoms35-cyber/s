using System.Collections.Generic;
using UnityEngine;

// Хелперы для процедурной постройки уровня из примитивов.
public static class Prim
{
    static readonly Dictionary<Color, Material> cache = new Dictionary<Color, Material>();
    static Shader shader;
    static Font font;

    public static Material Mat(Color c)
    {
        if (cache.TryGetValue(c, out var m) && m != null) return m;
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Diffuse");
        m = new Material(shader);
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        if (m.HasProperty("_Color")) m.SetColor("_Color", c);
        cache[c] = m;
        return m;
    }

    // Куб в мировых координатах.
    public static GameObject Box(string name, Vector3 pos, Vector3 size, Color c, Transform parent, bool collider = true)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.position = pos;
        go.transform.localScale = size;
        go.GetComponent<Renderer>().sharedMaterial = Mat(c);
        if (!collider) Object.DestroyImmediate(go.GetComponent<Collider>());
        return go;
    }

    // Деталь в локальных координатах родителя, без коллайдера.
    public static GameObject Part(PrimitiveType type, string name, Transform parent, Vector3 localPos, Vector3 localScale, Color c)
    {
        var go = GameObject.CreatePrimitive(type);
        go.name = name;
        Object.DestroyImmediate(go.GetComponent<Collider>());
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = localScale;
        go.GetComponent<Renderer>().sharedMaterial = Mat(c);
        return go;
    }

    // Стена между двумя точками на полу (вдоль X или вдоль Z).
    public static GameObject Wall(Vector3 a, Vector3 b, float height, Color c, Transform parent, float y0 = 0f, float thickness = 0.3f)
    {
        Vector3 center = (a + b) * 0.5f;
        center.y = y0 + height * 0.5f;
        float dx = Mathf.Abs(b.x - a.x), dz = Mathf.Abs(b.z - a.z);
        Vector3 size = dx >= dz
            ? new Vector3(dx + thickness, height, thickness)
            : new Vector3(thickness, height, dz + thickness);
        return Box("Wall", center, size, c, parent);
    }

    public static TextMesh Label(string text, Vector3 pos, Quaternion rot, float charSize, Color c, Transform parent)
    {
        if (font == null)
        {
            try { font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch { }
            if (font == null) { try { font = Resources.GetBuiltinResource<Font>("Arial.ttf"); } catch { } }
        }
        var go = new GameObject("Label");
        go.transform.SetParent(parent, false);
        go.transform.position = pos;
        go.transform.rotation = rot;
        var tm = go.AddComponent<TextMesh>();
        tm.font = font;
        tm.text = text;
        tm.fontSize = 64;
        tm.characterSize = charSize;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = c;
        if (font != null) go.GetComponent<MeshRenderer>().sharedMaterial = font.material;
        return tm;
    }
}
