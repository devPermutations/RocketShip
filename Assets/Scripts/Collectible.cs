using UnityEngine;

/// <summary>
/// Handles collectible item behavior and collection logic
/// </summary>
public class Collectible : MonoBehaviour
{
    [SerializeField] private CollectibleType m_Type;     // Type of collectible
    [SerializeField] private float m_Value = 10f;        // Value/amount of the collectible

    public CollectibleType Type => m_Type;  // Public accessor for type

    /// <summary>
    /// Handles collection when player touches the collectible
    /// </summary>
    private void OnTriggerEnter(Collider _other)
    {
        if (_other.TryGetComponent<RocketController>(out RocketController rocket))
        {
            // Play pickup sound
            SoundManager.Instance?.PlayCollectibleSound(m_Type, transform.position);

            // Apply effect based on type
            switch (m_Type)
            {
                case CollectibleType.Gold:
                    GameManager.Instance.AddGold(m_Value);
                    break;
                case CollectibleType.Fuel:
                    rocket.AddFuel(m_Value);
                    break;
                case CollectibleType.Energy:
                    GameManager.Instance.AddEnergy(m_Value);  // Add energy to GameManager
                    break;
                case CollectibleType.Metal:
                    GameManager.Instance.AddMetal(m_Value);   // Add metal to GameManager
                    break;
            }
            
            Destroy(gameObject);
        }
    }
} 