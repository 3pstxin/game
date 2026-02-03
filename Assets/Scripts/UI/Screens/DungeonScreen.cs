using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IdleViking.Data;
using IdleViking.Models;

namespace IdleViking.UI
{
    /// <summary>
    /// Screen for selecting and running dungeons.
    /// </summary>
    public class DungeonScreen : BaseScreen
    {
        [Header("Dungeon Selection")]
        [SerializeField] private Transform dungeonListContainer;
        [SerializeField] private DungeonListItem dungeonItemPrefab;

        [Header("Team Setup")]
        [SerializeField] private GameObject teamSetupPanel;
        [SerializeField] private Transform teamSlotContainer;
        [SerializeField] private VikingSlot vikingSlotPrefab;
        [SerializeField] private Button startDungeonButton;
        [SerializeField] private TextMeshProUGUI energyCostText;

        [Header("Run Display")]
        [SerializeField] private GameObject runPanel;
        [SerializeField] private TextMeshProUGUI floorText;
        [SerializeField] private TextMeshProUGUI combatLogText;
        [SerializeField] private ProgressBar bossHpBar;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button retreatButton;

        [Header("Results")]
        [SerializeField] private GameObject resultsPanel;
        [SerializeField] private TextMeshProUGUI resultTitleText;
        [SerializeField] private TextMeshProUGUI resultDetailsText;
        [SerializeField] private Button claimRewardsButton;

        [Header("Energy Display")]
        [SerializeField] private TextMeshProUGUI energyText;
        [SerializeField] private TimerDisplay energyTimer;

        [Header("Database")]
        [SerializeField] private DungeonDatabase dungeonDatabase;
        [SerializeField] private VikingDatabase vikingDatabase;

        private List<DungeonListItem> _dungeonItems = new List<DungeonListItem>();
        private List<VikingSlot> _teamSlots = new List<VikingSlot>();
        private DungeonData _selectedDungeon;
        private DungeonRun _currentRun;
        private List<int> _selectedTeam = new List<int>();

        protected override void Awake()
        {
            base.Awake();

            if (startDungeonButton != null)
                startDungeonButton.onClick.AddListener(OnStartDungeon);
            if (continueButton != null)
                continueButton.onClick.AddListener(OnContinueFloor);
            if (retreatButton != null)
                retreatButton.onClick.AddListener(OnRetreat);
            if (claimRewardsButton != null)
                claimRewardsButton.onClick.AddListener(OnClaimRewards);
        }

        private void OnDestroy()
        {
            if (startDungeonButton != null)
                startDungeonButton.onClick.RemoveListener(OnStartDungeon);
            if (continueButton != null)
                continueButton.onClick.RemoveListener(OnContinueFloor);
            if (retreatButton != null)
                retreatButton.onClick.RemoveListener(OnRetreat);
            if (claimRewardsButton != null)
                claimRewardsButton.onClick.RemoveListener(OnClaimRewards);
        }

        protected override void SubscribeToEvents()
        {
            UIEvents.OnDungeonFloorComplete += OnFloorComplete;
            UIEvents.OnDungeonComplete += OnDungeonComplete;
        }

        protected override void UnsubscribeFromEvents()
        {
            UIEvents.OnDungeonFloorComplete -= OnFloorComplete;
            UIEvents.OnDungeonComplete -= OnDungeonComplete;
        }

        public override void Refresh()
        {
            RefreshEnergy();
            RefreshDungeonList();
            ShowSelectionPanel();
        }

        private void RefreshEnergy()
        {
            var state = GameManager.Instance?.State;
            if (state == null) return;

            if (energyText != null)
            {
                int current = state.Dungeon.CurrentEnergy;
                int max = state.Dungeon.MaxEnergy;
                energyText.text = $"Energy: {current}/{max}";
            }
        }

        private void RefreshDungeonList()
        {
            ClearDungeonItems();

            if (dungeonDatabase == null || dungeonListContainer == null || dungeonItemPrefab == null)
                return;

            var state = GameManager.Instance?.State;
            if (state == null) return;

            foreach (var dungeon in dungeonDatabase.Dungeons)
            {
                if (!DungeonSystem.IsUnlocked(state, dungeon))
                    continue;

                var item = Instantiate(dungeonItemPrefab, dungeonListContainer);
                item.Setup(dungeon, state);
                item.OnDungeonSelected += OnDungeonSelected;
                _dungeonItems.Add(item);
            }
        }

        private void OnDungeonSelected(DungeonData dungeon)
        {
            _selectedDungeon = dungeon;
            ShowTeamSetup();
        }

        private void ShowSelectionPanel()
        {
            if (teamSetupPanel != null) teamSetupPanel.SetActive(false);
            if (runPanel != null) runPanel.SetActive(false);
            if (resultsPanel != null) resultsPanel.SetActive(false);
            if (dungeonListContainer != null) dungeonListContainer.gameObject.SetActive(true);
        }

        private void ShowTeamSetup()
        {
            if (dungeonListContainer != null) dungeonListContainer.gameObject.SetActive(false);
            if (teamSetupPanel != null) teamSetupPanel.SetActive(true);
            if (runPanel != null) runPanel.SetActive(false);
            if (resultsPanel != null) resultsPanel.SetActive(false);

            if (energyCostText != null)
                energyCostText.text = $"Energy Cost: {_selectedDungeon.EnergyCost}";

            RefreshTeamSlots();
        }

        private void RefreshTeamSlots()
        {
            ClearTeamSlots();
            _selectedTeam.Clear();

            var state = GameManager.Instance?.State;
            if (state == null || teamSlotContainer == null || vikingSlotPrefab == null) return;

            // Create slots for combat-assigned vikings
            foreach (var kvp in state.Vikings.OwnedVikings)
            {
                var viking = kvp.Value;
                if (viking.Assignment == VikingAssignment.Combat)
                {
                    var slot = Instantiate(vikingSlotPrefab, teamSlotContainer);
                    var data = vikingDatabase?.GetVikingById(viking.VikingId);
                    slot.Setup(viking, data);
                    slot.OnSlotClicked += OnTeamSlotClicked;
                    _teamSlots.Add(slot);
                    _selectedTeam.Add(viking.UniqueId);
                }
            }

            UpdateStartButton();
        }

        private void OnTeamSlotClicked(VikingSlot slot)
        {
            // Toggle selection
            slot.SetSelected(!slot.IsSelected);
            UpdateStartButton();
        }

        private void UpdateStartButton()
        {
            var state = GameManager.Instance?.State;
            if (state == null) return;

            bool hasTeam = _selectedTeam.Count > 0;
            bool hasEnergy = state.Dungeon.CurrentEnergy >= _selectedDungeon.EnergyCost;

            if (startDungeonButton != null)
                startDungeonButton.interactable = hasTeam && hasEnergy;
        }

        private void OnStartDungeon()
        {
            var state = GameManager.Instance?.State;
            if (state == null || _selectedDungeon == null) return;

            _currentRun = DungeonSystem.StartRun(state, _selectedDungeon, _selectedTeam, vikingDatabase);
            if (_currentRun != null)
            {
                ShowRunPanel();
                ProcessNextFloor();
            }
            else
            {
                UIEvents.FireToast("Cannot start dungeon.");
            }
        }

        private void ShowRunPanel()
        {
            if (teamSetupPanel != null) teamSetupPanel.SetActive(false);
            if (runPanel != null) runPanel.SetActive(true);
            if (resultsPanel != null) resultsPanel.SetActive(false);
        }

        private void ProcessNextFloor()
        {
            if (_currentRun == null || _currentRun.IsComplete) return;

            var state = GameManager.Instance?.State;
            if (state == null) return;

            var combatLog = DungeonSystem.ProcessFloor(state, _currentRun, _selectedDungeon, vikingDatabase);

            // Update UI
            if (floorText != null)
                floorText.text = $"Floor {_currentRun.CurrentFloor}/{_selectedDungeon.FloorCount}";

            if (combatLogText != null && combatLog != null)
            {
                string logStr = combatLog.Victory ? "Victory!\n" : "Defeat!\n";
                foreach (var action in combatLog.Actions)
                {
                    logStr += $"{action}\n";
                }
                combatLogText.text = logStr;
            }

            UIEvents.FireDungeonFloorComplete(_currentRun);
        }

        private void OnContinueFloor()
        {
            if (_currentRun == null || _currentRun.IsComplete)
            {
                ShowResults();
                return;
            }

            ProcessNextFloor();
        }

        private void OnFloorComplete(DungeonRun run)
        {
            if (run != _currentRun) return;

            if (continueButton != null)
                continueButton.interactable = !run.IsComplete;

            if (run.IsComplete)
            {
                UIEvents.FireDungeonComplete(run);
            }
        }

        private void OnDungeonComplete(DungeonRun run)
        {
            if (run != _currentRun) return;
            ShowResults();
        }

        private void ShowResults()
        {
            if (runPanel != null) runPanel.SetActive(false);
            if (resultsPanel != null) resultsPanel.SetActive(true);

            if (_currentRun == null) return;

            if (resultTitleText != null)
                resultTitleText.text = _currentRun.Victory ? "Victory!" : "Defeat";

            if (resultDetailsText != null)
            {
                string details = $"Floors Cleared: {_currentRun.CurrentFloor - 1}/{_selectedDungeon.FloorCount}\n";
                details += $"Rewards Earned: {_currentRun.RewardsEarned.Count} items";
                resultDetailsText.text = details;
            }
        }

        private void OnClaimRewards()
        {
            // Rewards were already added during the run
            _currentRun = null;
            Refresh();
            UIEvents.FireResourcesChanged();
            UIEvents.FireInventoryChanged();
        }

        private void OnRetreat()
        {
            _currentRun = null;
            Refresh();
        }

        private void ClearDungeonItems()
        {
            foreach (var item in _dungeonItems)
            {
                if (item != null)
                {
                    item.OnDungeonSelected -= OnDungeonSelected;
                    Destroy(item.gameObject);
                }
            }
            _dungeonItems.Clear();
        }

        private void ClearTeamSlots()
        {
            foreach (var slot in _teamSlots)
            {
                if (slot != null)
                {
                    slot.OnSlotClicked -= OnTeamSlotClicked;
                    Destroy(slot.gameObject);
                }
            }
            _teamSlots.Clear();
        }

        protected override void OnHide()
        {
            ClearDungeonItems();
            ClearTeamSlots();
        }
    }
}
