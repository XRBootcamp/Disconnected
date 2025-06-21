
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
#endif


/// <summary>
/// Generic reservation tracker - stores what enum values are "taken" by specific objects
/// </summary>
[System.Serializable]
public class EnumReservation
{
    public string reservedBy;        // Who reserved it (e.g., "Narrator", "FinalBoss")
    public string enumTypeName;      // Which enum type (e.g., "AIVoice", "WeaponType") 
    public int enumValue;            // The actual enum value as int
    public string enumValueName;     // For display purposes
    
    public EnumReservation(string reservedBy, Enum enumValue)
    {
        this.reservedBy = reservedBy;
        this.enumTypeName = enumValue.GetType().Name;
        this.enumValue = Convert.ToInt32(enumValue);
        this.enumValueName = enumValue.ToString();
    }
}

/// <summary>
/// Central registry of all enum reservations in your game
/// </summary>
[CreateAssetMenu(fileName = "EnumReservationRegistry", menuName = "Game/Enum Reservation Registry")]
public class EnumReservationRegistry : ScriptableObject
{
    private static EnumReservationRegistry _instance;
    
    /// <summary>
    /// Global access to the registry - no need to drag references!
    /// </summary>
    public static EnumReservationRegistry Instance
    {
        get
        {
            if (_instance == null)
            {
                // Try to find existing registry in Resources
                _instance = Resources.Load<EnumReservationRegistry>("EnumReservationRegistry");
                
                #if UNITY_EDITOR
                // In editor, also search project assets
                if (_instance == null)
                {
                    string[] guids = UnityEditor.AssetDatabase.FindAssets("t:EnumReservationRegistry");
                    if (guids.Length > 0)
                    {
                        string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                        _instance = UnityEditor.AssetDatabase.LoadAssetAtPath<EnumReservationRegistry>(path);
                    }
                }
                #endif
                
                // Create one if none exists (shouldn't happen in normal usage)
                if (_instance == null)
                {
                    _instance = CreateInstance<EnumReservationRegistry>();
                    #if UNITY_EDITOR
                    UnityEditor.AssetDatabase.CreateAsset(_instance, "Assets/Resources/EnumReservationRegistry.asset");
                    #endif
                    Debug.LogWarning("EnumReservationRegistry not found, created a new one. Consider placing it in a Resources folder.");
                }
            }
            return _instance;
        }
    }

    [Header("Current Reservations")]
    [SerializeField] private List<EnumReservation> reservations = new List<EnumReservation>();
    
    /// <summary>
    /// Reserve an enum value for a specific purpose
    /// </summary>
    public void ReserveValue(string reservedBy, Enum enumValue)
    {
        // Remove any existing reservation by the same entity for the same enum type
        string enumTypeName = enumValue.GetType().Name;
        reservations.RemoveAll(r => r.reservedBy == reservedBy && r.enumTypeName == enumTypeName);
        
        // Add new reservation
        reservations.Add(new EnumReservation(reservedBy, enumValue));
        
        #if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        #endif
    }
    
    /// <summary>
    /// Check if a specific enum value is reserved
    /// </summary>
    public bool IsReserved(Enum enumValue)
    {
        string enumTypeName = enumValue.GetType().Name;
        int enumIntValue = Convert.ToInt32(enumValue);
        
        return reservations.Any(r => r.enumTypeName == enumTypeName && r.enumValue == enumIntValue);
    }
    
    /// <summary>
    /// Get who reserved a specific enum value
    /// </summary>
    public string GetReservedBy(Enum enumValue)
    {
        string enumTypeName = enumValue.GetType().Name;
        int enumIntValue = Convert.ToInt32(enumValue);
        
        var reservation = reservations.FirstOrDefault(r => r.enumTypeName == enumTypeName && r.enumValue == enumIntValue);
        return reservation?.reservedBy;
    }
    
    /// <summary>
    /// Get all available (non-reserved) values for an enum type
    /// </summary>
    public T[] GetAvailableValues<T>() where T : Enum
    {
        var allValues = Enum.GetValues(typeof(T)).Cast<T>();
        return allValues.Where(value => !IsReserved(value)).ToArray();
    }
    
    /// <summary>
    /// Get all reserved values for an enum type
    /// </summary>
    public T[] GetReservedValues<T>() where T : Enum
    {
        var allValues = Enum.GetValues(typeof(T)).Cast<T>();
        return allValues.Where(value => IsReserved(value)).ToArray();
    }
    
    /// <summary>
    /// Clear all reservations by a specific entity
    /// </summary>
    public void ClearReservationsBy(string reservedBy)
    {
        reservations.RemoveAll(r => r.reservedBy == reservedBy);
        
        #if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        #endif
    }

    public void ClearAllReservations()
    {
        reservations.Clear();
        #if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        #endif
    }
    
    /// <summary>
    /// Get all reservations for debugging
    /// </summary>
    public EnumReservation[] GetAllReservations()
    {
        return reservations.ToArray();
    }
}
