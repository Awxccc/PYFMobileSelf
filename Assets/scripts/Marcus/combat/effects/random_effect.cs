using System;
using UnityEngine;

[CreateAssetMenu (fileName ="random_effect", menuName ="effects/random_effect")]
public class random_effect :  effects
{
    public effects[] possible_effects;
    public override void activate_effect(entity target)
    {
        
            for(int i=0;i<pow;i++)
            {
                
                UnityEngine.Random.InitState(DateTime.Now.Millisecond+System.Environment.TickCount+System.Diagnostics.Process.GetCurrentProcess().Id+i);
                effects chosen_effect = possible_effects[UnityEngine.Random.Range(0, possible_effects.Length)];
                chosen_effect.activate_effect(target);
            }
    }
}
