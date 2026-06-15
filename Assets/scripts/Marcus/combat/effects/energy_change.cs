using UnityEngine;
[CreateAssetMenu (fileName ="energy_change", menuName ="effects/energy_change")]
public class energy_change : effects
{
    public override void activate_effect(entity target)
    {
        if(target is Player_data)
        {
            Player_data player = (Player_data)target;
        }
    }
}
