namespace HorrorRPG.Editor
{
    using HorrorRPG.Battle;
    using HorrorRPG.Inventory;
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
        private SerializedProperty maxLevelProp;
        private SerializedProperty damageBonusPerLevelProp;
        private SerializedProperty markerSpeedProp;
        private SerializedProperty criticalZoneCenterProp;
        private SerializedProperty criticalZoneWidthProp;
        private SerializedProperty hitZoneWidthProp;
        private SerializedProperty criticalMultiplierProp;
        private bool showDamagePreview = true;
        private bool showTimingPreview = true;

        private void OnEnable()
        {
            itemNameProp = serializedObject.FindProperty("itemNameValue");
            descriptionProp = serializedObject.FindProperty("descriptionValue");
            iconProp = serializedObject.FindProperty("iconValue");
            categoryProp = serializedObject.FindProperty("categoryValue");
            maxStackSizeProp = serializedObject.FindProperty("maxStackSizeValue");
            disposableProp = serializedObject.FindProperty("disposableValue");
            baseDamageProp = serializedObject.FindProperty("baseDamageValue");
            effectiveAgainstProp = serializedObject.FindProperty("effectiveAgainstValue");
            effectivenessMultiplierProp = serializedObject.FindProperty("effectivenessMultiplierValue");
            maxLevelProp = serializedObject.FindProperty("maxLevelValue");
            damageBonusPerLevelProp = serializedObject.FindProperty("damageBonusPerLevelValue");
            markerSpeedProp = serializedObject.FindProperty("markerSpeedValue");
            criticalZoneCenterProp = serializedObject.FindProperty("criticalZoneCenterValue");
            criticalZoneWidthProp = serializedObject.FindProperty("criticalZoneWidthValue");
            hitZoneWidthProp = serializedObject.FindProperty("hitZoneWidthValue");
            criticalMultiplierProp = serializedObject.FindProperty("criticalMultiplierValue");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            WeaponData weapon = (WeaponData)target;

            EditorGUILayout.LabelField("Weapon Configuration", EditorStyles.boldLabel);
            DrawItemSection();
            EditorGUILayout.Space(8);
            DrawCombatSection();
            EditorGUILayout.Space(8);
            DrawTimingSection();
            EditorGUILayout.Space(8);
            DrawValidationSection(weapon);
            EditorGUILayout.Space(8);
            DrawDamagePreviewSection(weapon);
            EditorGUILayout.Space(8);
            DrawTimingPreviewSection(weapon);
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

        private void DrawCombatSection()
        {
            EditorGUILayout.LabelField("Combat and Level Progression", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(baseDamageProp);
            EditorGUILayout.PropertyField(effectiveAgainstProp);
            EditorGUILayout.PropertyField(effectivenessMultiplierProp);
            EditorGUILayout.PropertyField(maxLevelProp);
            EditorGUILayout.PropertyField(damageBonusPerLevelProp);
            EditorGUI.indentLevel--;
        }

        private void DrawTimingSection()
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
            EditorGUILayout.LabelField("Validation", EditorStyles.boldLabel);
            bool hasIssues = weapon.baseDamage < 1 || weapon.maxLevel < 1 || weapon.damageBonusPerLevel < 0f || weapon.icon == null || string.IsNullOrWhiteSpace(weapon.itemName);
            if (weapon.effectiveAgainst == EnemyType.None && weapon.effectivenessMultiplier != 1f)
                EditorGUILayout.HelpBox("effectivenessMultiplier has no target because effectiveAgainst is None.", MessageType.Warning);
            if (weapon.icon == null) EditorGUILayout.HelpBox("Weapon has no icon assigned.", MessageType.Warning);
            if (string.IsNullOrWhiteSpace(weapon.itemName)) EditorGUILayout.HelpBox("Weapon has no name.", MessageType.Error);
            EditorGUILayout.HelpBox(hasIssues ? "Review the highlighted weapon configuration." : "All validations passed.", hasIssues ? MessageType.Warning : MessageType.Info);
        }

        private void DrawDamagePreviewSection(WeaponData weapon)
        {
            showDamagePreview = EditorGUILayout.Foldout(showDamagePreview, "Damage Preview", true, EditorStyles.foldoutHeader);
            if (!showDamagePreview) return;
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField($"Level 1: {weapon.GetBaseDamageAtLevel(1)} base damage");
            EditorGUILayout.LabelField($"Level {weapon.maxLevel}: {weapon.GetBaseDamageAtLevel(weapon.maxLevel)} base damage");
            DrawDamageRow("None", weapon.GetEffectiveDamage(EnemyType.None, 1), weapon.GetEffectiveDamage(EnemyType.None, weapon.maxLevel), weapon.effectiveAgainst == EnemyType.None);
            DrawDamageRow("Demon", weapon.GetEffectiveDamage(EnemyType.Demon, 1), weapon.GetEffectiveDamage(EnemyType.Demon, weapon.maxLevel), weapon.effectiveAgainst == EnemyType.Demon);
            DrawDamageRow("Ghost", weapon.GetEffectiveDamage(EnemyType.Ghost, 1), weapon.GetEffectiveDamage(EnemyType.Ghost, weapon.maxLevel), weapon.effectiveAgainst == EnemyType.Ghost);
            DrawDamageRow("Zombie", weapon.GetEffectiveDamage(EnemyType.Zombie, 1), weapon.GetEffectiveDamage(EnemyType.Zombie, weapon.maxLevel), weapon.effectiveAgainst == EnemyType.Zombie);
            EditorGUI.indentLevel--;
        }

        private void DrawDamageRow(string enemyType, int levelOneDamage, int maximumLevelDamage, bool isEffective)
        {
            string suffix = isEffective ? " (EFFECTIVE)" : string.Empty;
            EditorGUILayout.LabelField($"vs {enemyType}: LVL1 {levelOneDamage} / MAX {maximumLevelDamage}{suffix}");
        }

        private void DrawTimingPreviewSection(WeaponData weapon)
        {
            showTimingPreview = EditorGUILayout.Foldout(showTimingPreview, "Timing Bar Preview", true, EditorStyles.foldoutHeader);
            if (!showTimingPreview) return;
            EditorGUILayout.HelpBox(
                $"Marker: {weapon.markerSpeed:F1}\n" +
                $"Critical zone: {weapon.criticalZoneWidth * 100f:F1}% at {weapon.criticalZoneCenter * 100f:F0}%\n" +
                $"Critical multiplier: {weapon.criticalMultiplier:F1}x",
                MessageType.Info);
        }
    }
}
