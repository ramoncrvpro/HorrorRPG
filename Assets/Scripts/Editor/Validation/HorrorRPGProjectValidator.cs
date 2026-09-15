using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using HorrorRPG.Core;
using HorrorRPG.Interaction;

namespace HorrorRPG.Editor
{
    /// <summary>Runs structural validation for the currently loaded HorrorRPG content.</summary>
    public static class HorrorRPGProjectValidator
    {
        [MenuItem("Tools/Horror RPG/Validate Project")]
        public static void ValidateProject()
        {
            int errors = 0;
            Scene activeScene = SceneManager.GetActiveScene();
            MonoBehaviour[] behaviours = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            int bootstrapCount = behaviours.OfType<GameBootstrap>().Count();
            if (bootstrapCount > 1) { Debug.LogError($"[{activeScene.name}] More than one GameBootstrap is present."); errors++; }
            int listenerCount = Object.FindObjectsByType<AudioListener>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
            if (listenerCount > 1) { Debug.LogError($"[{activeScene.name}] Found {listenerCount} AudioListeners; exactly one is expected."); errors++; }
            foreach (SceneLoadInteraction transition in behaviours.OfType<SceneLoadInteraction>())
            {
                if (transition == null) continue;
                Debug.Log($"[{activeScene.name}] Scene transition found on {transition.name}; validate its destination through the SceneCatalog.");
            }
            int missingScripts = 0;
            foreach (GameObject root in activeScene.GetRootGameObjects()) missingScripts += GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(root);
            if (missingScripts > 0) { Debug.LogError($"[{activeScene.name}] Found {missingScripts} missing MonoBehaviour scripts."); errors++; }
            if (errors == 0) Debug.Log($"HorrorRPG validation passed for scene '{activeScene.name}'.");
            else Debug.LogError($"HorrorRPG validation found {errors} structural error(s) in scene '{activeScene.name}'.");
        }
    }
}
