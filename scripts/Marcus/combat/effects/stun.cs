using UnityEngine;
[CreateAssetMenu (fileName ="stun", menuName ="effects/stun")]
public class stun : effects
{
    public buff stun_debuff;
    public override void activate_effect(entity target)//only works on enemies
    {//this implementation is abit fake in that it just removes their current queued hat and applies a fake debuff to show they are stunned
        target.queued_hat = null;
        target.add_buff(stun_debuff);
    }
}
