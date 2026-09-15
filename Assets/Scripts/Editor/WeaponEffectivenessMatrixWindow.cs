namespace HorrorRPG.Editor
{
using HorrorRPG.Inventory;
using HorrorRPG.Battle;


using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class WeaponEffectivenessMatrixWindow : EditorWindow
{
    private Vector2 scrollPosition;
    private List<WeaponData> weapons;
    private List<EnemyData> enemies;
    private bool showLegend = true;
    private bool highlightExtremes = true;
    private int minDamage = int.MaxValue;
    private int maxDamage = int.MinValue;

    [MenuItem("Window/Horror RPG/Weapon Effectiveness Matrix")]
    public static void ShowWindow()
    {
        WeaponEffectivenessMatrixWindow window = GetWindow<WeaponEffectivenessMatrixWindow>("Effectiveness Matrix");
        window.minSize = new Vector2(700, 400);
        window.LoadData();
    }

    private void OnEnable()
    {
        LoadData();
    }

    private void LoadData()
    {
        weapons = new List<WeaponData>();
        enemies = new List<EnemyData>();
        
        string[] weaponGuids = AssetDatabase.FindAssets("t:WeaponData");
        foreach (string guid in weaponGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            WeaponData weapon = AssetDatabase.LoadAssetAtPath<WeaponData>(path);
            if (weapon != null)
            {
                weapons.Add(weapon);
            }
        }
        
        string[] enemyGuids = AssetDatabase.FindAssets("t:EnemyData");
        foreach (string guid in enemyGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            EnemyData enemy = AssetDatabase.LoadAssetAtPath<EnemyData>(path);
            if (enemy != null)
            {
                enemies.Add(enemy);
            }
        }
        
        weapons = weapons.OrderBy(w => w.itemName).ToList();
        enemies = enemies.OrderBy(e => e.enemyName).ToList();
        
        CalculateDamageExtremes();
    }

    private void CalculateDamageExtremes()
    {
        minDamage = int.MaxValue;
        maxDamage = int.MinValue;
        
        foreach (var weapon in weapons)
        {
            foreach (EnemyType type in System.Enum.GetValues(typeof(EnemyType)))
            {
                if (type == EnemyType.None) continue;
                
                int damage = weapon.GetEffectiveDamage(type);
                if (damage < minDamage) minDamage = damage;
                if (damage > maxDamage) maxDamage = damage;
            }
        }
    }

    private void OnGUI()
    {
        DrawHeader();
        DrawToolbar();
        
        if (weapons.Count == 0)
        {
            EditorGUILayout.HelpBox("No weapons found in the project.", MessageType.Info);
            return;
        }
        
        DrawMatrix();
        
        if (showLegend)
        {
            DrawLegend();
        }
        
        DrawAnalysis();
        DrawFooter();
    }

    private void DrawHeader()
    {
        EditorGUILayout.Space(10);
        
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 16;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        
        EditorGUILayout.LabelField("Weapon Effectiveness Matrix", titleStyle);
        EditorGUILayout.Space(5);
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        
        if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(60)))
        {
            LoadData();
        }
        
        GUILayout.FlexibleSpace();
        
        highlightExtremes = GUILayout.Toggle(highlightExtremes, "Highlight Extremes", EditorStyles.toolbarButton);
        showLegend = GUILayout.Toggle(showLegend, "Show Legend", EditorStyles.toolbarButton);
        
        if (GUILayout.Button("Export CSV", EditorStyles.toolbarButton, GUILayout.Width(80)))
        {
            ExportToCSV();
        }
        
        EditorGUILayout.EndHorizontal();
    }

    private void DrawMatrix()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        DrawMatrixHeader();
        
        foreach (var weapon in weapons)
        {
            DrawWeaponRow(weapon);
        }
        
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.EndScrollView();
    }

    private void DrawMatrixHeader()
    {
        EditorGUILayout.BeginHorizontal();
        
        GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
        headerStyle.alignment = TextAnchor.MiddleCenter;
        
        GUILayout.Label("Weapon", headerStyle, GUILayout.Width(150));
        GUILayout.Label("Demon", headerStyle, GUILayout.Width(80));
        GUILayout.Label("Ghost", headerStyle, GUILayout.Width(80));
        GUILayout.Label("Zombie", headerStyle, GUILayout.Width(80));
        GUILayout.Label("Notes", headerStyle, GUILayout.Width(200));
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(3);
        DrawSeparator();
    }

    private void DrawWeaponRow(WeaponData weapon)
    {
        EditorGUILayout.BeginHorizontal();
        
        GUIStyle nameStyle = new GUIStyle(EditorStyles.label);
        nameStyle.fontStyle = FontStyle.Bold;
        
        if (GUILayout.Button(weapon.itemName, nameStyle, GUILayout.Width(150)))
        {
            Selection.activeObject = weapon;
            EditorGUIUtility.PingObject(weapon);
        }
        
        int demonDamage = weapon.GetEffectiveDamage(EnemyType.Demon);
        int ghostDamage = weapon.GetEffectiveDamage(EnemyType.Ghost);
        int zombieDamage = weapon.GetEffectiveDamage(EnemyType.Zombie);
        
        DrawDamageCell(demonDamage, weapon.effectiveAgainst == EnemyType.Demon);
        DrawDamageCell(ghostDamage, weapon.effectiveAgainst == EnemyType.Ghost);
        DrawDamageCell(zombieDamage, weapon.effectiveAgainst == EnemyType.Zombie);
        
        string notes = GetWeaponNotes(weapon);
        GUILayout.Label(notes, EditorStyles.miniLabel, GUILayout.Width(200));
        
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(2);
    }

    private void DrawDamageCell(int damage, bool isEffective)
    {
        GUIStyle cellStyle = new GUIStyle(EditorStyles.label);
        cellStyle.alignment = TextAnchor.MiddleCenter;
        cellStyle.fontStyle = FontStyle.Bold;
        
        Color backgroundColor = Color.white;
        
        if (isEffective)
        {
            cellStyle.normal.textColor = new Color(0.1f, 0.6f, 0.1f);
            backgroundColor = new Color(0.7f, 1f, 0.7f);
        }
        else if (highlightExtremes && weapons.Count > 1)
        {
            if (damage == maxDamage)
            {
                backgroundColor = new Color(1f, 0.9f, 0.7f);
            }
            else if (damage == minDamage)
            {
                backgroundColor = new Color(1f, 0.7f, 0.7f);
            }
        }
        
        Rect cellRect = GUILayoutUtility.GetRect(80, 20);
        
        if (backgroundColor != Color.white)
        {
            EditorGUI.DrawRect(cellRect, backgroundColor);
        }
        
        string displayText = isEffective ? $"{damage} ★" : damage.ToString();
        GUI.Label(cellRect, displayText, cellStyle);
    }

    private string GetWeaponNotes(WeaponData weapon)
    {
        List<string> notes = new List<string>();
        
        if (weapon.requiresAmmo)
        {
            notes.Add("Ammo Required");
        }
        
        if (weapon.effectiveAgainst == EnemyType.None)
        {
            notes.Add("No Effectiveness");
        }
        else
        {
            notes.Add($"x{weapon.effectivenessMultiplier} vs {weapon.effectiveAgainst}");
        }
        
        return string.Join(" | ", notes);
    }

    private void DrawLegend()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.LabelField("Legend:", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        DrawLegendItem(new Color(0.7f, 1f, 0.7f), "★ Effective (Multiplier Applied)");
        DrawLegendItem(new Color(1f, 0.9f, 0.7f), "Highest Damage");
        DrawLegendItem(new Color(1f, 0.7f, 0.7f), "Lowest Damage");
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
    }

    private void DrawLegendItem(Color color, string label)
    {
        EditorGUILayout.BeginHorizontal();
        
        Rect colorRect = GUILayoutUtility.GetRect(20, 15);
        EditorGUI.DrawRect(colorRect, color);
        
        GUILayout.Label(label, EditorStyles.miniLabel);
        
        EditorGUILayout.EndHorizontal();
    }

    private void DrawAnalysis()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.LabelField("Balance Analysis:", EditorStyles.boldLabel);
        
        Dictionary<EnemyType, List<WeaponData>> effectivenessMap = new Dictionary<EnemyType, List<WeaponData>>();
        
        foreach (EnemyType type in System.Enum.GetValues(typeof(EnemyType)))
        {
            if (type == EnemyType.None) continue;
            effectivenessMap[type] = weapons.Where(w => w.effectiveAgainst == type).ToList();
        }
        
        foreach (var kvp in effectivenessMap)
        {
            EditorGUILayout.BeginHorizontal();
            
            string label = $"vs {kvp.Key}:";
            EditorGUILayout.LabelField(label, GUILayout.Width(100));
            
            if (kvp.Value.Count == 0)
            {
                GUIStyle warningStyle = new GUIStyle(EditorStyles.label);
                warningStyle.normal.textColor = new Color(1f, 0.5f, 0f);
                EditorGUILayout.LabelField("⚠ No effective weapons!", warningStyle);
            }
            else
            {
                string weaponNames = string.Join(", ", kvp.Value.Select(w => w.itemName));
                EditorGUILayout.LabelField($"{kvp.Value.Count} weapon(s): {weaponNames}", EditorStyles.wordWrappedLabel);
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.Space(5);
        
        if (enemies.Count > 0)
        {
            EditorGUILayout.LabelField("Enemy Coverage:", EditorStyles.boldLabel);
            
            foreach (var enemy in enemies)
            {
                EditorGUILayout.BeginHorizontal();
                
                EditorGUILayout.LabelField($"{enemy.enemyName} ({enemy.category}):", GUILayout.Width(150));
                
                var effectiveWeapons = weapons.Where(w => w.effectiveAgainst == enemy.category).ToList();
                
                if (effectiveWeapons.Count == 0)
                {
                    GUIStyle warningStyle = new GUIStyle(EditorStyles.label);
                    warningStyle.normal.textColor = new Color(1f, 0.5f, 0f);
                    EditorGUILayout.LabelField("⚠ No effective weapons", warningStyle);
                }
                else
                {
                    EditorGUILayout.LabelField($"{effectiveWeapons.Count} effective weapon(s)");
                }
                
                EditorGUILayout.EndHorizontal();
            }
        }
        
        EditorGUILayout.EndVertical();
    }

    private void DrawFooter()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
        
        EditorGUILayout.LabelField($"Total Weapons: {weapons.Count} | Total Enemies: {enemies.Count}", EditorStyles.miniLabel);
        
        if (weapons.Count > 0)
        {
            EditorGUILayout.LabelField($"Damage Range: {minDamage} - {maxDamage}", EditorStyles.miniLabel);
        }
        
        EditorGUILayout.EndHorizontal();
    }

    private void DrawSeparator()
    {
        Rect rect = GUILayoutUtility.GetRect(1, 1, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(rect, Color.gray);
    }

    private void ExportToCSV()
    {
        string path = EditorUtility.SaveFilePanel("Export Weapon Effectiveness Matrix", "", "weapon_matrix.csv", "csv");
        
        if (string.IsNullOrEmpty(path))
            return;
        
        StringBuilder csv = new StringBuilder();
        
        csv.Append("Weapon,Demon,Ghost,Zombie,Effective Against,Multiplier,Type\n");
        
        foreach (var weapon in weapons)
        {
            csv.Append($"{weapon.itemName},");
            csv.Append($"{weapon.GetEffectiveDamage(EnemyType.Demon)},");
            csv.Append($"{weapon.GetEffectiveDamage(EnemyType.Ghost)},");
            csv.Append($"{weapon.GetEffectiveDamage(EnemyType.Zombie)},");
            csv.Append($"{weapon.effectiveAgainst},");
            csv.Append($"{weapon.effectivenessMultiplier},");
            csv.Append($"{(weapon.requiresAmmo ? "LIMITED" : "BASIC")}\n");
        }
        
        System.IO.File.WriteAllText(path, csv.ToString());
        
        EditorUtility.DisplayDialog("Export Complete", $"Matrix exported to:\n{path}", "OK");
    }
}


}
