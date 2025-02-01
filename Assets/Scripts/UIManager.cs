using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Manages the game's UI elements including HUD displays for gold and fuel
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI m_GoldText;
    [SerializeField] private Slider m_FuelSlider;
    [SerializeField] private TextMeshProUGUI m_HeightText;
    [SerializeField] private TextMeshProUGUI m_MaxHeightText;
    
    [Header("Game Over UI")]
    [SerializeField] private GameObject m_GameOverPanel;
    [SerializeField] private TextMeshProUGUI m_GameOverText;
    [SerializeField] private Button m_RestartButton;
    [SerializeField] private Button m_SpeedUpgradeButton;
    [SerializeField] private Button m_FuelUpgradeButton;
    [SerializeField] private Button m_TurnSpeedUpgradeButton;
    [SerializeField] private TextMeshProUGUI m_SpeedStatsText;
    [SerializeField] private TextMeshProUGUI m_FuelStatsText;
    [SerializeField] private TextMeshProUGUI m_TurnSpeedStatsText;
    [SerializeField] private TextMeshProUGUI m_SpeedUpgradeCostText;
    [SerializeField] private TextMeshProUGUI m_FuelUpgradeCostText;
    [SerializeField] private TextMeshProUGUI m_TurnSpeedUpgradeCostText;
    
    [Header("Colors")]
    [SerializeField] private Color m_FullFuelColor = Color.green;
    [SerializeField] private Color m_LowFuelColor = Color.red;
    
    private RocketController m_Rocket;
    private GameManager m_GameManager;
    private UpgradeManager m_UpgradeManager;

    private void Start()
    {
        // Get references
        m_Rocket = FindFirstObjectByType<RocketController>();
        m_GameManager = GameManager.Instance;
        m_UpgradeManager = FindFirstObjectByType<UpgradeManager>();

        // Subscribe to events
        if (m_GameManager != null)
        {
            m_GameManager.OnGoldChanged += UpdateGoldDisplay;
            m_GameManager.OnGameOver += ShowGameOver;
        }

        // Initialize display
        UpdateGoldDisplay(m_GameManager.Gold);
        
        // Hide game over UI at start
        if (m_GameOverPanel != null)
        {
            m_GameOverPanel.SetActive(false);
        }

        // Set up restart button
        if (m_RestartButton != null)
        {
            m_RestartButton.onClick.AddListener(RestartGame);
        }

        // Set up upgrade buttons
        if (m_SpeedUpgradeButton != null)
            m_SpeedUpgradeButton.onClick.AddListener(() => TryUpgrade(UpgradeType.Speed));
        if (m_FuelUpgradeButton != null)
            m_FuelUpgradeButton.onClick.AddListener(() => TryUpgrade(UpgradeType.FuelCapacity));
        if (m_TurnSpeedUpgradeButton != null)
            m_TurnSpeedUpgradeButton.onClick.AddListener(() => TryUpgrade(UpgradeType.TurnSpeed));
    }

    private void Update()
    {
        if (m_Rocket != null)
        {
            UpdateFuelDisplay();
            UpdateHeightDisplay();
        }
    }

    private void UpdateGoldDisplay(float _gold)
    {
        if (m_GoldText != null)
        {
            m_GoldText.text = $"Gold: {_gold:F0}";
        }
    }

    private void UpdateFuelDisplay()
    {
        if (m_FuelSlider != null)
        {
            float fuelRatio = m_Rocket.CurrentFuel / m_Rocket.MaxFuel;
            m_FuelSlider.value = fuelRatio;
            
            // Update fuel bar color
            Image fillImage = m_FuelSlider.fillRect.GetComponent<Image>();
            if (fillImage != null)
            {
                fillImage.color = Color.Lerp(m_LowFuelColor, m_FullFuelColor, fuelRatio);
            }
        }
    }

    private void UpdateHeightDisplay()
    {
        if (m_HeightText != null)
        {
            m_HeightText.text = $"Height: {m_Rocket.CurrentHeight:F0}m";
        }

        if (m_MaxHeightText != null)
        {
            m_MaxHeightText.text = $"Best Height: {m_Rocket.MaxHeight:F0}m";
        }
    }

    private void ShowGameOver()
    {
        if (m_GameOverPanel != null)
        {
            m_GameOverPanel.SetActive(true);
        }

        if (m_GameOverText != null)
        {
            m_GameOverText.text = "Game Over!";
        }

        UpdateUpgradeButtons();
    }

    private void UpdateUpgradeButtons()
    {
        if (m_UpgradeManager == null) return;

        UpdateUpgradeButton(UpgradeType.Speed, m_SpeedUpgradeButton, m_SpeedStatsText, m_SpeedUpgradeCostText);
        UpdateUpgradeButton(UpgradeType.FuelCapacity, m_FuelUpgradeButton, m_FuelStatsText, m_FuelUpgradeCostText);
        UpdateUpgradeButton(UpgradeType.TurnSpeed, m_TurnSpeedUpgradeButton, m_TurnSpeedStatsText, m_TurnSpeedUpgradeCostText);
    }

    private void UpdateUpgradeButton(UpgradeType type, Button button, TextMeshProUGUI statsText, TextMeshProUGUI costText)
    {
        if (button == null) return;

        button.interactable = m_UpgradeManager.CanAffordUpgrade(type);
        var info = m_UpgradeManager.GetUpgradeInfo(type);

        if (statsText != null)
        {
            statsText.text = $"{info.currentValue:F1}";
        }

        if (costText != null)
        {
            costText.text = $"{info.cost:F0} Gold";
        }
    }

    private void TryUpgrade(UpgradeType type)
    {
        if (m_UpgradeManager != null && m_UpgradeManager.TryUpgrade(type))
        {
            UpdateUpgradeButtons();
        }
    }

    private void RestartGame()
    {
        // Hide game over UI
        if (m_GameOverPanel != null)
        {
            m_GameOverPanel.SetActive(false);
        }

        // Tell GameManager to restart
        if (m_GameManager != null)
        {
            m_GameManager.RestartGame();
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (m_GameManager != null)
        {
            m_GameManager.OnGoldChanged -= UpdateGoldDisplay;
            m_GameManager.OnGameOver -= ShowGameOver;
        }

        // Clean up button listener
        if (m_RestartButton != null)
        {
            m_RestartButton.onClick.RemoveListener(RestartGame);
        }

        // Clean up upgrade button listeners
        if (m_SpeedUpgradeButton != null)
            m_SpeedUpgradeButton.onClick.RemoveListener(() => TryUpgrade(UpgradeType.Speed));
        if (m_FuelUpgradeButton != null)
            m_FuelUpgradeButton.onClick.RemoveListener(() => TryUpgrade(UpgradeType.FuelCapacity));
        if (m_TurnSpeedUpgradeButton != null)
            m_TurnSpeedUpgradeButton.onClick.RemoveListener(() => TryUpgrade(UpgradeType.TurnSpeed));
    }
} 