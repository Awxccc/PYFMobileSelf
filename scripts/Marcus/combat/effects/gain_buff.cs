using UnityEngine;
[CreateAssetMenu (fileName ="gain_buff", menuName ="effects/gain_buff")]
public class gain_buff : effects
{
    public buff buff_to_gain;
    public override void activate_effect(entity target)
    {
        buff copy = Instantiate(buff_to_gain);
        copy.pow = this.pow;
        target.add_buff(copy);
    }
}
