using System;
using UnityEngine;
[CreateAssetMenu(fileName = "spawn_enemy", menuName = "effects/spawn_enemy")]
public class spawn_enemy : effects
{
    public GameObject enemy_to_spawn;
    public static Action<GameObject> spawn_ads;
    public override void activate_effect(entity target)
    {
        Debug.Log("Spawning enemy effect activated");
        spawn_ads?.Invoke(enemy_to_spawn);
    }
}
