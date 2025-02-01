using UnityEngine;
using UnityEngine.UI;

public class RadarLogic : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform m_RadarPanel;      // UI panel that contains radar elements
    [SerializeField] private GameObject m_GoldIndicatorPrefab; // Prefab for gold indicators
    [SerializeField] private GameObject m_FuelIndicatorPrefab; // Prefab for fuel indicators
    [SerializeField] private GameObject m_PlayerIndicatorPrefab;  // New player indicator prefab
    
    [Header("Settings")]
    [SerializeField] private float m_DetectionRange = 200f;   // How far to detect collectibles
    [SerializeField] private Color m_GoldColor = Color.yellow;
    [SerializeField] private Color m_FuelColor = Color.green;
    [SerializeField] private Color m_PlayerColor = Color.red;    // Color for player indicator
    
    private Camera m_MainCamera;
    private Transform m_PlayerTransform;
    private RectTransform m_PlayerIndicator;  // Reference to the player's indicator

    private void Start()
    {
        m_MainCamera = Camera.main;
        m_PlayerTransform = FindFirstObjectByType<RocketController>()?.transform;

        // Create player indicator
        if (m_PlayerIndicatorPrefab != null && m_RadarPanel != null)
        {
            GameObject playerDot = Instantiate(m_PlayerIndicatorPrefab, m_RadarPanel);
            m_PlayerIndicator = playerDot.GetComponent<RectTransform>();
            
            // Set up the player indicator
            m_PlayerIndicator.anchorMin = new Vector2(0.5f, 0.5f);
            m_PlayerIndicator.anchorMax = new Vector2(0.5f, 0.5f);
            m_PlayerIndicator.pivot = new Vector2(0.5f, 0.5f);
            m_PlayerIndicator.anchoredPosition = Vector2.zero;  // Center of radar
            m_PlayerIndicator.sizeDelta = new Vector2(12f, 12f);  // Slightly larger than collectibles

            // Set color
            if (playerDot.TryGetComponent<Image>(out Image image))
            {
                image.color = m_PlayerColor;
            }
        }
    }

    private void Update()
    {
        if (m_PlayerTransform == null || m_RadarPanel == null) return;

        // Clear old indicators
        foreach (Transform child in m_RadarPanel)
        {
            if (child != m_PlayerIndicator.transform)  // Don't destroy the player indicator
            {
                Destroy(child.gameObject);
            }
        }

        // Find all collectibles
        GameObject[] collectibles = GameObject.FindGameObjectsWithTag("Collectible");
        
        foreach (GameObject collectible in collectibles)
        {
            // Get relative position from rocket to collectible
            Vector3 relativePos = collectible.transform.position - m_PlayerTransform.position;
            
            // Check if collectible is in range
            float distance = relativePos.magnitude;
            if (distance > m_DetectionRange) continue;

            // Use world space coordinates directly
            Vector2 radarPos = new Vector2(relativePos.x, relativePos.y);

            // Scale based on distance and radar size
            float halfWidth = m_RadarPanel.rect.width * 0.5f;
            float scale = halfWidth / m_DetectionRange;
            radarPos *= scale;

            // Clamp to radar bounds
            radarPos = Vector2.ClampMagnitude(radarPos, halfWidth);

            // Create indicator
            GameObject indicatorPrefab = collectible.GetComponent<Collectible>()?.Type == CollectibleType.Gold 
                ? m_GoldIndicatorPrefab 
                : m_FuelIndicatorPrefab;

            if (indicatorPrefab != null)
            {
                GameObject indicator = Instantiate(indicatorPrefab, m_RadarPanel);
                RectTransform indicatorRect = indicator.GetComponent<RectTransform>();
                
                // Set up the indicator transform
                indicatorRect.anchorMin = new Vector2(0.5f, 0.5f);
                indicatorRect.anchorMax = new Vector2(0.5f, 0.5f);
                indicatorRect.pivot = new Vector2(0.5f, 0.5f);
                indicatorRect.anchoredPosition = radarPos;
                indicatorRect.sizeDelta = new Vector2(10f, 10f);

                // Set color
                if (indicator.TryGetComponent<Image>(out Image image))
                {
                    image.color = collectible.GetComponent<Collectible>()?.Type == CollectibleType.Gold 
                        ? m_GoldColor 
                        : m_FuelColor;
                }
            }
        }
    }
} 