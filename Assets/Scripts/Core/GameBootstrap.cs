using HorrorRPG.Battle;
using HorrorRPG.Dialogue;
using HorrorRPG.Input;
using HorrorRPG.Inventory;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace HorrorRPG.Core
{
    /// <summary>Persistent composition root and the only global runtime entry point.</summary>
    [RequireComponent(typeof(PlayerInput), typeof(GameInputReader))]
    public sealed class GameBootstrap : MonoBehaviour
    {
        public static GameBootstrap Instance { get; private set; }

        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private SceneCatalog sceneCatalog;

        private GameInputReader inputReader;
        private SceneCompositionRoot activeSceneRoot;

        public GameContext Context { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            if (inputActions == null) throw new System.InvalidOperationException("GameBootstrap requires an InputActionAsset.");
            if (sceneCatalog == null) throw new System.InvalidOperationException("GameBootstrap requires a SceneCatalog.");

            inputReader = GetComponent<GameInputReader>();
            if (inputReader == null) throw new System.InvalidOperationException("GameBootstrap requires a GameInputReader.");

            var session = new GameSession();
            session.StartNewSession();
            var input = new InputContextService();
            var sceneFlow = new SceneFlowService(sceneCatalog, session);
            var inventory = new InventoryService(session);
            var dialogue = new DialogueService();
            var battle = new BattleService(session, inventory);
            var navigation = new UINavigationService();
            Context = new GameContext(session, inputReader, input, sceneFlow, inventory, dialogue, battle, navigation);

            input.ContextChanged += inputReader.SetContext;
            inputReader.SetContext(input.CurrentContext);
            sceneFlow.TransitionStarted += ClearSceneRuntimeState;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void Start()
        {
            ComposeLoadedScene();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ClearSceneRuntimeState();
            ComposeLoadedScene();
        }

        /// <summary>Composes all context receivers belonging to the supplied scene root.</summary>
        public void InitializeScene(SceneCompositionRoot sceneRoot)
        {
            if (sceneRoot == null) throw new System.ArgumentNullException(nameof(sceneRoot));
            if (Context == null) throw new System.InvalidOperationException("GameBootstrap context was not created.");
            if (activeSceneRoot != null && activeSceneRoot != sceneRoot) activeSceneRoot.Release();
            sceneRoot.Compose(Context);
            activeSceneRoot = sceneRoot;
        }

        /// <summary>Releases a composed scene and clears scene-scoped runtime state.</summary>
        public void ShutdownScene(SceneCompositionRoot sceneRoot)
        {
            if (sceneRoot != null) sceneRoot.Release();
            if (activeSceneRoot == sceneRoot) activeSceneRoot = null;
            ClearSceneRuntimeState();
        }

        private void ComposeLoadedScene()
        {
            SceneCompositionRoot sceneRoot = FindFirstObjectByType<SceneCompositionRoot>();
            if (sceneRoot != null) InitializeScene(sceneRoot);
        }

        private void ClearSceneRuntimeState()
        {
            if (Context == null) return;
            Context.Input.ClearSceneLeases();
            Context.Navigation.ClearSceneEntries();
            Context.Dialogue.AbortActive(DialogueEndReason.SceneChanged);
            if (Context.Battle.State.Phase != BattlePhase.Idle) Context.Battle.Abort(BattleEndReason.SceneChanged);
        }

        private void OnDestroy()
        {
            if (Instance != this) return;
            if (Context != null)
            {
                Context.Input.ContextChanged -= inputReader.SetContext;
                Context.SceneFlow.TransitionStarted -= ClearSceneRuntimeState;
            }
            activeSceneRoot?.Release();
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }
}
