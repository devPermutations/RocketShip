using UnityEngine;

/// <summary>
/// Defines the types of collectible items in the game
/// </summary>
public enum CollectibleType
{
    Gold,
    Fuel
}

/// <summary>
/// Handles collectible item behavior and collection logic
/// </summary>
public class Collectible : MonoBehaviour
{
    [SerializeField] private CollectibleType m_Type;     // Type of collectible
    [SerializeField] private float m_Value = 10f;        // Value/amount of the collectible

    [Header("Visual")]
    [SerializeField] private Color m_GoldColor = new Color(1f, 0.84f, 0f);    // Gold color
    [SerializeField] private Color m_FuelColor = new Color(0f, 0.8f, 0.2f);   // Green color

    public CollectibleType Type => m_Type;  // Public accessor for type

    private void Start()
    {
        // Get the renderer component
        if (TryGetComponent<Renderer>(out Renderer renderer))
        {
            // Set color based on type
            Color color = m_Type == CollectibleType.Gold ? m_GoldColor : m_FuelColor;
            
            // Create a new material instance to avoid sharing
            Material material = new Material(renderer.material);
            material.color = color;
            renderer.material = material;
        }
    }

    /// <summary>
    /// Handles collection when player touches the collectible
    /// </summary>
    private void OnTriggerEnter(Collider _other)
    {
        if (_other.TryGetComponent<RocketController>(out RocketController rocket))
        {
            switch (m_Type)
            {
                case CollectibleType.Gold:
                    GameManager.Instance.AddGold(m_Value);
                    break;
                case CollectibleType.Fuel:
                    rocket.AddFuel(m_Value);
                    break;
            }
            Destroy(gameObject);
        }
    }
} 