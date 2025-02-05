using UnityEngine;

/// <summary>
/// Controls the rocket's movement, fuel management, and upgrade capabilities
/// </summary>
public class RocketController : MonoBehaviour
{
    #region Private Fields
    [Header("Movement Settings")]
    [SerializeField] private float m_BaseSpeed = 10f;
    [SerializeField] private float m_TurnSpeed = 180f;
    [SerializeField] private float m_GravityCompensation = 9.81f;
    
    [Header("Fuel Settings")]
    [SerializeField] private float m_MaxFuel = 100f;
    [SerializeField] private float m_FuelConsumptionRate = 10f;
    
    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem[] m_ThrusterParticles;  // Array of 4 thrusters
    
    [Header("Collision Settings")]
    [SerializeField] private float m_LiftoffHeight = 1f;     // Height considered as "lifted off"
    [SerializeField] private float m_SafeTime = 2f;          // Time after liftoff before crash check
    
    [Header("Mouse/Touch Control")]
    [SerializeField] private float m_RotationDamping = 0.5f;
    [SerializeField] private float m_MaxPointerDistance = 2f;
    [SerializeField] private float m_MinPointerDistance = 0.1f;

    [Header("Health Settings")]
    [SerializeField] private float m_MaxHealth = 100f;
    private float m_CurrentHealth;

    private float m_CurrentFuel;      // Current amount of fuel
    private Rigidbody m_Rigidbody;    // Reference to the rocket's Rigidbody component
    private bool m_IsAlive = true;    // Tracks if the rocket is still operational
    private bool m_HasLiftedOff = false;         // Tracks if rocket has lifted off
    private float m_LiftoffTime = 0f;            // Time when rocket lifted off
    private float m_CurrentHeight = 0f;         // Current height in this run
    private static float s_MaxHeight = 0f;      // All-time max height across runs

    private bool m_IsPointerDown = false;
    private Vector3 m_TargetPosition;
    private Camera m_MainCamera;
    private Vector3 m_LastValidDirection;
    private bool m_WantsToThrust = false;  // Add this field to track thrust intent

    #endregion

    #region Properties
    public float CurrentFuel => m_CurrentFuel;   // Public access to current fuel level
    public float MaxFuel => m_MaxFuel;           // Public access to max fuel capacity
    public bool IsAlive => m_IsAlive;            // Public access to alive status
    public float BaseSpeed => m_BaseSpeed;        // Public access to base speed
    public float TurnSpeed => m_TurnSpeed;        // Public access to turn speed
    public float CurrentHeight => m_CurrentHeight;
    public float MaxHeight => s_MaxHeight;      // Best height ever achieved
    public bool HasFuel => m_CurrentFuel > 0;
    public float MaxHealth => m_MaxHealth;
    public float CurrentHealth => m_CurrentHealth;
    #endregion

    #region Unity Lifecycle
    /// <summary>
    /// Initialize components and fuel on start
    /// </summary>
    private void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        m_CurrentFuel = m_MaxFuel;
        m_IsAlive = true;
        m_CurrentHealth = m_MaxHealth;  // Initialize health

        // Make sure all particle systems start off
        if (m_ThrusterParticles != null)
        {
            foreach (var thruster in m_ThrusterParticles)
            {
                if (thruster != null)
                {
                    thruster.Stop();
                    thruster.gameObject.SetActive(true);
                }
            }
        }
        
        // Configure Rigidbody for better flight control
        m_Rigidbody.freezeRotation = true;
        m_Rigidbody.linearDamping = 0.5f;
        m_Rigidbody.angularDamping = 0.5f;
        m_Rigidbody.mass = 1f; // Ensure mass is reasonable

        // Set up camera follow
        CameraFollow cameraFollow = FindFirstObjectByType<CameraFollow>();
        if (cameraFollow != null)
        {
            cameraFollow.SetTarget(transform);
        }

        m_MainCamera = Camera.main;
    }

    /// <summary>
    /// Handle input and fuel management each frame
    /// </summary>
    private void Update()
    {
        // Handle input detection in Update for better responsiveness
        HandleInputDetection();
    }

    private void FixedUpdate()
    {
        if (!m_IsAlive) return;

        // Apply physics based on input state
        ApplyMovement();
        ConsumeFuel();
        CheckFuel();
        StabilizeRocket();
        CheckLiftoff();
        UpdateMaxHeight();
    }

    private void OnDrawGizmos()
    {
        // Draw rocket's up direction
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, transform.up * 2f);
        
        // Draw world up direction
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, Vector3.up * 2f);
    }

    private void CheckLiftoff()
    {
        if (!m_HasLiftedOff && transform.position.y > m_LiftoffHeight)
        {
            m_HasLiftedOff = true;
            m_LiftoffTime = Time.time;
            Debug.Log("Rocket has lifted off!");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Collided with: {collision.gameObject.name}, Tag: {collision.gameObject.tag}");
        
        // Check if we've lifted off and enough time has passed
        if (m_HasLiftedOff && Time.time > m_LiftoffTime + m_SafeTime)
        {
            // Check if we're hitting the ground
            if (collision.gameObject.CompareTag("Ground"))
            {
                Debug.Log("Crashed into ground!");
                Crash();
            }
        }
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Checks if fuel is depleted and triggers crash if necessary
    /// </summary>
    private void CheckFuel()
    {
        if (m_CurrentFuel <= 0)
        {
            m_CurrentFuel = 0;
            Crash();
        }
    }

    /// <summary>
    /// Applies a small force to keep the rocket mostly upright
    /// </summary>
    private void StabilizeRocket()
    {
        Vector3 upDirection = Vector3.up;
        Vector3 rocketUp = transform.up;
        
        // Calculate the rotation needed to stay upright
        Vector3 stabilizationTorque = Vector3.Cross(rocketUp, upDirection);
        m_Rigidbody.AddTorque(stabilizationTorque * 0.5f);
    }

    /// <summary>
    /// Handles rocket crash when fuel is depleted
    /// </summary>
    private void Crash()
    {
        if (m_CurrentHealth <= 0)
        {
            m_IsAlive = false;
            GameManager.Instance.GameOver();
        }
        else
        {
            // Take damage instead of instant death
            m_CurrentHealth -= 25f; // Adjust damage amount as needed
        }
    }

    private void UpdateMaxHeight()
    {
        m_CurrentHeight = transform.position.y;
        if (m_CurrentHeight > s_MaxHeight)
        {
            s_MaxHeight = m_CurrentHeight;
        }
    }

    private void HandleThrusters(bool isThrusting)
    {
        // Handle particle effects
        if (m_ThrusterParticles != null)
        {
            foreach (var thruster in m_ThrusterParticles)
            {
                if (thruster != null)
                {
                    // Always ensure GameObject is active
                    thruster.gameObject.SetActive(true);

                    if (isThrusting)
                    {
                        if (!thruster.isPlaying)
                        {
                            thruster.Clear(); // Clear any old particles
                            thruster.Play();
                        }
                    }
                    else
                    {
                        thruster.Stop();
                    }
                }
            }
        }

        // Handle sound through SoundManager
        SoundManager.Instance?.SetThrusterSound(isThrusting);
    }

    private void HandleInputDetection()
    {
        // Handle mouse/touch input first
        if (Input.GetMouseButtonDown(0))
        {
            m_IsPointerDown = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            m_IsPointerDown = false;
            m_WantsToThrust = false;
            HandleThrusters(false);
            return;
        }

        // Update thrust intent based on input method
        if (m_IsPointerDown)
        {
            UpdatePointerTarget();
        }
        else
        {
            m_WantsToThrust = Input.GetKey(KeyCode.W);
        }
    }

    private void UpdatePointerTarget()
    {
        if (m_MainCamera == null) return;

        // Convert mouse position to world space
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = transform.position.z - m_MainCamera.transform.position.z;
        m_TargetPosition = m_MainCamera.ScreenToWorldPoint(mousePos);
        m_TargetPosition.z = transform.position.z;

        // Calculate direction to target
        Vector3 direction = m_TargetPosition - transform.position;
        
        // Update direction and enable thrust while mouse is held
        m_LastValidDirection = direction.normalized;
        m_WantsToThrust = true;
    }

    private void ApplyMovement()
    {
        if (!m_WantsToThrust || m_CurrentFuel <= 0)
        {
            HandleThrusters(false);
            return;
        }

        // Apply thrust
        Vector3 thrustForce = transform.up * (m_BaseSpeed + m_GravityCompensation);
        m_Rigidbody.AddForce(thrustForce * Time.fixedDeltaTime, ForceMode.Force);
        HandleThrusters(true);
        ConsumeFuel();

        // Handle rotation
        if (m_IsPointerDown)
        {
            // Mouse/touch rotation
            Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, m_LastValidDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, m_RotationDamping * Time.fixedDeltaTime);
        }
        else
        {
            // Keyboard rotation
            float rotation = 0f;
            if (Input.GetKey(KeyCode.A)) rotation = m_TurnSpeed;
            if (Input.GetKey(KeyCode.D)) rotation = -m_TurnSpeed;
            
            if (rotation != 0)
            {
                transform.Rotate(Vector3.forward * rotation * Time.fixedDeltaTime);
            }
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Adds fuel to the rocket
    /// </summary>
    /// <param name="_amount">Amount of fuel to add</param>
    public void AddFuel(float _amount)
    {
        m_CurrentFuel = Mathf.Min(m_CurrentFuel + _amount, m_MaxFuel);
    }

    /// <summary>
    /// Upgrades the rocket's speed
    /// </summary>
    /// <param name="_multiplier">Speed increase multiplier</param>
    public void UpgradeSpeed(float _multiplier)
    {
        m_BaseSpeed *= _multiplier;
    }

    /// <summary>
    /// Upgrades the rocket's fuel capacity
    /// </summary>
    /// <param name="_multiplier">Fuel capacity increase multiplier</param>
    public void UpgradeFuelCapacity(float _multiplier)
    {
        m_MaxFuel *= _multiplier;
    }

    /// <summary>
    /// Upgrades the turn speed by the specified multiplier
    /// </summary>
    /// <param name="_multiplier">Turn speed increase multiplier</param>
    public void UpgradeTurnSpeed(float _multiplier)
    {
        m_TurnSpeed *= _multiplier;
    }

    /// <summary>
    /// Upgrades the health of the rocket
    /// </summary>
    /// <param name="_multiplier">Health increase multiplier</param>
    public void UpgradeHealth(float _multiplier)
    {
        m_MaxHealth *= _multiplier;
        m_CurrentHealth = m_MaxHealth;  // Heal to full when upgrading
    }

    /// <summary>
    /// Loads saved rocket stats
    /// </summary>
    /// <param name="_data">Saved game data to load from</param>
    public void LoadStats(GameData _data)
    {
        if (_data == null) return;
        
        m_BaseSpeed = _data.baseSpeed;
        m_MaxFuel = _data.maxFuel;
        m_TurnSpeed = _data.turnSpeed;
        m_MaxHealth = _data.maxHealth;  // Load health
        m_CurrentHealth = m_MaxHealth;
        m_CurrentFuel = m_MaxFuel;
    }

    /// <summary>
    /// Resets the rocket to its initial state
    /// </summary>
    public void ResetRocket()
    {
        // Stop all physics movement immediately
        m_Rigidbody.isKinematic = true;  // Temporarily disable physics
        
        // Reset position and rotation
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        
        // Reset all state variables
        m_CurrentFuel = m_MaxFuel;
        m_IsAlive = true;
        m_HasLiftedOff = false;
        m_LiftoffTime = 0f;
        m_CurrentHeight = 0f;
        m_CurrentHealth = m_MaxHealth;  // Reset health
        
        // Reset physics
        m_Rigidbody.isKinematic = false;  // Re-enable physics
        m_Rigidbody.linearVelocity = Vector3.zero;
        m_Rigidbody.angularVelocity = Vector3.zero;
        
        // Ensure rigidbody settings are correct
        m_Rigidbody.freezeRotation = true;
        m_Rigidbody.linearDamping = 0.5f;
        m_Rigidbody.angularDamping = 0.5f;

        // Stop all particle effects
        if (m_ThrusterParticles != null)
        {
            foreach (var thruster in m_ThrusterParticles)
            {
                if (thruster != null)
                {
                    thruster.Stop();
                }
            }
        }

        // Stop sounds through SoundManager
        SoundManager.Instance?.StopRocketSounds();
    }

    private void ConsumeFuel()
    {
        m_CurrentFuel -= m_FuelConsumptionRate * Time.fixedDeltaTime;
        if (m_CurrentFuel <= 0)
        {
            m_CurrentFuel = 0;
            Crash();
        }
    }
    #endregion
} 