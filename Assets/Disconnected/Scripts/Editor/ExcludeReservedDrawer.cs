using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Reflection;

/// <summary>
/// Custom property drawer for the ExcludeReserved attribute
/// Works with any enum type automatically!
/// </summary>
[CustomPropertyDrawer(typeof(ExcludeReservedAttribute))]
public class ExcludeReservedDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Only work with enum properties
        if (property.propertyType != SerializedPropertyType.Enum)
        {
            EditorGUI.PropertyField(position, property, label);
            return;
        }
        
        // Find the registry - now it's automatic!
        var registry = EnumReservationRegistry.Instance;
        
        if (registry == null)
        {
            // Fallback to normal enum popup if registry not found
            EditorGUI.PropertyField(position, property, label);
            var warningRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, 
                position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.HelpBox(warningRect, "EnumReservationRegistry not found", MessageType.Warning);
            return;
        }
        
        // Get the enum type
        var enumType = fieldInfo.FieldType;
        if (!enumType.IsEnum) return;
        
        // Get all enum values
        var allValues = Enum.GetValues(enumType);
        var availableValues = new List<Enum>();
        var availableNames = new List<string>();
        var availableIndices = new List<int>();
        
        // Filter out reserved values
        foreach (Enum enumValue in allValues)
        {
            if (!registry.IsReserved(enumValue))
            {
                availableValues.Add(enumValue);
                availableNames.Add(enumValue.ToString());
                availableIndices.Add(Convert.ToInt32(enumValue));
            }
        }
        
        // Check if current value is reserved
        var currentEnumValue = Enum.ToObject(enumType, property.enumValueIndex);
        bool isCurrentReserved = registry.IsReserved((Enum)currentEnumValue);
        
        EditorGUI.BeginProperty(position, label, property);
        
        // Show warning if current selection is reserved
        if (isCurrentReserved)
        {
            var reservedBy = registry.GetReservedBy((Enum)currentEnumValue);
            var warningRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.HelpBox(warningRect, $"RESERVED by {reservedBy}: {currentEnumValue}", MessageType.Error);
            position.y += EditorGUIUtility.singleLineHeight + 2;
        }
        
        // Find current selection index in available values
        int currentIndex = availableIndices.IndexOf(property.enumValueIndex);
        if (currentIndex == -1) currentIndex = 0;
        
        // Draw the filtered popup
        if (availableNames.Count > 0)
        {
            int newIndex = EditorGUI.Popup(position, label.text, currentIndex, availableNames.ToArray());
            if (newIndex >= 0 && newIndex < availableIndices.Count)
            {
                property.enumValueIndex = availableIndices[newIndex];
            }
        }
        else
        {
            EditorGUI.LabelField(position, label.text, "No available values");
        }
        
        EditorGUI.EndProperty();
    }
    
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var registry = EnumReservationRegistry.Instance;
        
        float height = EditorGUIUtility.singleLineHeight;
        
        // Add height for warnings
        if (registry == null)
        {
            height += EditorGUIUtility.singleLineHeight + 2;
        }
        else if (property.propertyType == SerializedPropertyType.Enum)
        {
            var enumType = fieldInfo.FieldType;
            if (enumType.IsEnum)
            {
                var currentEnumValue = Enum.ToObject(enumType, property.enumValueIndex);
                if (registry.IsReserved((Enum)currentEnumValue))
                {
                    height += EditorGUIUtility.singleLineHeight + 2;
                }
            }
        }
        
        return height;
    }
}
