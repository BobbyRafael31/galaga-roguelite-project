using System;
using UnityEngine;

public enum SwayType
{
    None,
    Horizontal,
    ExpandContract,
    Figure8
}

[Serializable]
public class GridRow
{
    public bool[] Cells = new bool[0];
}

[CreateAssetMenu(fileName = "Formation_", menuName = "Project/Enemy Data/Formation Data")]
public class FormationData : ScriptableObject
{
    [Header("Grid Layout")]
    public int Rows = 5;
    public int Columns = 10;
    public float CellWidth = 1.2f;
    public float CellHeight = 1.0f;

    [Header("Sway Settings")]
    public SwayType Type = SwayType.Horizontal;
    public float SwaySpeed = 2f;
    public float SwayAmplitude = 2f;

    [HideInInspector]
    public GridRow[] LayoutMask = new GridRow[0];

    public void ValidateGridSize()
    {
        Rows = Mathf.Max(1, Rows);
        Columns = Mathf.Max(1, Columns);

        if (LayoutMask.Length != Rows)
        {
            Array.Resize(ref LayoutMask, Rows);
        }

        for (int r = 0; r < Rows; r++)
        {
            if (LayoutMask[r] == null) LayoutMask[r] = new GridRow();

            if (LayoutMask[r].Cells.Length != Columns)
            {
                Array.Resize(ref LayoutMask[r].Cells, Columns);
            }
        }
    }

    public Vector3 GetLiveSeatPosition(Vector3 gridCenter, int row, int col, float time)
    {
        float offsetX = (col - (Columns - 1) / 2f) * CellWidth;
        float offsetY = row * CellHeight;

        Vector3 basePosition = gridCenter + new Vector3(offsetX, offsetY, 0);

        switch (Type)
        {
            case SwayType.Horizontal:
                float hOffset = Mathf.Sin(time * SwaySpeed) * SwayAmplitude;
                return basePosition + new Vector3(hOffset, 0, 0);

            case SwayType.ExpandContract:
                float expansionMult = 1f + (Mathf.Sin(time * SwaySpeed) * (SwayAmplitude * 0.25f));
                Vector3 expandedOffset = new Vector3(offsetX * expansionMult, offsetY * expansionMult, 0);
                return gridCenter + expandedOffset;

            case SwayType.Figure8:
                float f8X = Mathf.Sin(time * SwaySpeed) * SwayAmplitude;
                float f8Y = Mathf.Sin(time * SwaySpeed * 2f) * (SwayAmplitude * 0.5f);
                return basePosition + new Vector3(f8X, f8Y, 0);

            default:
                return basePosition;
        }
    }
}