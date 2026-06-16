using Unity.Cinemachine;
using UnityEngine;

public class cam_shake : MonoBehaviour
{
    public CinemachineImpulseSource impulse_source;
    void Start()
    {
        entity.play_impulse_event += playscrnshake;
    }
    public void playscrnshake(float intensity)
    {
        impulse_source.GenerateImpulseWithForce(intensity);
    }
    void OnDestroy()
    {
        entity.play_impulse_event -= playscrnshake;
    }
}
