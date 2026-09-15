using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using HorrorRPG.Battle;
using HorrorRPG.Dialogue;
using HorrorRPG.Input;
using HorrorRPG.Inventory;

namespace HorrorRPG.Core
{
    /// <summary>Persistent composition root and the only global runtime entry point.</summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        public static GameBootstrap Instance { get; private set; }
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private SceneCatalog sceneCatalog;
        public GameContext Context { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            if (inputActions == null) throw new System.InvalidOperationException("GameBootstrap requires an InputActionAsset.");
            if (sceneCatalog == null) throw new System.InvalidOperationException("GameBootstrap requires a SceneCatalog.");
            var session = new GameSession();
            var input = new InputContextService();
            var sceneFlow = new SceneFlowService(sceneCatalog, session);
            var inventory = new InventoryService(session);
            var dialogue = new DialogueService();
            var battle = new BattleService(session, inventory);
            var navigation = new UINavigationService();
            Context = new GameContext(session, input, sceneFlow, inventory, dialogue, battle, navigation);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SceneCompositionRoot root = FindFirstObjectByType<SceneCompositionRoot>();
            if (root != null) InitializeScene(root);
        }

        private void Start()
        {
            SceneCompositionRoot root = FindFirstObjectByType<SceneCompositionRoot>();
            if (root != null) InitializeScene(root);
        }

        public void InitializeScene(SceneCompositionRoot sceneRoot) { if (sceneRoot == null) throw new System.ArgumentNullException(nameof(sceneRoot)); sceneRoot.Compose(Context); }
        public void ShutdownScene(SceneCompositionRoot sceneRoot) { if (sceneRoot != null) sceneRoot.Release(); Context?.Input.ClearSceneLeases(); Context?.Navigation.ClearSceneEntries(); }
        private void OnDestroy() { SceneManager.sceneLoaded -= OnSceneLoaded; if (Instance == this) Instance = null; }
    }
}
