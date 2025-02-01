using UnityEngine;

/// <summary>
/// Handles spawning of collectibles (fuel and gold) in the game world
/// </summary>
public class CollectibleSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject m_FuelPrefab;
    [SerializeField] private GameObject m_GoldPrefab;
    
    [Header("Spawn Settings")]
    [SerializeField] private float m_SpawnRadius = 50f;        // How far from center to spawn
    [SerializeField] private float m_MinSpawnHeight = 10f;     // Minimum height for spawns
    [SerializeField] private float m_MaxSpawnHeight = 100f;    // Maximum height for spawns
    [SerializeField] private int m_MaxCollectibles = 10;       // Maximum number of collectibles at once
    [SerializeField] private float m_SpawnInterval = 3f;       // Time between spawn attempts
    
    private float m_NextSpawnTime;
    private Transform m_PlayerTransform;

    private void Start()
    {
        m_PlayerTransform = FindFirstObjectByType<RocketController>()?.transform;
        m_NextSpawnTime = Time.time + m_SpawnInterval;
        
        // Initial spawns
        for (int i = 0; i < m_MaxCollectibles / 2; i++)
        {
            SpawnCollectible();
        }
    }

    private void Update()
    {
        if (Time.time >= m_NextSpawnTime)
        {
            // Count current collectibles
            int currentCount = GameObject.FindGameObjectsWithTag("Collectible").Length;
            
            // Spawn if below max
            if (currentCount < m_MaxCollectibles)
            {
                SpawnCollectible();
            }
            
            m_NextSpawnTime = Time.time + m_SpawnInterval;
        }
    }

    private void SpawnCollectible()
    {
        // Determine spawn position
        Vector3 spawnPos;
        float zPosition = m_PlayerTransform != null ? m_PlayerTransform.position.z : 0f;

        if (m_PlayerTransform != null)
        {
            // Spawn along X axis only, keeping Z constant
            float randomX = Random.Range(-m_SpawnRadius, m_SpawnRadius);
            spawnPos = m_PlayerTransform.position;
            spawnPos.x += randomX;
            spawnPos.z = zPosition;  // Keep Z coordinate same as rocket
        }
        else
        {
            // Spawn along X axis if no player found
            float randomX = Random.Range(-m_SpawnRadius, m_SpawnRadius);
            spawnPos = new Vector3(randomX, 0, zPosition);
        }

        // Add random height
        spawnPos.y = Random.Range(m_MinSpawnHeight, m_MaxSpawnHeight);

        // Determine which prefab to spawn (70% gold, 30% fuel)
        GameObject prefabToSpawn = Random.value < 0.7f ? m_GoldPrefab : m_FuelPrefab;

        // Spawn the collectible
        if (prefabToSpawn != null)
        {
            GameObject collectible = Instantiate(prefabToSpawn, spawnPos, Random.rotation);
            collectible.tag = "Collectible";
        }
        else
        {
            Debug.LogWarning("Collectible prefab not assigned!");
        }
    }

    // Optional: Visualize spawn area in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, m_SpawnRadius);
        
        // Draw height range
        Vector3 minHeight = transform.position;
        minHeight.y = m_MinSpawnHeight;
        Vector3 maxHeight = transform.position;
        maxHeight.y = m_MaxSpawnHeight;
        
        Gizmos.DrawLine(minHeight, maxHeight);
    }

    public void ResetSpawner()
    {
        // Reset spawn timer
        m_NextSpawnTime = Time.time + m_SpawnInterval;
        
        // Do initial spawns
        for (int i = 0; i < m_MaxCollectibles / 2; i++)
        {
            SpawnCollectible();
        }
    }
} 