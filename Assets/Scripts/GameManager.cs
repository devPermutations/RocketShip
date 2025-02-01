using UnityEngine;
using System;

/// <summary>
/// Manages game state, upgrades, and save/load functionality
/// </summary>
public enum UpgradeType
{
    Speed,
    FuelCapacity,
    TurnSpeed
}

public class GameManager : MonoBehaviour
{
    #region Singleton
    public static GameManager Instance { get; private set; }
    
    /// <summary>
    /// Initializes the singleton instance
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    #region Events
    public event Action<float> OnGoldChanged;    // Single event for gold changes
    public event Action OnGameOver;              
    #endregion

    #region Private Fields
    private float m_Gold;                        // Single gold variable
    private RocketController m_RocketController;       
    #endregion

    #region Properties
    public float Gold => m_Gold;       
    #endregion

    #region Unity Lifecycle
    /// <summary>
    /// Initializes game state and finds the player's rocket
    /// </summary>
    private void Start()
    {
        LoadGameData();
        m_RocketController = FindFirstObjectByType<RocketController>();
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Adds gold to the current run total
    /// </summary>
    public void AddGold(float _amount)
    {
        m_Gold += _amount;
        OnGoldChanged?.Invoke(m_Gold);
        SaveGameData();
    }

    /// <summary>
    /// Handles game over state and adds current run gold to total
    /// </summary>
    public void GameOver()
    {
        // Disable rocket controller first
        if (m_RocketController != null)
        {
            m_RocketController.enabled = false;  // Disable the controller component
        }

        // Reset positions but keep controller disabled
        ResetPositions();
        
        // Trigger game over UI and save
        OnGameOver?.Invoke();
        SaveGameData();
    }

    /// <summary>
    /// Attempts to purchase and apply an upgrade using total gold
    /// </summary>
    public bool TryUpgrade(UpgradeType _type)
    {
        if (m_RocketController == null)
        {
            m_RocketController = FindFirstObjectByType<RocketController>();
            if (m_RocketController == null) return false;
        }

        // Apply the selected upgrade
        switch (_type)
        {
            case UpgradeType.Speed:
                m_RocketController.UpgradeSpeed(1.05f);  // 5% increase
                break;
            case UpgradeType.FuelCapacity:
                m_RocketController.UpgradeFuelCapacity(1.05f);
                break;
            case UpgradeType.TurnSpeed:
                m_RocketController.UpgradeTurnSpeed(1.05f);
                break;
        }

        m_Gold -= 100f;  // Fixed cost of 100
        OnGoldChanged?.Invoke(m_Gold);
        SaveGameData();
        return true;
    }

    /// <summary>
    /// Restarts the game, resetting the rocket and current run gold
    /// </summary>
    public void RestartGame()
    {
        // Re-enable rocket controller
        if (m_RocketController != null)
        {
            m_RocketController.enabled = true;
        }

        // Reset collectible spawner
        CollectibleSpawner spawner = FindFirstObjectByType<CollectibleSpawner>();
        if (spawner != null)
        {
            spawner.ResetSpawner();
        }
    }

    public void SpendGold(float _amount)
    {
        m_Gold -= _amount;
        OnGoldChanged?.Invoke(m_Gold);
        SaveGameData();
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Saves current game state
    /// </summary>
    private void SaveGameData()
    {
        SaveSystem.SaveGame(m_Gold, m_RocketController);
    }

    /// <summary>
    /// Loads saved game state
    /// </summary>
    private void LoadGameData()
    {
        GameData data = SaveSystem.LoadGame();
        if (data != null)
        {
            m_Gold = data.gold;
            OnGoldChanged?.Invoke(m_Gold);
            
            if (m_RocketController != null)
            {
                m_RocketController.LoadStats(data);
            }
        }
    }

    // New method to handle position resets without enabling control
    private void ResetPositions()
    {
        // Clear collectibles
        GameObject[] collectibles = GameObject.FindGameObjectsWithTag("Collectible");
        foreach (GameObject collectible in collectibles)
        {
            Destroy(collectible);
        }

        // Reset rocket position but keep controller disabled
        if (m_RocketController != null)
        {
            m_RocketController.gameObject.SetActive(true);
            m_RocketController.ResetRocket();
        }
        else
        {
            m_RocketController = FindFirstObjectByType<RocketController>();
            if (m_RocketController != null)
            {
                m_RocketController.ResetRocket();
            }
        }
    }
    #endregion
} 