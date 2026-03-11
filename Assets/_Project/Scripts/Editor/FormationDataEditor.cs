using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FormationData))]
public class FormationDataEditor : Editor
{
    private void OnEnable()
    {
        SceneView.duringSceneGui -= OnCustomSceneGUI;
        SceneView.duringSceneGui += OnCustomSceneGUI;
        EditorApplication.update += ForceRepaint;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnCustomSceneGUI;
        EditorApplication.update -= ForceRepaint;
    }

    private void ForceRepaint()
    {
        if (Selection.activeObject == target) SceneView.RepaintAll();
    }

    public override void OnInspectorGUI()
    {
        FormationData data = (FormationData)target;

        EditorGUI.BeginChangeCheck();

        data.Rows = EditorGUILayout.IntField("Rows", data.Rows);
        data.Columns = EditorGUILayout.IntField("Columns", data.Columns);
        data.CellWidth = EditorGUILayout.FloatField("Cell Width", data.CellWidth);
        data.CellHeight = EditorGUILayout.FloatField("Cell Height", data.CellHeight);

        EditorGUILayout.Space();
        data.Type = (SwayType)EditorGUILayout.EnumPopup("Sway Type", data.Type);
        data.SwaySpeed = EditorGUILayout.FloatField("Sway Speed", data.SwaySpeed);
        data.SwayAmplitude = EditorGUILayout.FloatField("Sway Amplitude", data.SwayAmplitude);

        data.ValidateGridSize();

        EditorGUILayout.Space(15);
        GUILayout.Label("Formation Shape (Click to Toggle Seats)", EditorStyles.boldLabel);

        for (int r = data.Rows - 1; r >= 0; r--)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            for (int c = 0; c < data.Columns; c++)
            {
                data.LayoutMask[r].Cells[c] = GUILayout.Toggle(
                    data.LayoutMask[r].Cells[c],
                    "",
                    "Button",
                    GUILayout.Width(25),
                    GUILayout.Height(25)
                );
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(data);
        }
    }

    private void OnCustomSceneGUI(SceneView sceneView)
    {
        if (Selection.activeObject != target) return;

        FormationData data = (FormationData)target;
        if (data == null) return;

        data.ValidateGridSize();

        Vector3 gridCenter = Vector3.zero;
        FormationManager fm = FindFirstObjectByType<FormationManager>();
        if (fm != null) gridCenter = fm.transform.position;

        Handles.color = Color.yellow;
        float simTime = (float)EditorApplication.timeSinceStartup;

        for (int r = 0; r < data.Rows; r++)
        {
            for (int c = 0; c < data.Columns; c++)
            {
                if (data.LayoutMask[r].Cells[c])
                {
                    Vector3 livePos = data.GetLiveSeatPosition(gridCenter, r, c, simTime);
                    Handles.DrawWireCube(livePos, new Vector3(data.CellWidth * 0.8f, data.CellHeight * 0.8f, 0));
                }
            }
        }
    }
}