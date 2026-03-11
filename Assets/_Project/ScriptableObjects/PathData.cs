using UnityEngine;
using System;

public enum PathType
{
    CustomBezier,
    SineWave,
    Parabola
}
[CreateAssetMenu(fileName = "New Path", menuName = "Project/Enemy Data/Path Data")]
public class PathData : ScriptableObject
{
    public PathType Type = PathType.CustomBezier;

    [Header("Custom Bezier Settings")]
    [Tooltip("Must be 4, 7, 10, 13 points, etc. (+3 for each new segment)")]
    public Vector2[] BezierPoints = new Vector2[4]
    {
        new Vector2(-5, 5),
        new Vector2(-2, 2),
        new Vector2(2, 2),
        new Vector2(5, 5)
    };

    [Header("Mathematical Settings")]
    public Vector2 MathStartPoint = new Vector2(-5, 5);
    public Vector2 MathEndPoint = new Vector2(5, 0);
    public float Amplitude = 2f;
    public float Frequency = 1f;

    [Header("Baking Settings")]
    [Range(10, 200)] public int Resolution = 50;

    [HideInInspector]
    public Vector2[] BakedPath;

    private void OnValidate()
    {
        EnforceBezierArraySize();
        BakePath();
    }

    private void EnforceBezierArraySize()
    {
        if (BezierPoints == null || BezierPoints.Length < 4)
        {
            Array.Resize(ref BezierPoints, 4);
        }
        else if ((BezierPoints.Length - 1) % 3 != 0)
        {
            int validLength = Mathf.CeilToInt((BezierPoints.Length - 1) / 3f) * 3 + 1;
            Array.Resize(ref BezierPoints, validLength);
        }
    }

    public void BakePath()
    {
        BakedPath = new Vector2[Resolution];

        for (int i = 0; i < Resolution; i++)
        {
            float t = i / (float)(Resolution - 1);

            switch (Type)
            {
                case PathType.CustomBezier:
                    BakedPath[i] = BezierMath.GetPoint(BezierPoints, t);
                    break;

                case PathType.SineWave:
                    float lerpX = Mathf.Lerp(MathStartPoint.x, MathEndPoint.x, t);
                    float lerpY = Mathf.Lerp(MathStartPoint.y, MathEndPoint.y, t);
                    float sineOffset = Mathf.Sin(t * Mathf.PI * 2f * Frequency) * Amplitude;
                    BakedPath[i] = new Vector2(lerpX + sineOffset, lerpY);
                    break;

                case PathType.Parabola:
                    float pX = Mathf.Lerp(MathStartPoint.x, MathEndPoint.x, t);
                    float pY = Mathf.Lerp(MathStartPoint.y, MathEndPoint.y, t);
                    float parabolaOffset = (4f * Amplitude) * t * (1f - t);
                    BakedPath[i] = new Vector2(pX, pY + parabolaOffset);
                    break;
            }
        }
    }
}