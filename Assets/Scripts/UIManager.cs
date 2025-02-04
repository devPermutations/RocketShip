using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Manages all UI elements and their interactions in the game
/// </summary>
public class UIManager : MonoBehaviour
{
    #region Serialized Fields
    [Header("HUD Elements")]
    [SerializeField] private TextMeshProUGUI m_GoldText;           // Displays current gold amount
    [SerializeField] private TextMeshProUGUI m_MetalText;          // Displays current metal amount
    [SerializeField] private TextMeshProUGUI m_EnergyText;         // Displays current energy amount
    [SerializeField] private Slider m_FuelSlider;                  // Shows fuel level
    [SerializeField] private TextMeshProUGUI m_HeightText;         // Shows current height
    [SerializeField] private TextMeshProUGUI m_MaxHeightText;      // Shows best height achieved
    
    [Header("Game Over Panel")]
    [SerializeField] private GameObject m_GameOverPanel;           // Container for game over UI
    [SerializeField] private TextMeshProUGUI m_GameOverText;      // Game over message
    [SerializeField] private Button m_RestartButton;              // Button to restart game
    
    [Header("Upgrade UI")]
    // Buttons for different upgrade types
    [SerializeField] private Button m_SpeedUpgradeButton;
    [SerializeField] private Button m_FuelUpgradeButton;
    [SerializeField] private Button m_TurnSpeedUpgradeButton;
    
    // Text displays for upgrade stats
    [SerializeField] private TextMeshProUGUI m_SpeedStatsText;
    [SerializeField] private TextMeshProUGUI m_FuelStatsText;
    [SerializeField] private TextMeshProUGUI m_TurnSpeedStatsText;
    
    // Text displays for upgrade costs
    [SerializeField] private TextMeshProUGUI m_SpeedUpgradeCostText;
    [SerializeField] private TextMeshProUGUI m_FuelUpgradeCostText;
    [SerializeField] private TextMeshProUGUI m_TurnSpeedUpgradeCostText;
    
    [Header("Visual Settings")]
    [SerializeField] private Color m_FullFuelColor = Color.green;  // Color when fuel is full
    [SerializeField] private Color m_LowFuelColor = Color.red;     // Color when fuel is low
    #endregion

    #region Private Fields
    private RocketController m_Rocket;        // Reference to player's rocket
    private GameManager m_GameManager;        // Reference to game manager
    private UpgradeManager m_UpgradeManager;  // Reference to upgrade system
    #endregion

    #region Unity Lifecycle
    /// <summary>
    /// Initialize UI and set up event listeners
    /// </summary>
    private void Start()
    {
        // Get necessary component references
        m_Rocket = FindFirstObjectByType<RocketController>();
        m_GameManager = GameManager.Instance;
        m_UpgradeManager = FindFirstObjectByType<UpgradeManager>();

        // Subscribe to game events
        if (m_GameManager != null)
        {
            m_GameManager.OnGoldChanged += UpdateGoldDisplay;
            m_GameManager.OnMetalChanged += UpdateMetalDisplay;
            m_GameManager.OnEnergyChanged += UpdateEnergyDisplay;
            m_GameManager.OnGameOver += ShowGameOver;
        }

        // Initialize UI state
        UpdateGoldDisplay(m_GameManager.Gold);
        UpdateMetalDisplay(m_GameManager.Metal);
        UpdateEnergyDisplay(m_GameManager.Energy);
        m_GameOverPanel?.SetActive(false);

        // Set up button listeners
        SetupButtonListeners();
    }

    /// <summary>
    /// Update dynamic UI elements
    /// </summary>
    private void Update()
    {
        if (m_Rocket != null)
        {
            UpdateFuelDisplay();
            UpdateHeightDisplay();
        }
    }
    #endregion

    #region UI Update Methods
    /// <summary>
    /// Updates the gold counter display
    /// </summary>
    private void UpdateGoldDisplay(float _gold)
    {
        if (m_GoldText != null)
        {
            m_GoldText.text = $"{_gold:F0}";
        }
    }

    /// <summary>
    /// Updates the metal counter display
    /// </summary>
    private void UpdateMetalDisplay(float _metal)
    {
        if (m_MetalText != null)
        {
            m_MetalText.text = $"{_metal:F0}";
        }
    }

    /// <summary>
    /// Updates the energy counter display
    /// </summary>
    private void UpdateEnergyDisplay(float _energy)
    {
        if (m_EnergyText != null)
        {
            m_EnergyText.text = $"{_energy:F0}";
        }
    }

    /// <summary>
    /// Updates the fuel gauge and its color
    /// </summary>
    private void UpdateFuelDisplay()
    {
        if (m_FuelSlider != null)
        {
            float fuelRatio = m_Rocket.CurrentFuel / m_Rocket.MaxFuel;
            m_FuelSlider.value = fuelRatio;
            
            // Interpolate color based on fuel level
            Image fillImage = m_FuelSlider.fillRect.GetComponent<Image>();
            if (fillImage != null)
            {
                fillImage.color = Color.Lerp(m_LowFuelColor, m_FullFuelColor, fuelRatio);
            }
        }
    }

    /// <summary>
    /// Updates current and best height displays
    /// </summary>
    private void UpdateHeightDisplay()
    {
        if (m_HeightText != null)
        {
            m_HeightText.text = $"{m_Rocket.CurrentHeight:F0}";
        }

        if (m_MaxHeightText != null)
        {
            m_MaxHeightText.text = $"Best Height: {m_Rocket.MaxHeight:F0}m";
        }
    }
    #endregion

    #region Game Over Handling
    /// <summary>
    /// Shows game over UI and updates upgrade options
    /// </summary>
    private void ShowGameOver()
    {
        m_GameOverPanel?.SetActive(true);
        if (m_GameOverText != null)
        {
            m_GameOverText.text = "Game Over!";
        }
        UpdateUpgradeButtons();
    }

    /// <summary>
    /// Updates all upgrade button states and costs
    /// </summary>
    private void UpdateUpgradeButtons()
    {
        if (m_UpgradeManager == null) return;

        UpdateUpgradeButton(UpgradeType.Speed, m_SpeedUpgradeButton, m_SpeedStatsText, m_SpeedUpgradeCostText);
        UpdateUpgradeButton(UpgradeType.FuelCapacity, m_FuelUpgradeButton, m_FuelStatsText, m_FuelUpgradeCostText);
        UpdateUpgradeButton(UpgradeType.TurnSpeed, m_TurnSpeedUpgradeButton, m_TurnSpeedStatsText, m_TurnSpeedUpgradeCostText);
    }

    /// <summary>
    /// Updates a single upgrade button's state and text
    /// </summary>
    private void UpdateUpgradeButton(UpgradeType type, Button button, TextMeshProUGUI statsText, TextMeshProUGUI costText)
    {
        if (button == null) return;

        button.interactable = m_UpgradeManager.CanAffordUpgrade(type);
        var info = m_UpgradeManager.GetUpgradeInfo(type);

        if (statsText != null) statsText.text = $"{info.currentValue:F1}";
        if (costText != null) costText.text = $"{info.cost:F0} Gold";
    }
    #endregion

    #region Button Handlers
    /// <summary>
    /// Attempts to purchase and apply an upgrade
    /// </summary>
    private void TryUpgrade(UpgradeType type)
    {
        if (m_UpgradeManager != null && m_UpgradeManager.TryUpgrade(type))
        {
            UpdateUpgradeButtons();
        }
    }

    /// <summary>
    /// Handles game restart
    /// </summary>
    private void RestartGame()
    {
        m_GameOverPanel?.SetActive(false);
        m_GameManager?.RestartGame();
    }

    /// <summary>
    /// Sets up all button click listeners
    /// </summary>
    private void SetupButtonListeners()
    {
        m_RestartButton?.onClick.AddListener(RestartGame);
        m_SpeedUpgradeButton?.onClick.AddListener(() => TryUpgrade(UpgradeType.Speed));
        m_FuelUpgradeButton?.onClick.AddListener(() => TryUpgrade(UpgradeType.FuelCapacity));
        m_TurnSpeedUpgradeButton?.onClick.AddListener(() => TryUpgrade(UpgradeType.TurnSpeed));
    }
    #endregion

    #region Cleanup
    /// <summary>
    /// Clean up event subscriptions when destroyed
    /// </summary>
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (m_GameManager != null)
        {
            m_GameManager.OnGoldChanged -= UpdateGoldDisplay;
            m_GameManager.OnMetalChanged -= UpdateMetalDisplay;
            m_GameManager.OnEnergyChanged -= UpdateEnergyDisplay;
            m_GameManager.OnGameOver -= ShowGameOver;
        }

        // Remove button listeners
        m_RestartButton?.onClick.RemoveListener(RestartGame);
        m_SpeedUpgradeButton?.onClick.RemoveListener(() => TryUpgrade(UpgradeType.Speed));
        m_FuelUpgradeButton?.onClick.RemoveListener(() => TryUpgrade(UpgradeType.FuelCapacity));
        m_TurnSpeedUpgradeButton?.onClick.RemoveListener(() => TryUpgrade(UpgradeType.TurnSpeed));
    }
    #endregion
} 