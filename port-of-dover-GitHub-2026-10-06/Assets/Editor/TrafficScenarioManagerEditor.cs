using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TrafficScenarioManager))]
public class TrafficScenarioManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var manager = (TrafficScenarioManager)target;
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Runtime State (read only)", EditorStyles.boldLabel);
        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.EnumPopup("State", manager.State);
            EditorGUILayout.TextField("Locked Scenario", manager.State == TrafficScenarioState.Idle ? "—" : manager.CurrentScenario.ToString());
            EditorGUILayout.IntField("Spawned", manager.SpawnedVehicleCount);
            EditorGUILayout.IntField("Active", manager.ActiveVehicleCount);
            EditorGUILayout.IntField("Completed", manager.CompletedVehicleCount);
            EditorGUILayout.FloatField("Elapsed Seconds", manager.ElapsedTime);
        }
        if (Application.isPlaying) Repaint();
    }
}
