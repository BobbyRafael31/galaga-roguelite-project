using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("Master Database")]
    [Tooltip("Drag every UpgradeData SO in your project into this list")]
    [SerializeField] private List<UpgradeData> _allAvailableUpgrades = new List<UpgradeData>();

    [Header("Economy")]
    [SerializeField] private int _baseRerollCost = 250;
    [SerializeField] private int _rerollCostScaling = 250;

    private readonly Dictionary<string, ActiveUpgrade> _playerInventory = new Dictionary<string, ActiveUpgrade>();
    public UpgradeData[] CurrentDraftChoices { get; private set; } = new UpgradeData[3];
    public int CurrentRerollCost { get; private set; }

    private PlayerController _playerController;
    private PlayerShooter _playerShooter;
    private PlayerHealth _playerHealth;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        EventBus.OnGameStarted += ResetShopForNewRun;
        EventBus.OnShopEntered += GenerateDraftChoices;
    }

    private void OnDisable()
    {
        EventBus.OnGameStarted -= ResetShopForNewRun;
        EventBus.OnShopEntered -= GenerateDraftChoices;
    }

    private void ResetShopForNewRun()
    {
        _playerInventory.Clear();
        CurrentRerollCost = _baseRerollCost;

        // Cache player references safely since they just spawned
        _playerController = FindFirstObjectByType<PlayerController>();
        _playerShooter = FindFirstObjectByType<PlayerShooter>();
        _playerHealth = FindFirstObjectByType<PlayerHealth>();
    }

    #region RNG DRAFT GENERATION
    public void GenerateDraftChoices()
    {
        List<UpgradeData> validPool = GetValidUpgradePool();

        for (int i = 0; i < 3; i++)
        {
            if (validPool.Count == 0)
            {
                CurrentDraftChoices[i] = null; // No more valid upgrades exist in the game
                continue;
            }

            UpgradeData selected = GetWeightedRandomUpgrade(validPool);
            CurrentDraftChoices[i] = selected;

            // Remove it from the local pool so we don't roll the exact same card twice in one draft
            validPool.Remove(selected);
        }

        EventBus.OnDraftGenerated?.Invoke(CurrentDraftChoices);
    }

    private List<UpgradeData> GetValidUpgradePool()
    {
        List<UpgradeData> pool = new List<UpgradeData>();

        foreach (var upgrade in _allAvailableUpgrades)
        {
            if (!_playerInventory.TryGetValue(upgrade.UpgradeName, out ActiveUpgrade activeUp))
            {
                pool.Add(upgrade);
            }
            else if (activeUp.CurrentLevel < upgrade.MaxLevel)
            {
                pool.Add(upgrade);
            }
        }
        return pool;
    }

    private UpgradeData GetWeightedRandomUpgrade(List<UpgradeData> pool)
    {
        int totalWeight = 0;
        foreach (var u in pool) totalWeight += u.BaseRNGWeight;

        int randomRoll = Random.Range(0, totalWeight);
        int currentWeight = 0;

        foreach (var u in pool)
        {
            currentWeight += u.BaseRNGWeight;
            if (randomRoll < currentWeight)
            {
                return u;
            }
        }

        return pool[0];
    }

    #endregion

    #region ECONOMY & PURCHASING

    public int GetCurrentUpgradeCost(UpgradeData data)
    {
        if (_playerInventory.TryGetValue(data.UpgradeName, out ActiveUpgrade activeUp))
        {
            return data.BaseCost + (data.CostScalingPerLevel * activeUp.CurrentLevel);
        }
        return data.BaseCost;
    }

    public int GetCurrentUpgradeLevel(UpgradeData data)
    {
        if (_playerInventory.TryGetValue(data.UpgradeName, out ActiveUpgrade activeUp)) return activeUp.CurrentLevel;
        return 0;
    }

    public bool TryPurchaseUpgrade(int slotIndex)
    {
        UpgradeData data = CurrentDraftChoices[slotIndex];
        if (data == null) return false;

        int cost = GetCurrentUpgradeCost(data);

        if (ScoreManager.Instance.TrySpendScore(cost))
        {
            ApplyUpgradeToPlayer(data);

            CurrentDraftChoices[slotIndex] = null;
            return true;
        }

        return false;
    }

    public bool TryReroll()
    {
        if (ScoreManager.Instance.TrySpendScore(CurrentRerollCost))
        {
            CurrentRerollCost += _rerollCostScaling;
            GenerateDraftChoices();
            return true;
        }
        return false;
    }

    private void ApplyUpgradeToPlayer(UpgradeData data)
    {
        if (!_playerInventory.ContainsKey(data.UpgradeName))
        {
            _playerInventory.Add(data.UpgradeName, new ActiveUpgrade(data));
        }
        _playerInventory[data.UpgradeName].CurrentLevel++;

        if (_playerController != null && _playerShooter != null && _playerHealth != null)
        {
            data.ApplyUpgrade(_playerController, _playerShooter, _playerHealth);
        }
    }
    #endregion
}
