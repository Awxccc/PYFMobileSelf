using UnityEngine;
[CreateAssetMenu (fileName ="discard", menuName ="effects/heal")]
public class heal_effect : effects
{
    public override void activate_effect(entity target)
    {
        if (target is Player_data)
        {
            Debug.Log("healing player for "+pow);
            Player_data player = (Player_data)target;
            player.change_hp(pow,true,true);
        }
    }
}