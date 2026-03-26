using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UpgradeData), true)]
public class UpgradeDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("UI Presentation", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("UpgradeName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Description"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("CoinIcon"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Tier"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Economy & Limits", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("BaseCost"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("MaxLevel"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("CostScalingPerLevel"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("BaseRNGWeight"));

        EditorGUILayout.Space();

        if (target.GetType() == typeof(UpgradeData))
        {
            EditorGUILayout.LabelField("Numerical Buff (S-Tier)", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("TargetStat"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("StatModifierValue"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ModifierType"));
        }
        else if (target.GetType() == typeof(MechanicalUpgradeData))
        {
            EditorGUILayout.LabelField("Mechanical Buff (SS-Tier)", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Mechanic"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("PowerPerLevel"));
        }
        else if (target.GetType() == typeof(CursedUpgradeData))
        {
            EditorGUILayout.LabelField("Mechanical Game-Changer (SSS-Tier)", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Mechanic"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Stat Debuffs / Curses", EditorStyles.boldLabel);

            // The 'true' parameter forces Unity to draw the List array elements and the +/- buttons.
            SerializedProperty cursesProp = serializedObject.FindProperty("Curses");
            if (cursesProp != null)
            {
                EditorGUILayout.PropertyField(cursesProp, true);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}