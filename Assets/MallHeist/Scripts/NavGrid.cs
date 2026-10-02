using System.Collections.Generic;
using UnityEngine;

// Простая сетка проходимости + A*, чтобы охрана не тупила в стены
// (без NavMesh и без лишних пакетов).
public class NavGrid
{
    public static NavGrid I;

    readonly float minX, minZ, cell;
    readonly int w, h;
    readonly bool[] blocked;
    readonly float[] gScore;
    readonly int[] cameFrom;
    readonly bool[] closed;
    readonly SortedSet<(float, int)> open = new SortedSet<(float, int)>();
    readonly List<int> cells = new List<int>();

    public NavGrid(Vector3 min, Vector3 max, float cellSize)
    {
        cell = cellSize;
        minX = min.x;
        minZ = min.z;
        w = Mathf.CeilToInt((max.x - min.x) / cell);
        h = Mathf.CeilToInt((max.z - min.z) / cell);
        blocked = new bool[w * h];
        gScore = new float[w * h];
        cameFrom = new int[w * h];
        closed = new bool[w * h];

        Physics.SyncTransforms();
        var half = new Vector3(cell * 0.5f + 0.2f, 0.7f, cell * 0.5f + 0.2f);
        for (int z = 0; z < h; z++)
            for (int x = 0; x < w; x++)
                blocked[z * w + x] = Physics.CheckBox(Center(x, z) + Vector3.up * 0.85f, half, Quaternion.identity, ~0, QueryTriggerInteraction.Ignore);
        I = this;
    }

    Vector3 Center(int x, int z) => new Vector3(minX + (x + 0.5f) * cell, 0f, minZ + (z + 0.5f) * cell);
    Vector3 Center(int id) => Center(id % w, id / w);
    bool Free(int x, int z) => x >= 0 && z >= 0 && x < w && z < h && !blocked[z * w + x];

    void ToCell(Vector3 p, out int x, out int z)
    {
        x = Mathf.Clamp(Mathf.FloorToInt((p.x - minX) / cell), 0, w - 1);
        z = Mathf.Clamp(Mathf.FloorToInt((p.z - minZ) / cell), 0, h - 1);
    }

    bool NearestFree(ref int x, ref int z)
    {
        if (Free(x, z)) return true;
        for (int r = 1; r <= 6; r++)
            for (int dz = -r; dz <= r; dz++)
                for (int dx = -r; dx <= r; dx++)
                {
                    if (Mathf.Abs(dx) != r && Mathf.Abs(dz) != r) continue;
                    if (Free(x + dx, z + dz)) { x += dx; z += dz; return true; }
                }
        return false;
    }

    bool LineFree(int a, int b)
    {
        Vector2 pa = new Vector2(a % w, a / w), pb = new Vector2(b % w, b / w);
        int steps = Mathf.CeilToInt(Vector2.Distance(pa, pb) * 4f);
        for (int i = 1; i < steps; i++)
        {
            Vector2 p = Vector2.Lerp(pa, pb, (float)i / steps);
            if (!Free(Mathf.RoundToInt(p.x), Mathf.RoundToInt(p.y))) return false;
        }
        return true;
    }

    float Heur(int a, int b)
    {
        int dx = Mathf.Abs(a % w - b % w), dz = Mathf.Abs(a / w - b / w);
        return Mathf.Max(dx, dz) + 0.4142f * Mathf.Min(dx, dz);
    }

    public bool FindPath(Vector3 from, Vector3 to, List<Vector3> result)
    {
        result.Clear();
        ToCell(from, out int sx, out int sz);
        ToCell(to, out int tx, out int tz);
        int otx = tx, otz = tz;
        if (!NearestFree(ref sx, ref sz) || !NearestFree(ref tx, ref tz)) return false;
        bool exactTarget = otx == tx && otz == tz;

        int start = sz * w + sx, goal = tz * w + tx;
        for (int i = 0; i < gScore.Length; i++) { gScore[i] = float.MaxValue; closed[i] = false; }
        open.Clear();
        gScore[start] = 0f;
        cameFrom[start] = start;
        open.Add((Heur(start, goal), start));

        while (open.Count > 0)
        {
            var top = open.Min;
            open.Remove(top);
            int c = top.Item2;
            if (closed[c]) continue;
            closed[c] = true;
            if (c == goal) break;

            int cx = c % w, cz = c / w;
            for (int dz = -1; dz <= 1; dz++)
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (dx == 0 && dz == 0) continue;
                    int nx = cx + dx, nz = cz + dz;
                    if (!Free(nx, nz)) continue;
                    bool diag = dx != 0 && dz != 0;
                    if (diag && (!Free(cx + dx, cz) || !Free(cx, cz + dz))) continue;
                    int n = nz * w + nx;
                    if (closed[n]) continue;
                    float ng = gScore[c] + (diag ? 1.4142f : 1f);
                    if (ng < gScore[n])
                    {
                        gScore[n] = ng;
                        cameFrom[n] = c;
                        open.Add((ng + Heur(n, goal), n));
                    }
                }
        }
        if (!closed[goal]) return false;

        cells.Clear();
        for (int c = goal; c != start; c = cameFrom[c]) cells.Add(c);
        cells.Add(start);
        cells.Reverse();

        // Срезаем углы: идём к самой дальней клетке, до которой прямая видимость.
        int a = 0;
        while (a < cells.Count - 1)
        {
            int b = a + 1;
            while (b + 1 < cells.Count && LineFree(cells[a], cells[b + 1])) b++;
            result.Add(Center(cells[b]));
            a = b;
        }

        if (exactTarget)
        {
            var t = new Vector3(to.x, 0f, to.z);
            if (result.Count > 0) result[result.Count - 1] = t;
            else result.Add(t);
        }
        return true;
    }
}
