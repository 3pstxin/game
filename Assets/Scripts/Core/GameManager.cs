using UnityEngine;
using IdleViking.Data;
using IdleViking.Models;
using IdleViking.Systems;

namespace IdleViking.Core
{
    /// <summary>
    /// Entry point for the game. Bootstraps all systems, owns the game loop,
    /// and manages save/load lifecycle.
    /// Attach this to a single GameObject in your boot scene.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState State { get; private set; }
        public TimeManager Time { get; private set; }

        [Header("Databases")]
        [SerializeField] private ResourceDatabase resourceDatabase;
        [SerializeField] private BuildingDatabase buildingDatabase;
        [SerializeField] private VikingDatabase vikingDatabase;
        [SerializeField] private EquipmentDatabase equipmentDatabase;
        [SerializeField] private EnemyDatabase enemyDatabase;
        [SerializeField] private DungeonDatabase dungeonDatabase;
        [SerializeField] private FarmDatabase farmDatabase;
        [SerializeField] private MilestoneDatabase milestoneDatabase;

        [Header("Settings")]
        [Tooltip("Auto-save interval in seconds")]
        [SerializeField] private float autoSaveInterval = 60f;

        [Tooltip("Max offline time in hours for idle gains")]
        [SerializeField] private float maxOfflineHours = 8f;

        [Tooltip("How often to check milestones in seconds")]
        [SerializeField] private float milestoneCheckInterval = 5f;

        private float _autoSaveTimer;
        private float _milestoneTimer;

        // Convenience accessors
        public ResourceDatabase ResourceDB => resourceDatabase;
        public BuildingDatabase BuildingDB => buildingDatabase;
        public VikingDatabase VikingDB => vikingDatabase;
        public EquipmentDatabase EquipmentDB => equipmentDatabase;
        public EnemyDatabase EnemyDB => enemyDatabase;
        public DungeonDatabase DungeonDB => dungeonDatabase;
        public FarmDatabase FarmDB => farmDatabase;
        public MilestoneDatabase MilestoneDB => milestoneDatabase;
        public PrestigeData PrestigeConfig => milestoneDatabase != null ? milestoneDatabase.prestigeConfig : null;

        /// <summary>
        /// Current prestige production multiplier. Used by ResourceSystem.
        /// </summary>
        public double PrestigeMultiplier =>
            ProgressionSystem.GetPrestigeMultiplier(State, PrestigeConfig);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            Initialize();
        }

        private void Initialize()
        {
            State = SaveSystem.Load();

            Time = new TimeManager();
            Time.MaxOfflineSeconds = maxOfflineHours * 3600;
            Time.CalculateOfflineTime(State.GetLastSaveTime());

            ApplyOfflineGains(Time.OfflineSeconds);

            // Check milestones that may have been achieved while offline
            if (milestoneDatabase != null)
                ProgressionSystem.CheckAllMilestones(State, milestoneDatabase);

            Debug.Log("[GameManager] Initialization complete.");
        }

        private void Update()
        {
            float dt = Time.GetDeltaTime();

            ResourceSystem.Tick(State, resourceDatabase, buildingDatabase, vikingDatabase,
                PrestigeMultiplier, dt);
            DungeonSystem.TickEnergy(State.dungeons, dt);

            // Periodic milestone check
            _milestoneTimer += dt;
            if (_milestoneTimer >= milestoneCheckInterval && milestoneDatabase != null)
            {
                _milestoneTimer = 0f;
                ProgressionSystem.CheckAllMilestones(State, milestoneDatabase);
            }

            // Auto-save
            _autoSaveTimer += dt;
            if (_autoSaveTimer >= autoSaveInterval)
            {
                _autoSaveTimer = 0f;
                SaveGame();
            }
        }

        private void ApplyOfflineGains(double offlineSeconds)
        {
            if (offlineSeconds <= 0)
                return;

            Debug.Log($"[GameManager] Applying {offlineSeconds:F1}s of offline gains.");

            ResourceSystem.ApplyOffline(State, resourceDatabase, buildingDatabase,
                vikingDatabase, PrestigeMultiplier, offlineSeconds);
            DungeonSystem.ApplyOfflineEnergy(State.dungeons, offlineSeconds);
            FarmSystem.ProcessOffline(State, farmDatabase);
        }

        /// <summary>
        /// Run a standalone combat encounter.
        /// </summary>
        public CombatLog StartBattle(EnemyData[] enemies)
        {
            var log = CombatSystem.Fight(State, vikingDatabase, equipmentDatabase, enemies);
            if (log != null) SaveGame();
            return log;
        }

        /// <summary>
        /// Start a dungeon run.
        /// </summary>
        public DungeonResult StartDungeon(DungeonData dungeon)
        {
            return DungeonSystem.StartRun(State, dungeonDatabase, dungeon, vikingDatabase, equipmentDatabase);
        }

        /// <summary>
        /// Fight the next floor of an active dungeon run.
        /// </summary>
        public DungeonResult AdvanceDungeon(DungeonRun run, DungeonData dungeon)
        {
            var result = DungeonSystem.AdvanceFloor(State, run, dungeon, vikingDatabase, equipmentDatabase);
            SaveGame();
            return result;
        }

        /// <summary>
        /// Retreat from an active dungeon run.
        /// </summary>
        public DungeonResult RetreatDungeon(DungeonRun run)
        {
            var result = DungeonSystem.Retreat(State, run, vikingDatabase);
            SaveGame();
            return result;
        }

        /// <summary>
        /// Attempt to prestige. Returns true if successful.
        /// </summary>
        public bool TryPrestige()
        {
            if (PrestigeConfig == null) return false;

            bool success = ProgressionSystem.TryPrestige(State, PrestigeConfig);
            if (success) SaveGame();
            return success;
        }

        public void SaveGame()
        {
            SaveSystem.Save(State);
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused)
                SaveGame();
        }

        private void OnApplicationQuit()
        {
            SaveGame();
        }
    }
}
