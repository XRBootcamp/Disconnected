using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Reflection;

/// <summary>
/// Custom editor for the reservation registry to show current state
/// </summary>
[CustomEditor(typeof(EnumReservationRegistry))]
public class EnumReservationRegistryEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var registry = (EnumReservationRegistry)target;
        
        EditorGUILayout.LabelField("Enum Reservation Registry", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // Show current reservations in a nice format
        var reservations = registry.GetAllReservations();
        if (reservations.Length > 0)
        {
            EditorGUILayout.LabelField("Current Reservations:", EditorStyles.boldLabel);
            foreach (var reservation in reservations.GroupBy(r => r.enumTypeName))
            {
                EditorGUILayout.LabelField($"{reservation.Key}:", EditorStyles.miniBoldLabel);
                foreach (var r in reservation)
                {
                    EditorGUILayout.LabelField($"  • {r.enumValueName} → Reserved by '{r.reservedBy}'");
                }
                EditorGUILayout.Space();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("No reservations yet. Reservations will appear here when GameSettings or other objects reserve enum values.", MessageType.Info);
        }
        
        // Show the actual serialized data (mostly for debugging)
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Raw Data (for debugging):", EditorStyles.boldLabel);
        DrawDefaultInspector();
        
        if (GUILayout.Button("Clear All Reservations"))
        {
            if (EditorUtility.DisplayDialog("Clear Reservations", "Are you sure you want to clear all reservations?", "Yes", "Cancel"))
            {
                registry.GetType().GetField("reservations", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(registry, new List<EnumReservation>());
                EditorUtility.SetDirty(registry);
            }
        }
    }
}