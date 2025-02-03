using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Handles spawning of collectibles (fuel and gold) in the game world
/// </summary>
public class CollectibleSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject m_FuelPrefab;
    [SerializeField] private GameObject m_GoldPrefab;
    [SerializeField] private GameObject m_EnergyPrefab;    // New Energy prefab
    [SerializeField] private GameObject m_MetalPrefab;     // New Metal prefab
    
    [Header("Spawn Settings")]
    [SerializeField] private float m_SpawnRadius = 500f;        // How far from center to spawn
    [SerializeField] private float m_MinSpawnHeight = 40f;      // Minimum height for spawns
    [SerializeField] private float m_MaxSpawnHeight = 1000f;    // Maximum height for spawns
    [SerializeField] private float m_MinSpacing = 50f;         // Minimum distance between collectibles
    [SerializeField] private int m_MaxCollectibles = 1000;     // Maximum number of collectibles
    [SerializeField] private int m_InitialSpawnCount = 500;    // How many to spawn at start
    [SerializeField] private float m_SpawnInterval = 1f;       // Time between spawn attempts
    
    [Header("Spawn Chances")]
    [Range(0f, 1f)]
    [SerializeField] private float m_GoldChance = 0.5f;    // 50% chance for gold
    [Range(0f, 1f)]
    [SerializeField] private float m_FuelChance = 0.4f;    // 40% chance for fuel
    [Range(0f, 1f)]
    [SerializeField] private float m_EnergyChance = 0.05f;  // 5% chance for energy
    [Range(0f, 1f)]
    [SerializeField] private float m_MetalChance = 0.05f;   // 5% chance for metal

    private float m_NextSpawnTime;
    private Transform m_PlayerTransform;
    private HashSet<Vector2Int> m_OccupiedCells;  // 2D grid tracking

    private void Start()
    {
        m_PlayerTransform = FindFirstObjectByType<RocketController>()?.transform;
        m_NextSpawnTime = Time.time + m_SpawnInterval;
        m_OccupiedCells = new HashSet<Vector2Int>();
        
        SpawnInitialCollectibles();
    }

    private GameObject GetRandomPrefab()
    {
        // Normalize probabilities to ensure they sum to 1
        float totalChance = m_GoldChance + m_FuelChance + m_EnergyChance + m_MetalChance;
        if (totalChance <= 0) return m_GoldPrefab; // Fallback to gold if all chances are 0

        float random = Random.value * totalChance; // Scale random value to total probability
        float currentProb = 0f;

        // Check each probability range
        currentProb += m_GoldChance;
        if (random <= currentProb) return m_GoldPrefab;

        currentProb += m_FuelChance;
        if (random <= currentProb) return m_FuelPrefab;

        currentProb += m_EnergyChance;
        if (random <= currentProb) return m_EnergyPrefab;

        currentProb += m_MetalChance;
        if (random <= currentProb) return m_MetalPrefab;

        return m_GoldPrefab; // Fallback to gold if something goes wrong
    }

    private void SpawnInitialCollectibles()
    {
        Debug.Log($"Starting initial spawn of {m_InitialSpawnCount} collectibles...");
        
        // Calculate grid dimensions
        int gridSize = Mathf.CeilToInt(m_SpawnRadius * 2f / m_MinSpacing);
        List<Vector2Int> availableCells = new List<Vector2Int>();
        
        // Generate all possible grid positions
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                availableCells.Add(new Vector2Int(x, y));
            }
        }
        
        // Shuffle the list
        for (int i = availableCells.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var temp = availableCells[i];
            availableCells[i] = availableCells[j];
            availableCells[j] = temp;
        }
        
        int spawned = 0;
        float halfGrid = gridSize * m_MinSpacing * 0.5f;
        
        // Spawn collectibles using pre-shuffled positions
        for (int i = 0; i < Mathf.Min(m_InitialSpawnCount, availableCells.Count); i++)
        {
            Vector2Int cell = availableCells[i];
            
            // Convert grid position to world position
            float x = (cell.x * m_MinSpacing) - halfGrid;
            float y = Random.Range(m_MinSpawnHeight, m_MaxSpawnHeight);
            float z = m_PlayerTransform != null ? m_PlayerTransform.position.z : 0f;
            
            Vector3 spawnPos = new Vector3(x, y, z);
            
            // Spawn the collectible using weighted random selection
            GameObject prefabToSpawn = GetRandomPrefab();
            if (prefabToSpawn != null)
            {
                GameObject collectible = Instantiate(prefabToSpawn, spawnPos, Random.rotation, transform);
                collectible.tag = "Collectible";
                m_OccupiedCells.Add(cell);
                spawned++;
            }
        }
        
        Debug.Log($"Successfully spawned {spawned} collectibles");
    }

    private void Update()
    {
        if (Time.time >= m_NextSpawnTime)
        {
            int currentCount = GameObject.FindGameObjectsWithTag("Collectible").Length;
            if (currentCount < m_MaxCollectibles)
            {
                TrySpawnCollectible();
            }
            m_NextSpawnTime = Time.time + m_SpawnInterval;
        }
    }

    private bool TrySpawnCollectible()
    {
        // Find a random unoccupied cell near the player
        float gridSize = m_SpawnRadius * 2f / m_MinSpacing;
        for (int i = 0; i < 10; i++) // Limit attempts
        {
            Vector2Int cell = new Vector2Int(
                Random.Range(0, Mathf.CeilToInt(gridSize)),
                Random.Range(0, Mathf.CeilToInt(gridSize))
            );
            
            if (!m_OccupiedCells.Contains(cell))
            {
                float x = (cell.x * m_MinSpacing) - m_SpawnRadius;
                float y = Random.Range(m_MinSpawnHeight, m_MaxSpawnHeight);
                float z = m_PlayerTransform != null ? m_PlayerTransform.position.z : 0f;
                
                Vector3 spawnPos = new Vector3(x, y, z);
                GameObject prefabToSpawn = GetRandomPrefab();
                
                if (prefabToSpawn != null)
                {
                    GameObject collectible = Instantiate(prefabToSpawn, spawnPos, Random.rotation, transform);
                    collectible.tag = "Collectible";
                    m_OccupiedCells.Add(cell);
                    return true;
                }
            }
        }
        return false;
    }

    public void ResetSpawner()
    {
        m_OccupiedCells.Clear();
        m_NextSpawnTime = Time.time + m_SpawnInterval;
        SpawnInitialCollectibles();
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
} 