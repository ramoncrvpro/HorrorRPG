namespace HorrorRPG.Editor
{
using HorrorRPG.Inventory;
using HorrorRPG.Battle;


using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(WeaponDatabase))]
public class WeaponDatabaseEditor : Editor
{
    private const string WEAPONS_FOLDER = "Assets/ScriptableObjects/Itens/WeaponRelated/Weapon";

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(10);

        WeaponDatabase database = (WeaponDatabase)target;

        if (GUILayout.Button("Auto-populate from Weapons Folder", GUILayout.Height(30)))
        {
            AutoPopulateWeapons(database);
        }

        EditorGUILayout.Space(5);

        if (GUILayout.Button("Remove Null References"))
        {
            RemoveNullReferences(database);
        }

        EditorGUILayout.Space(10);
        ShowWeaponStats(database);
    }

    private void AutoPopulateWeapons(WeaponDatabase database)
    {
        string[] guids = AssetDatabase.FindAssets("t:WeaponData", new[] { WEAPONS_FOLDER });
        
        List<WeaponData> foundWeapons = new List<WeaponData>();
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            WeaponData weapon = AssetDatabase.LoadAssetAtPath<WeaponData>(path);
            
            if (weapon != null)
            {
                foundWeapons.Add(weapon);
            }
        }

        SerializedProperty weaponsProperty = serializedObject.FindProperty("weapons");
        
        weaponsProperty.ClearArray();
        
        foundWeapons = foundWeapons.OrderBy(w => w.itemName).ToList();
        
        for (int i = 0; i < foundWeapons.Count; i++)
        {
            weaponsProperty.InsertArrayElementAtIndex(i);
            weaponsProperty.GetArrayElementAtIndex(i).objectReferenceValue = foundWeapons[i];
        }
        
        serializedObject.ApplyModifiedProperties();
        
        EditorUtility.SetDirty(database);
        AssetDatabase.SaveAssets();
        
        Debug.Log($"WeaponDatabase atualizado com {foundWeapons.Count} armas.");
    }

    private void RemoveNullReferences(WeaponDatabase database)
    {
        SerializedProperty weaponsProperty = serializedObject.FindProperty("weapons");
        
        for (int i = weaponsProperty.arraySize - 1; i >= 0; i--)
        {
            if (weaponsProperty.GetArrayElementAtIndex(i).objectReferenceValue == null)
            {
                weaponsProperty.DeleteArrayElementAtIndex(i);
            }
        }
        
        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(database);
        
        Debug.Log("Referências nulas removidas do WeaponDatabase.");
    }

    private void ShowWeaponStats(WeaponDatabase database)
    {
        List<WeaponData> allWeapons = database.GetAllWeapons();
        
        EditorGUILayout.HelpBox($"Total de armas: {allWeapons.Count}", MessageType.Info);
        
        EditorGUILayout.LabelField("Weapon Levels:", allWeapons.Count(w => w.maxLevel > 0).ToString());
        EditorGUILayout.LabelField("Max configured level:", allWeapons.Count == 0 ? "0" : allWeapons.Max(w => w.maxLevel).ToString());
    }
}


}
