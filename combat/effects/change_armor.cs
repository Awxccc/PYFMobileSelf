using UnityEngine;

[CreateAssetMenu (fileName ="discard", menuName ="effects/change_armor")]
public class change_armor : effects
{
    public override void activate_effect(entity target)
    {
        target.shield += pow;
        if (target.shield < 0)
        {
            target.shield = 0;
        }
    }   
}
