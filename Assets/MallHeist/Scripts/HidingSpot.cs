using System.Collections.Generic;
using UnityEngine;

// Примерочная: внутри тебя не видят ни охрана, ни камеры.
// Но если охранник видел, как ты туда залез, — он придёт проверить.
public class HidingSpot : MonoBehaviour
{
    static readonly List<HidingSpot> all = new List<HidingSpot>();
    public Bounds Area;

    void OnEnable() => all.Add(this);
    void OnDisable() => all.Remove(this);

    public static bool Contains(Vector3 feet)
    {
        foreach (var h in all)
            if (h.Area.Contains(feet + Vector3.up * 0.5f)) return true;
        return false;
    }
}
