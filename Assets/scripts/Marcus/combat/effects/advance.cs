using System;
using UnityEngine;

[CreateAssetMenu (fileName ="advance", menuName ="effects/advance")]
public class advance : effects
{
    static public Action<int> skip_turn;
    public override void activate_effect(entity target)
    {
        skip_turn?.Invoke(pow);
    }
}