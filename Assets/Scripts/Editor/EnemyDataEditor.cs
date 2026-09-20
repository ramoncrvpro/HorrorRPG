namespace HorrorRPG.Editor
{
using HorrorRPG.Inventory;
using HorrorRPG.Battle;


using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(EnemyData))]
public class EnemyDataEditor : Editor
{
    private SerializedProperty enemyNameProp;
    private SerializedProperty maxHealthProp;
    private SerializedProperty baseDamageProp;
    private SerializedProperty categoryProp;
    private SerializedProperty availableAttacksProp;
    
    private bool showWeaponPreview = true;
    private bool showValidation = true;
    private bool showDifficultyCalculator = true;

    private void OnEnable()
    {
        enemyNameProp = serializedObject.FindProperty("enemyNameValue");
        maxHealthProp = serializedObject.FindProperty("maxHealthValue");
        baseDamageProp = serializedObject.FindProperty("baseDamageValue");
        categoryProp = serializedObject.FindProperty("categoryValue");
        availableAttacksProp = serializedObject.FindProperty("availableAttacksValue");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        EnemyData enemy = (EnemyData)target;
        
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Enemy Configuration", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        DrawEnemyInfoSection();
        
        EditorGUILayout.Space(10);
        DrawValidationSection(enemy);
        
        EditorGUILayout.Space(10);
        DrawWeaponEffectivenessSection(enemy);
        
        EditorGUILayout.Space(10);
        DrawDifficultyCalculatorSection(enemy);
        
        serializedObject.ApplyModifiedProperties();
    }

    private void DrawEnemyInfoSection()
    {
        EditorGUILayout.LabelField("Enemy Properties", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        
        EditorGUILayout.PropertyField(enemyNameProp);
        EditorGUILayout.PropertyField(maxHealthProp);
        EditorGUILayout.PropertyField(baseDamageProp);
        EditorGUILayout.PropertyField(categoryProp);
        
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Attack System", EditorStyles.miniBoldLabel);
        if (availableAttacksProp != null)
        {
            EditorGUILayout.PropertyField(availableAttacksProp, new GUIContent("Available Attacks"), true);
        }
        
        EditorGUI.indentLevel--;
    }

    private void DrawValidationSection(EnemyData enemy)
    {
        showValidation = EditorGUILayout.Foldout(showValidation, "Validation", true, EditorStyles.foldoutHeader);
        
        if (!showValidation) return;
        
        EditorGUI.indentLevel++;
        
        bool hasIssues = false;
        
        if (enemy.maxHealth <= 0)
        {
            EditorGUILayout.HelpBox("⚠ Max health must be greater than 0!", MessageType.Error);
            hasIssues = true;
        }
        
        if (enemy.baseDamage <= 0)
        {
            EditorGUILayout.HelpBox("⚠ Base damage must be greater than 0!", MessageType.Error);
            hasIssues = true;
        }
        
        if (string.IsNullOrEmpty(enemy.enemyName))
        {
            EditorGUILayout.HelpBox("⚠ Enemy has no name!", MessageType.Error);
            hasIssues = true;
        }
        
        if (enemy.category == EnemyType.None)
        {
            EditorGUILayout.HelpBox("⚠ Enemy category is set to None. This means no weapon will be effective against it.", MessageType.Warning);
            hasIssues = true;
        }
        
        if (!hasIssues)
        {
            EditorGUILayout.HelpBox("✓ All validations passed!", MessageType.Info);
        }
        
        EditorGUI.indentLevel--;
    }

    private void DrawWeaponEffectivenessSection(EnemyData enemy)
    {
        showWeaponPreview = EditorGUILayout.Foldout(showWeaponPreview, "Weapon Effectiveness", true, EditorStyles.foldoutHeader);
        
        if (!showWeaponPreview) return;
        
        EditorGUI.indentLevel++;
        
        string[] weaponGuids = AssetDatabase.FindAssets("t:WeaponData");
        List<WeaponData> allWeapons = new List<WeaponData>();
        
        foreach (string guid in weaponGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            WeaponData weapon = AssetDatabase.LoadAssetAtPath<WeaponData>(path);
            if (weapon != null)
            {
                allWeapons.Add(weapon);
            }
        }
        
        if (allWeapons.Count == 0)
        {
            EditorGUILayout.HelpBox("No weapons found in project.", MessageType.Info);
            EditorGUI.indentLevel--;
            return;
        }
        
        EditorGUILayout.LabelField($"Weapons Effective Against {enemy.category}:", EditorStyles.boldLabel);
        
        var effectiveWeapons = allWeapons.Where(w => w.effectiveAgainst == enemy.category).ToList();
        var normalWeapons = allWeapons.Where(w => w.effectiveAgainst != enemy.category).ToList();
        
        if (effectiveWeapons.Count > 0)
        {
            GUIStyle effectiveStyle = new GUIStyle(EditorStyles.label);
            effectiveStyle.normal.textColor = new Color(0.2f, 0.8f, 0.2f);
            effectiveStyle.fontStyle = FontStyle.Bold;
            
            foreach (var weapon in effectiveWeapons)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"★ {weapon.itemName}", effectiveStyle, GUILayout.Width(150));
                EditorGUILayout.LabelField($"{weapon.GetEffectiveDamage(enemy.category)} damage", effectiveStyle);
                EditorGUILayout.EndHorizontal();
            }
        }
        else
        {
            EditorGUILayout.HelpBox($"No weapons are specifically effective against {enemy.category}!", MessageType.Warning);
        }
        
        if (normalWeapons.Count > 0)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Other Weapons:", EditorStyles.miniLabel);
            
            foreach (var weapon in normalWeapons)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"  {weapon.itemName}", GUILayout.Width(150));
                EditorGUILayout.LabelField($"{weapon.GetEffectiveDamage(enemy.category)} damage", EditorStyles.miniLabel);
                EditorGUILayout.EndHorizontal();
            }
        }
        
        EditorGUI.indentLevel--;
    }

    private void DrawDifficultyCalculatorSection(EnemyData enemy)
    {
        showDifficultyCalculator = EditorGUILayout.Foldout(showDifficultyCalculator, "Difficulty Calculator", true, EditorStyles.foldoutHeader);
        
        if (!showDifficultyCalculator) return;
        
        EditorGUI.indentLevel++;
        
        string[] weaponGuids = AssetDatabase.FindAssets("t:WeaponData");
        List<WeaponData> allWeapons = new List<WeaponData>();
        
        foreach (string guid in weaponGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            WeaponData weapon = AssetDatabase.LoadAssetAtPath<WeaponData>(path);
            if (weapon != null)
            {
                allWeapons.Add(weapon);
            }
        }
        
        if (allWeapons.Count == 0)
        {
            EditorGUILayout.HelpBox("No weapons found in project.", MessageType.Info);
            EditorGUI.indentLevel--;
            return;
        }
        
        EditorGUILayout.LabelField($"Hits Required to Defeat {enemy.enemyName} ({enemy.maxHealth} HP):", EditorStyles.boldLabel);
        
        foreach (var weapon in allWeapons.OrderByDescending(w => w.GetEffectiveDamage(enemy.category)))
        {
            int damage = weapon.GetEffectiveDamage(enemy.category);
            int hitsRequired = Mathf.CeilToInt((float)enemy.maxHealth / damage);
            
            EditorGUILayout.BeginHorizontal();
            
            GUIStyle style = new GUIStyle(EditorStyles.label);
            if (weapon.effectiveAgainst == enemy.category)
            {
                style.normal.textColor = new Color(0.2f, 0.8f, 0.2f);
                style.fontStyle = FontStyle.Bold;
            }
            
            EditorGUILayout.LabelField($"{weapon.itemName}", style, GUILayout.Width(120));
            EditorGUILayout.LabelField($"{hitsRequired} hits", style, GUILayout.Width(60));
            EditorGUILayout.LabelField($"({damage} dmg/hit)", EditorStyles.miniLabel);
            
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUI.indentLevel--;
    }
}


}
