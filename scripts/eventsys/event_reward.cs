using UnityEngine;

[CreateAssetMenu(fileName = "event_reward", menuName = "Scriptable Objects/event_reward")]
public abstract class event_reward : ScriptableObject
{
    public abstract void grant_reward(entity target);
}
