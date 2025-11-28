using UnityEngine;
using Unity.Cinemachine; // For Unity 6 / Cinemachine 3.0

[RequireComponent(typeof(CinemachineImpulseSource))]
public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }
    
    private CinemachineImpulseSource impulseSource;

    private void Awake()
    {
        Instance = this;
        impulseSource = GetComponent<CinemachineImpulseSource>();
        
        // Auto-add if missing
        if (impulseSource == null)
        {
            impulseSource = gameObject.AddComponent<CinemachineImpulseSource>();
        }
        
        // Configure default impulse profile if none exists
        impulseSource.DefaultVelocity = new Vector3(0, -1, 0); // Shake down
        impulseSource.ImpulseDefinition.ImpulseDuration = 0.2f;
    }

    public void Shake(float force)
    {
        if (impulseSource != null)
        {
            impulseSource.GenerateImpulse(force);
        }
    }
}
