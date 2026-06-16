using UnityEngine;
[CreateAssetMenu (fileName ="discard", menuName ="effects/draw_cards")]
public class draw_cards : effects
{
    public override void activate_effect( entity target)
    {
     if(target is Player_data)//only players can draw cards
        {
            Player_data player = target as Player_data;
            //player.hand_ui.draw_x(pow);
        }
    }
}
