using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages rocket upgrades and their costs
/// </summary>
public class UpgradeManager : MonoBehaviour
{
    [System.Serializable]
    private class UpgradeData
    {
        public string displayName;        // Name shown in UI
        public float currentCost;         // Current cost of upgrade
        public float upgradePercentage;   // How much it increases per upgrade
        public int upgradeCount;          // How many times upgraded
        public System.Action<float> upgradeAction;  // Function to call when upgrading
    }

    [Header("Base Settings")]
    [SerializeField] private float m_BaseUpgradeCost = 100f;
    [SerializeField] private float m_CostIncreasePercentage = 100f;  // Cost doubles each time

    [Header("Upgrade Percentages")]
    [SerializeField] private float m_SpeedUpgradePercentage = 5f;
    [SerializeField] private float m_FuelUpgradePercentage = 5f;
    [SerializeField] private float m_TurnSpeedUpgradePercentage = 5f;  // Changed from m_ManeuverabilityUpgradePercentage

    private Dictionary<UpgradeType, UpgradeData> m_UpgradeData;
    private RocketController m_RocketController;
    private GameManager m_GameManager;

    #region Properties
    public float CurrentUpgradeCost => m_UpgradeData.ContainsKey(UpgradeType.Speed) ? m_UpgradeData[UpgradeType.Speed].currentCost : m_BaseUpgradeCost;
    #endregion

    private void Start()
    {
        m_RocketController = FindFirstObjectByType<RocketController>();
        m_GameManager = GameManager.Instance;
        InitializeUpgrades();
    }

    private void InitializeUpgrades()
    {
        m_UpgradeData = new Dictionary<UpgradeType, UpgradeData>
        {
            {
                UpgradeType.Speed,
                new UpgradeData
                {
                    displayName = "Speed",
                    currentCost = m_BaseUpgradeCost,
                    upgradePercentage = m_SpeedUpgradePercentage,
                    upgradeAction = m_RocketController.UpgradeSpeed
                }
            },
            {
                UpgradeType.FuelCapacity,
                new UpgradeData
                {
                    displayName = "Fuel",
                    currentCost = m_BaseUpgradeCost,
                    upgradePercentage = m_FuelUpgradePercentage,
                    upgradeAction = m_RocketController.UpgradeFuelCapacity
                }
            },
            {
                UpgradeType.TurnSpeed,
                new UpgradeData
                {
                    displayName = "Turn Speed",
                    currentCost = m_BaseUpgradeCost,
                    upgradePercentage = m_TurnSpeedUpgradePercentage,
                    upgradeAction = m_RocketController.UpgradeTurnSpeed
                }
            }
        };
    }

    /// <summary>
    /// Gets the current cost for a specific upgrade type
    /// </summary>
    public float GetUpgradeCost(UpgradeType type)
    {
        return m_UpgradeData.ContainsKey(type) ? m_UpgradeData[type].currentCost : m_BaseUpgradeCost;
    }

    /// <summary>
    /// Gets the current upgrade percentage for a specific type
    /// </summary>
    public float GetUpgradePercentage(UpgradeType type)
    {
        return m_UpgradeData.ContainsKey(type) ? m_UpgradeData[type].upgradePercentage : 5f;
    }

    /// <summary>
    /// Gets the number of times this type has been upgraded
    /// </summary>
    public int GetUpgradeCount(UpgradeType type)
    {
        return m_UpgradeData.ContainsKey(type) ? m_UpgradeData[type].upgradeCount : 0;
    }

    /// <summary>
    /// Attempts to purchase and apply an upgrade
    /// </summary>
    public bool TryUpgrade(UpgradeType _type)
    {
        if (!m_UpgradeData.ContainsKey(_type) || m_RocketController == null) return false;

        var data = m_UpgradeData[_type];
        if (m_GameManager.Gold < data.currentCost) return false;

        // Apply upgrade
        float upgradeMultiplier = 1f + (data.upgradePercentage / 100f);
        data.upgradeAction(upgradeMultiplier);

        // Handle cost
        m_GameManager.SpendGold(data.currentCost);
        data.upgradeCount++;
        data.currentCost *= (1f + (m_CostIncreasePercentage / 100f));

        return true;
    }

    public (string name, float currentValue, float cost, float percentage) GetUpgradeInfo(UpgradeType type)
    {
        if (!m_UpgradeData.ContainsKey(type)) 
            return ("Unknown", 0f, 0f, 0f);

        var data = m_UpgradeData[type];
        float currentValue = type switch
        {
            UpgradeType.Speed => m_RocketController.BaseSpeed,
            UpgradeType.FuelCapacity => m_RocketController.MaxFuel,
            UpgradeType.TurnSpeed => m_RocketController.TurnSpeed,
            _ => 0f
        };

        return (data.displayName, currentValue, data.currentCost, data.upgradePercentage);
    }

    /// <summary>
    /// Checks if player can afford an upgrade
    /// </summary>
    public bool CanAffordUpgrade(UpgradeType type)
    {
        return m_UpgradeData.ContainsKey(type) && 
               m_GameManager != null && 
               m_GameManager.Gold >= m_UpgradeData[type].currentCost;
    }
} 