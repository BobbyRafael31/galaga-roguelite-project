using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(WaveData))]
public class WaveDataEditor : Editor
{
    private int _selectedBatchIndex = 0;

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

    private Color GetBatchColor(int batchIndex, float saturation)
    {
        return Color.HSVToRGB((batchIndex * 0.25f) % 1f, saturation, 1f);
    }

    public override void OnInspectorGUI()
    {
        WaveData wave = (WaveData)target;
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("Formation"));
        EditorGUILayout.Space(10);

        GUILayout.Label("Wave Batches", EditorStyles.boldLabel);
        SerializedProperty batchesProp = serializedObject.FindProperty("Batches");

        for (int i = 0; i < batchesProp.arraySize; i++)
        {
            EditorGUILayout.BeginVertical("box");
            SerializedProperty batchProp = batchesProp.GetArrayElementAtIndex(i);

            EditorGUILayout.BeginHorizontal();

            GUI.contentColor = GetBatchColor(i, 0.5f);
            batchProp.isExpanded = EditorGUILayout.Foldout(batchProp.isExpanded, $"Batch {i}", true);
            GUI.contentColor = Color.white;

            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                batchesProp.DeleteArrayElementAtIndex(i);
                break;
            }
            EditorGUILayout.EndHorizontal();

            if (batchProp.isExpanded)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(batchProp.FindPropertyRelative("EnemyPrefab"));
                EditorGUILayout.PropertyField(batchProp.FindPropertyRelative("EntrancePath"));
                EditorGUILayout.PropertyField(batchProp.FindPropertyRelative("WaveStartTime"));
                EditorGUILayout.PropertyField(batchProp.FindPropertyRelative("SpawnDelay"));
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();
        }

        if (GUILayout.Button("+ Add New Empty Batch", GUILayout.Height(30)))
        {
            wave.Batches.Add(new BatchData());
            EditorUtility.SetDirty(wave);
        }

        serializedObject.ApplyModifiedProperties();

        if (wave.Formation == null || wave.Batches.Count == 0) return;

        EditorGUILayout.Space(15);
        GUILayout.Label("Visual Seat Assigner", EditorStyles.boldLabel);

        string[] batchOptions = new string[wave.Batches.Count];
        for (int i = 0; i < wave.Batches.Count; i++) batchOptions[i] = $"Editing: Batch {i}";

        _selectedBatchIndex = EditorGUILayout.Popup(_selectedBatchIndex, batchOptions);
        _selectedBatchIndex = Mathf.Clamp(_selectedBatchIndex, 0, wave.Batches.Count - 1);

        BatchData activeBatch = wave.Batches[_selectedBatchIndex];
        FormationData form = wave.Formation;
        form.ValidateGridSize();

        EditorGUI.BeginChangeCheck();

        for (int r = form.Rows - 1; r >= 0; r--)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            for (int c = 0; c < form.Columns; c++)
            {
                bool isValidSeat = form.LayoutMask[r].Cells[c];
                Vector2Int seatPos = new Vector2Int(c, r);

                if (!isValidSeat)
                {
                    GUI.enabled = false;
                    GUILayout.Button("", GUILayout.Width(25), GUILayout.Height(25));
                    GUI.enabled = true;
                }
                else
                {
                    int ownerBatchIndex = -1;
                    for (int b = 0; b < wave.Batches.Count; b++)
                    {
                        if (wave.Batches[b].TargetSeats.Contains(seatPos))
                        {
                            ownerBatchIndex = b;
                            break;
                        }
                    }


                    if (ownerBatchIndex == _selectedBatchIndex)
                    {
                        GUI.backgroundColor = GetBatchColor(_selectedBatchIndex, 0.8f);
                    }
                    else if (ownerBatchIndex != -1)
                    {
                        GUI.backgroundColor = GetBatchColor(ownerBatchIndex, 0.3f);
                    }
                    else
                    {
                        GUI.backgroundColor = Color.white;
                    }

                    bool isOwnedByActive = (ownerBatchIndex == _selectedBatchIndex);
                    bool toggled = GUILayout.Toggle(isOwnedByActive, "", "Button", GUILayout.Width(25), GUILayout.Height(25));

                    GUI.backgroundColor = Color.white;

                    if (toggled && !isOwnedByActive)
                    {
                        activeBatch.TargetSeats.Add(seatPos);
                        if (ownerBatchIndex != -1) wave.Batches[ownerBatchIndex].TargetSeats.Remove(seatPos);
                    }
                    else if (!toggled && isOwnedByActive)
                    {
                        activeBatch.TargetSeats.Remove(seatPos);
                    }
                }
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(wave);
        }
    }

    private void OnCustomSceneGUI(SceneView sceneView)
    {
        if (Selection.activeObject != target) return;

        WaveData wave = (WaveData)target;
        if (wave.Formation == null) return;

        FormationManager fm = FindFirstObjectByType<FormationManager>();
        Vector3 gridCenter = fm != null ? fm.transform.position : Vector3.zero;
        float simTime = (float)EditorApplication.timeSinceStartup;

        Handles.color = Color.yellow;
        for (int r = 0; r < wave.Formation.Rows; r++)
        {
            for (int c = 0; c < wave.Formation.Columns; c++)
            {
                if (wave.Formation.LayoutMask[r].Cells[c])
                {
                    Vector3 livePos = wave.Formation.GetLiveSeatPosition(gridCenter, r, c, simTime);
                    Handles.DrawWireCube(livePos, new Vector3(wave.Formation.CellWidth * 0.8f, wave.Formation.CellHeight * 0.8f, 0));
                }
            }
        }

        if (wave.Batches == null) return;

        GUIStyle labelStyle = new GUIStyle();
        labelStyle.normal.textColor = Color.white;
        labelStyle.fontSize = 12;
        labelStyle.alignment = TextAnchor.MiddleCenter;

        for (int b = 0; b < wave.Batches.Count; b++)
        {
            BatchData batch = wave.Batches[b];
            if (batch.TargetSeats == null || batch.TargetSeats.Count == 0) continue;

            Handles.color = GetBatchColor(b, 1f);

            for (int i = 0; i < batch.TargetSeats.Count; i++)
            {
                Vector2Int seat = batch.TargetSeats[i];
                if (seat.y < wave.Formation.Rows && seat.x < wave.Formation.Columns && wave.Formation.LayoutMask[seat.y].Cells[seat.x])
                {
                    Vector3 seatWorldPos = wave.Formation.GetLiveSeatPosition(gridCenter, seat.y, seat.x, simTime);
                    Handles.DrawSolidDisc(seatWorldPos, Vector3.forward, 0.25f);
                    Handles.Label(seatWorldPos + Vector3.up * 0.4f, $"B{b}", labelStyle);
                }
            }
        }
    }
}