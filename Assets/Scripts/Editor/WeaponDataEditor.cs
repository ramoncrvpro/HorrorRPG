namespace HorrorRPG.Editor
{
using HorrorRPG.Inventory;
using HorrorRPG.Battle;


using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WeaponData))]
public class WeaponDataEditor : Editor
{
    private SerializedProperty itemNameProp;
    private SerializedProperty descriptionProp;
    private SerializedProperty iconProp;
    private SerializedProperty categoryProp;
    private SerializedProperty maxStackSizeProp;
    private SerializedProperty disposableProp;
    
    private SerializedProperty baseDamageProp;
    private SerializedProperty effectiveAgainstProp;
    private SerializedProperty effectivenessMultiplierProp;
    private SerializedProperty ammoTypeProp;
    
    private SerializedProperty markerSpeedProp;
    private SerializedProperty criticalZoneCenterProp;
    private SerializedProperty criticalZoneWidthProp;
    private SerializedProperty hitZoneWidthProp;
    private SerializedProperty criticalMultiplierProp;
    
    private bool showDamagePreview = true;
    private bool showValidation = true;
    private bool showTimingBarPreview = true;

    private void OnEnable()
    {
        itemNameProp = serializedObject.FindProperty("itemName");
        descriptionProp = serializedObject.FindProperty("description");
        iconProp = serializedObject.FindProperty("icon");
        categoryProp = serializedObject.FindProperty("category");
        maxStackSizeProp = serializedObject.FindProperty("maxStackSize");
        disposableProp = serializedObject.FindProperty("disposable");
        
        baseDamageProp = serializedObject.FindProperty("baseDamage");
        effectiveAgainstProp = serializedObject.FindProperty("effectiveAgainst");
        effectivenessMultiplierProp = serializedObject.FindProperty("effectivenessMultiplier");
        ammoTypeProp = serializedObject.FindProperty("ammoType");
        
        markerSpeedProp = serializedObject.FindProperty("markerSpeed");
        criticalZoneCenterProp = serializedObject.FindProperty("criticalZoneCenter");
        criticalZoneWidthProp = serializedObject.FindProperty("criticalZoneWidth");
        hitZoneWidthProp = serializedObject.FindProperty("hitZoneWidth");
        criticalMultiplierProp = serializedObject.FindProperty("criticalMultiplier");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        WeaponData weapon = (WeaponData)target;
        
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Weapon Configuration", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        DrawItemSection();
        
        EditorGUILayout.Space(10);
        DrawWeaponTypeSection(weapon);
        
        EditorGUILayout.Space(10);
        DrawWeaponStatsSection();
        
        EditorGUILayout.Space(10);
        DrawTimingBarSection();
        
        EditorGUILayout.Space(10);
        DrawValidationSection(weapon);
        
        EditorGUILayout.Space(10);
        DrawDamagePreviewSection(weapon);
        
        EditorGUILayout.Space(10);
        DrawTimingBarPreviewSection(weapon);
        
        EditorGUILayout.Space(10);
        DrawTestSection(weapon);
        
        serializedObject.ApplyModifiedProperties();
    }

    private void DrawItemSection()
    {
        EditorGUILayout.LabelField("Item Properties", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        
        EditorGUILayout.PropertyField(itemNameProp);
        EditorGUILayout.PropertyField(descriptionProp);
        EditorGUILayout.PropertyField(iconProp);
        EditorGUILayout.PropertyField(categoryProp);
        EditorGUILayout.PropertyField(maxStackSizeProp);
        EditorGUILayout.PropertyField(disposableProp);
        
        EditorGUI.indentLevel--;
    }

    private void DrawWeaponTypeSection(WeaponData weapon)
    {
        EditorGUILayout.LabelField("Weapon Type", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        
        EditorGUILayout.PropertyField(ammoTypeProp);
        
        bool isLimited = weapon.requiresAmmo;
        string weaponType = isLimited ? "LIMITED (Requires Ammo)" : "BASIC (Unlimited)";
        EditorGUILayout.HelpBox($"Type: {weaponType}", MessageType.Info);
        
        EditorGUI.indentLevel--;
    }

    private void DrawWeaponStatsSection()
    {
        EditorGUILayout.LabelField("Combat Stats", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        
        EditorGUILayout.PropertyField(baseDamageProp);
        EditorGUILayout.PropertyField(effectiveAgainstProp);
        
        if (effectiveAgainstProp.enumValueIndex != 0)
        {
            EditorGUILayout.PropertyField(effectivenessMultiplierProp);
        }
        else
        {
            GUI.enabled = false;
            EditorGUILayout.PropertyField(effectivenessMultiplierProp);
            GUI.enabled = true;
        }
        
        EditorGUI.indentLevel--;
    }

    private void DrawTimingBarSection()
    {
        EditorGUILayout.LabelField("Timing Bar Settings", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        
        EditorGUILayout.PropertyField(markerSpeedProp);
        EditorGUILayout.PropertyField(criticalZoneCenterProp);
        EditorGUILayout.PropertyField(criticalZoneWidthProp);
        EditorGUILayout.PropertyField(hitZoneWidthProp);
        EditorGUILayout.PropertyField(criticalMultiplierProp);
        
        EditorGUI.indentLevel--;
    }

    private void DrawValidationSection(WeaponData weapon)
    {
        showValidation = EditorGUILayout.Foldout(showValidation, "Validation", true, EditorStyles.foldoutHeader);
        
        if (!showValidation) return;
        
        EditorGUI.indentLevel++;
        
        bool hasIssues = false;
        
        if (weapon.requiresAmmo && weapon.ammoType == null)
        {
            EditorGUILayout.HelpBox("⚠ Weapon requires ammo but ammoType is not defined!", MessageType.Error);
            hasIssues = true;
        }
        
        if (weapon.effectiveAgainst == EnemyType.None && weapon.effectivenessMultiplier != 1f)
        {
            EditorGUILayout.HelpBox("⚠ effectivenessMultiplier is not 1.0 but effectiveAgainst is None. The multiplier will have no effect.", MessageType.Warning);
            hasIssues = true;
        }
        
        if (weapon.baseDamage <= 0)
        {
            EditorGUILayout.HelpBox("⚠ Base damage must be greater than 0!", MessageType.Error);
            hasIssues = true;
        }
        
        if (weapon.icon == null)
        {
            EditorGUILayout.HelpBox("⚠ Weapon has no icon assigned.", MessageType.Warning);
            hasIssues = true;
        }
        
        if (string.IsNullOrEmpty(weapon.itemName))
        {
            EditorGUILayout.HelpBox("⚠ Weapon has no name!", MessageType.Error);
            hasIssues = true;
        }
        
        if (!hasIssues)
        {
            EditorGUILayout.HelpBox("✓ All validations passed!", MessageType.Info);
        }
        
        EditorGUI.indentLevel--;
    }

    private void DrawDamagePreviewSection(WeaponData weapon)
    {
        showDamagePreview = EditorGUILayout.Foldout(showDamagePreview, "Damage Preview", true, EditorStyles.foldoutHeader);
        
        if (!showDamagePreview) return;
        
        EditorGUI.indentLevel++;
        
        EditorGUILayout.LabelField("Damage vs Enemy Types:", EditorStyles.boldLabel);
        
        DrawDamageRow("None", weapon.GetEffectiveDamage(EnemyType.None), weapon.effectiveAgainst == EnemyType.None);
        DrawDamageRow("Demon", weapon.GetEffectiveDamage(EnemyType.Demon), weapon.effectiveAgainst == EnemyType.Demon);
        DrawDamageRow("Ghost", weapon.GetEffectiveDamage(EnemyType.Ghost), weapon.effectiveAgainst == EnemyType.Ghost);
        DrawDamageRow("Zombie", weapon.GetEffectiveDamage(EnemyType.Zombie), weapon.effectiveAgainst == EnemyType.Zombie);
        
        EditorGUI.indentLevel--;
    }
    
    private void DrawTimingBarPreviewSection(WeaponData weapon)
    {
        showTimingBarPreview = EditorGUILayout.Foldout(showTimingBarPreview, "Timing Bar Preview", true, EditorStyles.foldoutHeader);
        
        if (!showTimingBarPreview) return;
        
        EditorGUI.indentLevel++;
        
        EditorGUILayout.Space(5);
        DrawTimingBarVisual(weapon);
        
        EditorGUILayout.Space(10);
        EditorGUILayout.HelpBox(
            $"Velocidade do Marcador: {weapon.markerSpeed:F1} unidades/segundo\n" +
            $"Zona de Acerto: {weapon.hitZoneWidth * 100f:F0}% de cada lado da crítica\n" +
            $"Zona Crítica: {weapon.criticalZoneWidth * 100f:F1}% na posição {weapon.criticalZoneCenter * 100f:F0}%\n" +
            $"Multiplicador Crítico: {weapon.criticalMultiplier:F1}x",
            MessageType.Info
        );
        
        EditorGUI.indentLevel--;
    }
    
    private void DrawTimingBarVisual(WeaponData weapon)
    {
        Rect previewRect = GUILayoutUtility.GetRect(0, 60);
        previewRect.x += 20;
        previewRect.width -= 40;
        previewRect.y += 10;
        previewRect.height = 40;
        
        float critMin = weapon.criticalZoneCenter - (weapon.criticalZoneWidth / 2f);
        float critMax = weapon.criticalZoneCenter + (weapon.criticalZoneWidth / 2f);
        
        float hitLeftMin = Mathf.Max(0f, critMin - weapon.hitZoneWidth);
        float hitLeftMax = critMin;
        
        float hitRightMin = critMax;
        float hitRightMax = Mathf.Min(1f, critMax + weapon.hitZoneWidth);
        
        float totalWidth = previewRect.width;
        
        DrawZone(previewRect, 0f, hitLeftMin, Color.black, "MISS");
        DrawZone(previewRect, hitLeftMin, hitLeftMax, Color.white, "HIT");
        DrawZone(previewRect, critMin, critMax, new Color(0.3f, 0.3f, 0.3f), "CRIT");
        DrawZone(previewRect, hitRightMin, hitRightMax, Color.white, "HIT");
        DrawZone(previewRect, hitRightMax, 1f, Color.black, "MISS");
        
        Rect borderRect = new Rect(previewRect.x, previewRect.y, totalWidth, previewRect.height);
        EditorGUI.DrawRect(new Rect(borderRect.x - 1, borderRect.y - 1, borderRect.width + 2, 1), Color.gray);
        EditorGUI.DrawRect(new Rect(borderRect.x - 1, borderRect.y + borderRect.height, borderRect.width + 2, 1), Color.gray);
        EditorGUI.DrawRect(new Rect(borderRect.x - 1, borderRect.y - 1, 1, borderRect.height + 2), Color.gray);
        EditorGUI.DrawRect(new Rect(borderRect.x + borderRect.width, borderRect.y - 1, 1, borderRect.height + 2), Color.gray);
    }
    
    private void DrawZone(Rect container, float startNorm, float endNorm, Color color, string label)
    {
        float totalWidth = container.width;
        float startX = container.x + (startNorm * totalWidth);
        float width = (endNorm - startNorm) * totalWidth;
        
        Rect zoneRect = new Rect(startX, container.y, width, container.height);
        EditorGUI.DrawRect(zoneRect, color);
        
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.alignment = TextAnchor.MiddleCenter;
        labelStyle.fontSize = 9;
        labelStyle.normal.textColor = color == Color.white ? Color.black : Color.white;
        
        if (width > 40)
        {
            GUI.Label(zoneRect, label, labelStyle);
        }
    }

    private void DrawDamageRow(string enemyType, int damage, bool isEffective)
    {
        EditorGUILayout.BeginHorizontal();
        
        GUIStyle labelStyle = new GUIStyle(EditorStyles.label);
        if (isEffective)
        {
            labelStyle.normal.textColor = new Color(0.2f, 0.8f, 0.2f);
            labelStyle.fontStyle = FontStyle.Bold;
        }
        
        EditorGUILayout.LabelField($"vs {enemyType}:", labelStyle, GUILayout.Width(100));
        EditorGUILayout.LabelField($"{damage} damage", labelStyle);
        
        if (isEffective)
        {
            EditorGUILayout.LabelField("★ EFFECTIVE", labelStyle);
        }
        
        EditorGUILayout.EndHorizontal();
    }

    private void DrawTestSection(WeaponData weapon)
    {
        EditorGUILayout.LabelField("Quick Test", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Show Weapon Stats", GUILayout.Height(30)))
        {
            string stats = GenerateWeaponStats(weapon);
            EditorUtility.DisplayDialog("Weapon Statistics", stats, "OK");
        }
    }

    private string GenerateWeaponStats(WeaponData weapon)
    {
        string stats = $"=== {weapon.itemName} ===\n\n";
        stats += $"Type: {(weapon.requiresAmmo ? "LIMITED" : "BASIC")}\n";
        stats += $"Base Damage: {weapon.baseDamage}\n";
        stats += $"Effective Against: {weapon.effectiveAgainst}\n";
        stats += $"Multiplier: x{weapon.effectivenessMultiplier}\n\n";
        
        stats += "--- Damage Output ---\n";
        stats += $"vs None: {weapon.GetEffectiveDamage(EnemyType.None)}\n";
        stats += $"vs Demon: {weapon.GetEffectiveDamage(EnemyType.Demon)}\n";
        stats += $"vs Ghost: {weapon.GetEffectiveDamage(EnemyType.Ghost)}\n";
        stats += $"vs Zombie: {weapon.GetEffectiveDamage(EnemyType.Zombie)}\n";
        
        if (weapon.requiresAmmo)
        {
            stats += $"\nAmmo Type: {(weapon.ammoType != null ? weapon.ammoType.itemName : "NOT SET")}";
        }
        
        return stats;
    }
}


}
