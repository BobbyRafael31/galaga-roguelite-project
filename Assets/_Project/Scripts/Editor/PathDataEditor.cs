using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PathData))]
public class PathDataEditor : Editor
{
    private void OnEnable()
    {
        SceneView.duringSceneGui -= OnCustomSceneGUI;
        SceneView.duringSceneGui += OnCustomSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnCustomSceneGUI;
    }

    private void OnCustomSceneGUI(SceneView sceneView)
    {
        if (Selection.activeObject != target) return;

        PathData pathData = (PathData)target;

        if (pathData == null || pathData.Type != PathType.CustomBezier) return;
        if (pathData.BezierPoints == null || pathData.BezierPoints.Length < 4) return;

        Handles.color = Color.gray;

        for (int i = 0; i < pathData.BezierPoints.Length - 1; i += 3)
        {
            Handles.DrawLine(pathData.BezierPoints[i], pathData.BezierPoints[i + 1]);
            Handles.DrawLine(pathData.BezierPoints[i + 3], pathData.BezierPoints[i + 2]);

            Handles.DrawBezier(
                pathData.BezierPoints[i],
                pathData.BezierPoints[i + 3],
                pathData.BezierPoints[i + 1],
                pathData.BezierPoints[i + 2],
                Color.cyan, null, 2f);
        }

        EditorGUI.BeginChangeCheck();

        for (int i = 0; i < pathData.BezierPoints.Length; i++)
        {
            pathData.BezierPoints[i] = Handles.PositionHandle(pathData.BezierPoints[i], Quaternion.identity);
        }

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(pathData, "Modify Bezier Spline");
            pathData.BakePath();
            EditorUtility.SetDirty(pathData);
            sceneView.Repaint();
        }
    }
}