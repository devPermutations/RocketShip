using UnityEngine;

/// <summary>
/// Makes the camera follow the target (rocket) while maintaining its orientation
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform m_Target;          // The rocket to follow
    [SerializeField] private Vector3 m_Offset = new Vector3(0, 5, -10); // Camera offset from target
    [SerializeField] private float m_SmoothSpeed = 5f;   // How smoothly the camera follows
    
    private Vector3 m_DesiredPosition;
    private Vector3 m_SmoothedPosition;

    private void LateUpdate()
    {
        if (m_Target == null) return;

        // Calculate the desired position
        m_DesiredPosition = m_Target.position + m_Offset;
        
        // Smoothly move the camera towards that position
        m_SmoothedPosition = Vector3.Lerp(transform.position, m_DesiredPosition, m_SmoothSpeed * Time.deltaTime);
        transform.position = m_SmoothedPosition;
    }

    /// <summary>
    /// Sets the target for the camera to follow
    /// </summary>
    public void SetTarget(Transform _target)
    {
        m_Target = _target;
    }
} 