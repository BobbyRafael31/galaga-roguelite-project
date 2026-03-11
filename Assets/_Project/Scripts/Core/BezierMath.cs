using UnityEngine;

public static class BezierMath
{
    public static Vector2 GetPoint(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
    {
        t = Mathf.Clamp01(t);
        float oneMinusT = 1f - t;

        return
            oneMinusT * oneMinusT * oneMinusT * p0 +
            3f * oneMinusT * oneMinusT * t * p1 +
            3f * oneMinusT * t * t * p2 +
            t * t * t * p3;
    }

    public static Vector2 GetPoint(Vector2[] pts, float t)
    {
        int curveCount = (pts.Length - 1) / 3;
        if (curveCount <= 0) return pts[0]; // Fallback
        if (t >= 1f) return pts[pts.Length - 1];

        float tPerCurve = 1f / curveCount;
        int curveIndex = Mathf.FloorToInt(t / tPerCurve);

        float curveT = (t - (curveIndex * tPerCurve)) / tPerCurve;

        int p0 = curveIndex * 3;
        return GetPoint(pts[p0], pts[p0 + 1], pts[p0 + 2], pts[p0 + 3], curveT);
    }
}