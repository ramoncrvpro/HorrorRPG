using Bezi;
using System;
using System.Collections.Generic;
using System.Linq;
using HorrorRPG.Battle;
using HorrorRPG.Core;
using HorrorRPG.Dialogue;
using HorrorRPG.Interaction;
using HorrorRPG.Inventory;
using HorrorRPG.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

namespace HorrorRPG.Editor
{
    /// <summary>Runs structural validation across HorrorRPG scenes, prefabs and data assets.</summary>
    public static class HorrorRPGProjectValidator
    {
        private const string InteractableLayerName = "Interactable";
        private static readonly string[] RequiredScenePaths =
        {
            "Assets/Scenes/Floor0.unity",
            "Assets/Scenes/Floor1.unity",
            "Assets/Scenes/SampleScene.unity",
            "Assets/Scenes/SampleScene 1.unity"
        };

        /// <summary>Validates every planned scene and all gameplay data assets.</summary>
        [MenuItem("Tools/Horror RPG/Validate Project")]
        [BeziAction("Validates all HorrorRPG scenes, prefabs, input references, world IDs and gameplay data.", IsReadOnly = true)]
        public static void ValidateProject()
        {
            SceneSetup[] originalSetup = EditorSceneManager.GetSceneManagerSetup();
            int errors = 0;
            try
            {
                foreach (string scenePath in RequiredScenePaths)
                {
                    Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                    errors += ValidateScene(scene);
                }
                errors += ValidateDataAssets();
                errors += ValidateStableIds();
            }
            finally
            {
                EditorSceneManager.RestoreSceneManagerSetup(originalSetup);
            }

            if (errors == 0) Debug.Log($"HorrorRPG validation passed for {RequiredScenePaths.Length} scenes and all gameplay data.");
            else Debug.LogError($"HorrorRPG validation found {errors} structural error(s).");
        }

        private static int ValidateScene(Scene scene)
        {
            MonoBehaviour[] behaviours = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(behaviour => behaviour.gameObject.scene == scene)
                .ToArray();
            int errors = 0;
            errors += ValidateSceneComposition(scene, behaviours);
            errors += ValidateTransitions(scene, behaviours);
            errors += ValidateWorldObjectIds(scene, behaviours);
            errors += ValidateInteractableLayers(scene, behaviours);
            errors += ValidateWaypoints(scene, behaviours);
            errors += ValidateRenderers(scene);
            errors += ValidateRequiredReferences(scene, behaviours);
            return errors;
        }

        private static int ValidateSceneComposition(Scene scene, MonoBehaviour[] behaviours)
        {
            int errors = 0;
            int bootstrapCount = behaviours.OfType<GameBootstrap>().Count();
            int compositionCount = behaviours.OfType<SceneCompositionRoot>().Count();
            int listenerCount = UnityEngine.Object.FindObjectsByType<AudioListener>(FindObjectsInactive.Include, FindObjectsSortMode.None).Count(listener => listener.gameObject.scene == scene);
            InputSystemUIInputModule[] modules = UnityEngine.Object.FindObjectsByType<InputSystemUIInputModule>(FindObjectsInactive.Include, FindObjectsSortMode.None).Where(module => module.gameObject.scene == scene).ToArray();

            if (bootstrapCount != 1) errors += Report(scene, $"Expected exactly one GameBootstrap, found {bootstrapCount}.");
            if (compositionCount != 1) errors += Report(scene, $"Expected exactly one SceneCompositionRoot, found {compositionCount}.");
            if (listenerCount != 1) errors += Report(scene, $"Expected exactly one AudioListener, found {listenerCount}.");
            if (modules.Length != 1) errors += Report(scene, $"Expected exactly one InputSystemUIInputModule, found {modules.Length}.");
            else if (modules[0].actionsAsset == null || modules[0].move == null || modules[0].submit == null || modules[0].cancel == null)
                errors += Report(scene, "InputSystemUIInputModule requires actionsAsset, move, submit and cancel references.", modules[0]);

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                int missingScripts = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(root);
                if (missingScripts > 0) errors += Report(scene, $"'{root.name}' contains {missingScripts} missing MonoBehaviour script(s).", root, missingScripts);
            }
            return errors;
        }

        private static int ValidateTransitions(Scene scene, IEnumerable<MonoBehaviour> behaviours)
        {
            SceneCatalog catalog = AssetDatabase.FindAssets("t:SceneCatalog")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<SceneCatalog>)
                .FirstOrDefault(asset => asset != null);
            int errors = 0;
            foreach (SceneLoadInteraction transition in behaviours.OfType<SceneLoadInteraction>())
            {
                var serializedTransition = new SerializedObject(transition);
                SerializedProperty enabledProperty = serializedTransition.FindProperty("canInteract");
                if (enabledProperty != null && !enabledProperty.boolValue) continue;
                string sceneId = serializedTransition.FindProperty("sceneToLoad")?.stringValue;
                if (catalog == null || string.IsNullOrWhiteSpace(sceneId) || !catalog.TryGetScene(sceneId, out SceneCatalogEntry entry))
                {
                    errors += Report(scene, $"SceneLoadInteraction '{transition.name}' has invalid catalog destination '{sceneId}'.", transition);
                    continue;
                }
                if (!IsSceneEnabledInBuild(entry.SceneName))
                    errors += Report(scene, $"SceneLoadInteraction '{transition.name}' targets '{entry.SceneName}', which is not enabled in Build Settings.", transition);
            }
            return errors;
        }

        private static int ValidateWorldObjectIds(Scene scene, IEnumerable<MonoBehaviour> behaviours)
        {
            var ids = new HashSet<string>(StringComparer.Ordinal);
            int errors = 0;
            foreach (WorldObjectId worldObjectId in behaviours.OfType<WorldObjectId>())
            {
                if (string.IsNullOrWhiteSpace(worldObjectId.Value) || !ids.Add(worldObjectId.Value))
                    errors += Report(scene, $"WorldObjectId on '{worldObjectId.name}' is empty or duplicated: '{worldObjectId.Value}'.", worldObjectId);
            }
            foreach (MonoBehaviour behaviour in behaviours.Where(IsPersistentWorldObject))
            {
                if (behaviour.GetComponent<WorldObjectId>() == null)
                    errors += Report(scene, $"Persistent object '{behaviour.name}' requires a WorldObjectId.", behaviour);
            }
            return errors;
        }

        private static bool IsSceneEnabledInBuild(string sceneName)
        {
            return EditorBuildSettings.scenes.Any(scene =>
                scene.enabled && string.Equals(System.IO.Path.GetFileNameWithoutExtension(scene.path), sceneName, StringComparison.Ordinal));
        }

        private static bool IsPersistentWorldObject(MonoBehaviour behaviour)
        {
            return behaviour is ItemInteraction
                || behaviour is DoorInteraction
                || behaviour is DialogueItemInteraction
                || behaviour is BattleTrigger3D;
        }

        private static int ValidateInteractableLayers(Scene scene, IEnumerable<MonoBehaviour> behaviours)
        {
            int interactableLayer = LayerMask.NameToLayer(InteractableLayerName);
            int errors = 0;
            if (interactableLayer < 0) return Report(scene, $"Layer '{InteractableLayerName}' is missing.");
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour is IInteractable && behaviour.gameObject.layer != interactableLayer)
                    errors += Report(scene, $"Interactable '{behaviour.name}' is not on layer '{InteractableLayerName}'.", behaviour);
            }
            return errors;
        }

        private static int ValidateWaypoints(Scene scene, IEnumerable<MonoBehaviour> behaviours)
        {
            int errors = 0;
            foreach (EnemyWaypointMovement movement in behaviours.OfType<EnemyWaypointMovement>())
            {
                SerializedProperty waypoints = new SerializedObject(movement).FindProperty("waypoints");
                if (waypoints == null || waypoints.arraySize == 0)
                    errors += Report(scene, $"EnemyWaypointMovement '{movement.name}' has no waypoints configured.", movement);
            }
            return errors;
        }

        private static int ValidateRenderers(Scene scene)
        {
            int errors = 0;
            foreach (MeshRenderer renderer in UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None).Where(component => component.gameObject.scene == scene))
            {
                if (renderer.sharedMaterials.Any(material => material == null))
                    errors += Report(scene, $"MeshRenderer '{renderer.name}' contains a null material.", renderer);
            }
            return errors;
        }

        private static int ValidateRequiredReferences(Scene scene, IEnumerable<MonoBehaviour> behaviours)
        {
            int errors = 0;
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour is InventoryManager) errors += ValidateReferences(scene, behaviour, "inventoryView");
                else if (behaviour is DialogueSystem) errors += ValidateReferences(scene, behaviour, "dialogueView");
                else if (behaviour is PlayerInteraction) errors += ValidateReferences(scene, behaviour, "raycastOrigin", "playerCamera", "promptUI");
                else if (behaviour is BattleManager) errors += ValidateReferences(scene, behaviour, "battleArena", "battleEnemyRenderer", "battleUIManager", "attackTimingBar", "defenseManager", "projectileManager", "transitionEffects");
                else if (behaviour is BattleUIManager) errors += ValidateReferences(scene, behaviour, "battleMainMenuBackground", "attackMenuBackground", "itensMenuBackground", "weaponTabs", "weaponSlots", "itemSlots", "playerHealthBar", "enemyHealthBar");
                else if (behaviour is ProjectileManager) errors += ValidateReferences(scene, behaviour, "spawnPoint", "leftTarget", "upTarget", "rightTarget", "projectilePrefab", "defenseManager");
                else if (behaviour is AttackTimingBar) errors += ValidateReferences(scene, behaviour, "timingView");
            }
            return errors;
        }

        private static int ValidateReferences(Scene scene, MonoBehaviour behaviour, params string[] properties)
        {
            var serializedObject = new SerializedObject(behaviour);
            int errors = 0;
            foreach (string propertyName in properties)
            {
                SerializedProperty property = serializedObject.FindProperty(propertyName);
                bool invalid = property == null
                    || (property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue == null)
                    || (property.isArray && property.propertyType != SerializedPropertyType.String && property.arraySize == 0);
                if (invalid) errors += Report(scene, $"{behaviour.GetType().Name} '{behaviour.name}' requires serialized property '{propertyName}'.", behaviour);
            }
            return errors;
        }

        private static int ValidateDataAssets()
        {
            int errors = 0;
            foreach (string path in FindAssetPaths<ItemData>())
            {
                ItemData item = AssetDatabase.LoadAssetAtPath<ItemData>(path);
                if (item == null || string.IsNullOrWhiteSpace(item.itemName) || item.maxStackSize <= 0) errors += ReportAsset(path, "Invalid ItemData");
            }
            foreach (string path in FindAssetPaths<WeaponData>())
            {
                WeaponData weapon = AssetDatabase.LoadAssetAtPath<WeaponData>(path);
                if (weapon == null || weapon.baseDamage <= 0 || weapon.markerSpeed <= 0f || weapon.criticalZoneCenter < 0f || weapon.criticalZoneCenter > 1f || weapon.criticalZoneWidth <= 0f || weapon.criticalMultiplier < 1f)
                    errors += ReportAsset(path, "Invalid WeaponData");
            }
            foreach (string path in FindAssetPaths<ConsumableData>())
            {
                ConsumableData item = AssetDatabase.LoadAssetAtPath<ConsumableData>(path);
                if (item == null || item.effectValue < 0 || item.effectDuration < 0f) errors += ReportAsset(path, "Invalid ConsumableData");
            }
            foreach (string path in FindAssetPaths<EnemyData>())
            {
                EnemyData enemy = AssetDatabase.LoadAssetAtPath<EnemyData>(path);
                if (enemy == null || string.IsNullOrWhiteSpace(enemy.enemyName) || enemy.maxHealth <= 0 || enemy.baseDamage <= 0 || enemy.availableAttacks.Count == 0 || enemy.availableAttacks.Any(attack => attack == null || !attack.IsValid()))
                    errors += ReportAsset(path, "Invalid EnemyData");
            }
            foreach (string path in FindAssetPaths<AttackData>())
            {
                AttackData attack = AssetDatabase.LoadAssetAtPath<AttackData>(path);
                if (attack == null || string.IsNullOrWhiteSpace(attack.attackName) || !attack.IsValid()) errors += ReportAsset(path, "Invalid AttackData");
            }
            foreach (string path in FindAssetPaths<ProjectileConfig>())
            {
                ProjectileConfig config = AssetDatabase.LoadAssetAtPath<ProjectileConfig>(path);
                if (config == null || config.loopRadius < 0f || config.minLoopTime < 0f || config.maxLoopTime < config.minLoopTime || config.minTravelSpeed <= 0f || config.maxTravelSpeed < config.minTravelSpeed || config.ProjectileVisual == null)
                    errors += ReportAsset(path, "Invalid ProjectileConfig");
            }
            foreach (string path in FindAssetPaths<DialogueData>())
            {
                DialogueData dialogue = AssetDatabase.LoadAssetAtPath<DialogueData>(path);
                if (dialogue == null || dialogue.sentences == null || dialogue.sentences.Length == 0 || dialogue.sentences.Any(string.IsNullOrWhiteSpace)) errors += ReportAsset(path, "Invalid DialogueData");
            }
            return errors;
        }

        private static int ValidateStableIds()
        {
            var ids = new Dictionary<string, string>(StringComparer.Ordinal);
            int errors = 0;
            IEnumerable<string> paths = FindAssetPaths<ItemData>()
                .Concat(FindAssetPaths<EnemyData>())
                .Concat(FindAssetPaths<AttackData>())
                .Concat(FindAssetPaths<ProjectileConfig>())
                .Concat(FindAssetPaths<DialogueData>())
                .Distinct();
            foreach (string path in paths)
            {
                var serializedObject = new SerializedObject(AssetDatabase.LoadMainAssetAtPath(path));
                string id = serializedObject.FindProperty("stableId")?.stringValue;
                if (string.IsNullOrWhiteSpace(id))
                {
                    errors += ReportAsset(path, "Stable ID is empty");
                    continue;
                }
                if (ids.TryGetValue(id, out string duplicatePath)) errors += ReportAsset(path, $"Stable ID duplicates '{duplicatePath}'");
                else ids.Add(id, path);
            }
            return errors;
        }

        private static IEnumerable<string> FindAssetPaths<T>() where T : UnityEngine.Object
        {
            return AssetDatabase.FindAssets($"t:{typeof(T).Name}").Select(AssetDatabase.GUIDToAssetPath);
        }

        private static int Report(Scene scene, string message, UnityEngine.Object context = null, int count = 1)
        {
            Debug.LogError($"[{scene.name}] {message}", context);
            return count;
        }

        private static int ReportAsset(string path, string message)
        {
            Debug.LogError($"[{path}] {message}.", AssetDatabase.LoadMainAssetAtPath(path));
            return 1;
        }
    }
}
