namespace HorrorRPG.Editor
{
using HorrorRPG.Inventory;
using HorrorRPG.Battle;


using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ItemDatabaseWindow : EditorWindow
{
    private Vector2 scrollPosition;
    private string searchQuery = "";
    private FilterType currentFilter = FilterType.All;
    private ItemData selectedItem;
    private Vector2 detailScrollPosition;
    
    private enum FilterType
    {
        All,
        Items,
        Weapons,
        Ammo,
        Consumables,
        Keys
    }

    [MenuItem("Window/Horror RPG/Item Database Viewer")]
    public static void ShowWindow()
    {
        ItemDatabaseWindow window = GetWindow<ItemDatabaseWindow>("Item Database");
        window.minSize = new Vector2(600, 400);
    }

    private void OnGUI()
    {
        DrawHeader();
        DrawToolbar();
        
        EditorGUILayout.BeginHorizontal();
        
        DrawItemList();
        
        if (selectedItem != null)
        {
            DrawItemDetails();
        }
        
        EditorGUILayout.EndHorizontal();
        
        DrawFooter();
    }

    private void DrawHeader()
    {
        EditorGUILayout.Space(10);
        
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 16;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        
        EditorGUILayout.LabelField("Item & Weapon Database Viewer", titleStyle);
        EditorGUILayout.Space(5);
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        
        GUILayout.Label("Search:", GUILayout.Width(50));
        searchQuery = EditorGUILayout.TextField(searchQuery, EditorStyles.toolbarTextField, GUILayout.Width(200));
        
        GUILayout.FlexibleSpace();
        
        GUILayout.Label("Filter:", GUILayout.Width(40));
        currentFilter = (FilterType)EditorGUILayout.EnumPopup(currentFilter, EditorStyles.toolbarDropDown, GUILayout.Width(120));
        
        if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(60)))
        {
            AssetDatabase.Refresh();
        }
        
        EditorGUILayout.EndHorizontal();
    }

    private void DrawItemList()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(300));
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, EditorStyles.helpBox);
        
        List<ItemData> items = GetFilteredItems();
        
        if (items.Count == 0)
        {
            EditorGUILayout.HelpBox("No items found.", MessageType.Info);
        }
        else
        {
            foreach (var item in items)
            {
                DrawItemEntry(item);
            }
        }
        
        EditorGUILayout.EndScrollView();
        
        EditorGUILayout.EndVertical();
    }

    private void DrawItemEntry(ItemData item)
    {
        EditorGUILayout.BeginHorizontal();
        
        bool isSelected = selectedItem == item;
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.alignment = TextAnchor.MiddleLeft;
        
        if (isSelected)
        {
            buttonStyle.normal.background = Texture2D.grayTexture;
        }
        
        if (GUILayout.Button("", buttonStyle, GUILayout.Height(40)))
        {
            selectedItem = item;
        }
        
        Rect lastRect = GUILayoutUtility.GetLastRect();
        
        Rect iconRect = new Rect(lastRect.x + 5, lastRect.y + 5, 30, 30);
        if (item.icon != null)
        {
            GUI.DrawTexture(iconRect, item.icon.texture, ScaleMode.ScaleToFit);
        }
        else
        {
            EditorGUI.DrawRect(iconRect, Color.gray);
        }
        
        Rect labelRect = new Rect(lastRect.x + 40, lastRect.y + 5, lastRect.width - 100, 15);
        GUIStyle nameStyle = new GUIStyle(EditorStyles.boldLabel);
        nameStyle.fontSize = 11;
        GUI.Label(labelRect, item.itemName, nameStyle);
        
        Rect typeRect = new Rect(lastRect.x + 40, lastRect.y + 22, lastRect.width - 100, 15);
        GUI.Label(typeRect, GetItemTypeLabel(item), EditorStyles.miniLabel);
        
        Rect validationRect = new Rect(lastRect.x + lastRect.width - 50, lastRect.y + 10, 40, 20);
        DrawValidationIcon(validationRect, item);
        
        EditorGUILayout.EndHorizontal();
    }

    private string GetItemTypeLabel(ItemData item)
    {
        if (item is WeaponData weapon)
        {
            return weapon.requiresAmmo ? "Weapon (Ammo)" : "Weapon (Basic)";
        }
        return $"Item ({item.category})";
    }

    private void DrawValidationIcon(Rect rect, ItemData item)
    {
        bool hasIssues = ValidateItem(item, out string _);
        
        GUIStyle style = new GUIStyle(EditorStyles.miniLabel);
        style.alignment = TextAnchor.MiddleCenter;
        style.fontSize = 14;
        
        if (hasIssues)
        {
            style.normal.textColor = new Color(1f, 0.5f, 0f);
            GUI.Label(rect, "⚠", style);
        }
        else
        {
            style.normal.textColor = new Color(0.2f, 0.8f, 0.2f);
            GUI.Label(rect, "✓", style);
        }
    }

    private bool ValidateItem(ItemData item, out string issues)
    {
        List<string> problemList = new List<string>();
        
        if (string.IsNullOrEmpty(item.itemName))
            problemList.Add("Missing name");
        
        if (item.icon == null)
            problemList.Add("Missing icon");
        
        if (item is WeaponData weapon)
        {
            if (weapon.baseDamage <= 0)
                problemList.Add("Invalid damage");
            
            if (weapon.requiresAmmo && weapon.ammoType == null)
                problemList.Add("Missing ammo type");
            
            if (weapon.effectiveAgainst == EnemyType.None && weapon.effectivenessMultiplier != 1f)
                problemList.Add("Multiplier has no effect");
        }
        
        issues = string.Join("\n", problemList);
        return problemList.Count > 0;
    }

    private void DrawItemDetails()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(270));
        
        detailScrollPosition = EditorGUILayout.BeginScrollView(detailScrollPosition);
        
        EditorGUILayout.LabelField("Item Details", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        if (selectedItem.icon != null)
        {
            Rect iconRect = GUILayoutUtility.GetRect(80, 80, GUILayout.ExpandWidth(false));
            iconRect.x = (270 - 80) / 2;
            GUI.DrawTexture(iconRect, selectedItem.icon.texture, ScaleMode.ScaleToFit);
            EditorGUILayout.Space(5);
        }
        
        EditorGUILayout.LabelField("Name:", EditorStyles.miniLabel);
        EditorGUILayout.LabelField(selectedItem.itemName, EditorStyles.wordWrappedLabel);
        
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Description:", EditorStyles.miniLabel);
        EditorGUILayout.LabelField(selectedItem.description, EditorStyles.wordWrappedLabel);
        
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Category:", EditorStyles.miniLabel);
        EditorGUILayout.LabelField(selectedItem.category.ToString());
        
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Stack Size:", EditorStyles.miniLabel);
        EditorGUILayout.LabelField(selectedItem.maxStackSize.ToString());
        
        if (selectedItem is WeaponData weapon)
        {
            DrawWeaponSpecificDetails(weapon);
        }
        
        EditorGUILayout.Space(10);
        
        bool hasIssues = ValidateItem(selectedItem, out string issueText);
        if (hasIssues)
        {
            EditorGUILayout.HelpBox("Issues:\n" + issueText, MessageType.Warning);
        }
        else
        {
            EditorGUILayout.HelpBox("No issues found", MessageType.Info);
        }
        
        EditorGUILayout.Space(10);
        
        if (GUILayout.Button("Edit Asset", GUILayout.Height(30)))
        {
            Selection.activeObject = selectedItem;
            EditorGUIUtility.PingObject(selectedItem);
        }
        
        EditorGUILayout.EndScrollView();
        
        EditorGUILayout.EndVertical();
    }

    private void DrawWeaponSpecificDetails(WeaponData weapon)
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Weapon Stats", EditorStyles.boldLabel);
        
        EditorGUILayout.LabelField("Base Damage:", EditorStyles.miniLabel);
        EditorGUILayout.LabelField(weapon.baseDamage.ToString());
        
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Effective Against:", EditorStyles.miniLabel);
        EditorGUILayout.LabelField(weapon.effectiveAgainst.ToString());
        
        if (weapon.effectiveAgainst != EnemyType.None)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Multiplier:", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"x{weapon.effectivenessMultiplier}");
        }
        
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Type:", EditorStyles.miniLabel);
        EditorGUILayout.LabelField(weapon.requiresAmmo ? "LIMITED (Uses Ammo)" : "BASIC (Unlimited)");
        
        if (weapon.requiresAmmo)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Ammo Type:", EditorStyles.miniLabel);
            EditorGUILayout.LabelField(weapon.ammoType != null ? weapon.ammoType.itemName : "NOT SET");
        }
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Damage Preview:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"vs None: {weapon.GetEffectiveDamage(EnemyType.None)}");
        EditorGUILayout.LabelField($"vs Demon: {weapon.GetEffectiveDamage(EnemyType.Demon)}");
        EditorGUILayout.LabelField($"vs Ghost: {weapon.GetEffectiveDamage(EnemyType.Ghost)}");
        EditorGUILayout.LabelField($"vs Zombie: {weapon.GetEffectiveDamage(EnemyType.Zombie)}");
    }

    private void DrawFooter()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
        
        List<ItemData> allItems = GetAllItems();
        int weaponCount = allItems.OfType<WeaponData>().Count();
        int basicWeaponCount = allItems.OfType<WeaponData>().Count(w => !w.requiresAmmo);
        int limitedWeaponCount = allItems.OfType<WeaponData>().Count(w => w.requiresAmmo);
        int consumableCount = allItems.Count(i => !(i is WeaponData) && i.category == ItemCategory.Consumable);
        int keyCount = allItems.Count(i => !(i is WeaponData) && i.category == ItemCategory.Key);
        
        EditorGUILayout.LabelField($"Total: {allItems.Count} | Weapons: {weaponCount} (Basic: {basicWeaponCount}, Limited: {limitedWeaponCount}) | Consumables: {consumableCount} | Keys: {keyCount}", EditorStyles.miniLabel);
        
        GUILayout.FlexibleSpace();
        
        if (GUILayout.Button("Create New Item", GUILayout.Width(120)))
        {
            ShowCreateMenu();
        }
        
        EditorGUILayout.EndHorizontal();
    }

    private void ShowCreateMenu()
    {
        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("Item"), false, () => CreateAsset<ItemData>("New Item", "Inventory/Item"));
        menu.AddItem(new GUIContent("Weapon"), false, () => CreateAsset<WeaponData>("New Weapon", "Inventory/Weapon"));
        menu.ShowAsContext();
    }

    private void CreateAsset<T>(string defaultName, string menuPath) where T : ScriptableObject
    {
        string path = EditorUtility.SaveFilePanelInProject(
            $"Create {defaultName}",
            $"{defaultName}.asset",
            "asset",
            "Choose location to save the asset",
            "Assets/Itens"
        );
        
        if (!string.IsNullOrEmpty(path))
        {
            T asset = CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }
    }

    private List<ItemData> GetFilteredItems()
    {
        List<ItemData> items = GetAllItems();
        
        if (!string.IsNullOrEmpty(searchQuery))
        {
            items = items.Where(i => i.itemName.ToLower().Contains(searchQuery.ToLower())).ToList();
        }
        
        switch (currentFilter)
        {
            case FilterType.Weapons:
                items = items.OfType<WeaponData>().Cast<ItemData>().ToList();
                break;
            case FilterType.Ammo:
                items = items.OfType<WeaponData>().Where(w => w.requiresAmmo).Cast<ItemData>().ToList();
                break;
            case FilterType.Consumables:
                items = items.Where(i => !(i is WeaponData) && i.category == ItemCategory.Consumable).ToList();
                break;
            case FilterType.Keys:
                items = items.Where(i => !(i is WeaponData) && i.category == ItemCategory.Key).ToList();
                break;
            case FilterType.Items:
                items = items.Where(i => !(i is WeaponData)).ToList();
                break;
        }
        
        return items.OrderBy(i => i.itemName).ToList();
    }

    private List<ItemData> GetAllItems()
    {
        string[] guids = AssetDatabase.FindAssets("t:ItemData");
        List<ItemData> items = new List<ItemData>();
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ItemData item = AssetDatabase.LoadAssetAtPath<ItemData>(path);
            if (item != null)
            {
                items.Add(item);
            }
        }
        
        return items;
    }
}


}
