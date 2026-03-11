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

    public override void OnInspectorGUI()
    {

        DrawDefaultInspector();

        PathData pathData = (PathData)target;
        if (pathData.Type != PathType.CustomBezier) return;

        GUILayout.Space(15);
        GUILayout.Label("Path Utility Tools", EditorStyles.boldLabel);

        // Tool: Flip X (Mirrors the path left/right)
        if (GUILayout.Button("Mirror Horizontal (Flip X)", GUILayout.Height(25)))
        {
            Undo.RecordObject(pathData, "Mirror Path X");
            for (int i = 0; i < pathData.BezierPoints.Length; i++)
            {
                pathData.BezierPoints[i] = new Vector2(-pathData.BezierPoints[i].x, pathData.BezierPoints[i].y);
            }
            pathData.BakePath();
            EditorUtility.SetDirty(pathData);
            SceneView.RepaintAll();
        }

        // Tool: Flip Y (Mirrors the path up/down)
        if (GUILayout.Button("Mirror Vertical (Flip Y)", GUILayout.Height(25)))
        {
            Undo.RecordObject(pathData, "Mirror Path Y");
            for (int i = 0; i < pathData.BezierPoints.Length; i++)
            {
                pathData.BezierPoints[i] = new Vector2(pathData.BezierPoints[i].x, -pathData.BezierPoints[i].y);
            }
            pathData.BakePath();
            EditorUtility.SetDirty(pathData);
            SceneView.RepaintAll();
        }

        // Tool: Reverse Path (Swaps Start and End, makes enemy fly backwards)
        if (GUILayout.Button("Reverse Path Direction", GUILayout.Height(25)))
        {
            Undo.RecordObject(pathData, "Reverse Path");
            System.Array.Reverse(pathData.BezierPoints);
            pathData.BakePath();
            EditorUtility.SetDirty(pathData);
            SceneView.RepaintAll();
        }
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

        Vector2 centroid = Vector2.zero;
        for (int i = 0; i < pathData.BezierPoints.Length; i++)
        {
            centroid += pathData.BezierPoints[i];
        }
        centroid /= pathData.BezierPoints.Length;

        Handles.color = Color.yellow;
        Vector2 newCentroid = Handles.PositionHandle(centroid, Quaternion.identity);
        Handles.Label(centroid + new Vector2(0.5f, 0.5f), "DRAG TO MOVE ALL");

        Handles.color = Color.white;
        for (int i = 0; i < pathData.BezierPoints.Length; i++)
        {
            Handles.DrawSolidDisc(pathData.BezierPoints[i], Vector3.forward, 0.1f);
            pathData.BezierPoints[i] = Handles.PositionHandle(pathData.BezierPoints[i], Quaternion.identity);
        }

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(pathData, "Modify Bezier Spline");

            Vector2 delta = newCentroid - centroid;
            if (delta.sqrMagnitude > 0.0001f)
            {
                for (int i = 0; i < pathData.BezierPoints.Length; i++)
                {
                    pathData.BezierPoints[i] += delta;
                }
            }

            pathData.BakePath();
            EditorUtility.SetDirty(pathData);
            sceneView.Repaint();
        }
    }
}