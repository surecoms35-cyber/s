using UnityEngine;

public static class Vision
{
    public static bool CanSee(Vector3 eye, Vector3 forward, float range, float halfAngle, PlayerController p)
    {
        if (p == null || p.Hidden) return false;
        Vector3 d = p.Chest - eye;
        float dist = d.magnitude;
        if (dist > range || dist < 0.01f) return false;
        if (Vector3.Angle(forward, d) > halfAngle) return false;
        if (Physics.Raycast(eye, d / dist, out var hit, dist + 0.5f, ~0, QueryTriggerInteraction.Ignore))
            return hit.collider.transform == p.transform;
        return false;
    }
}
