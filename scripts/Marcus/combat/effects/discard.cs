using UnityEngine;
[CreateAssetMenu (fileName ="discard", menuName ="effects/discard")]
public class discard : effects
{
    public override void activate_effect( entity target)
    {
     if(target is Player_data)//only players can discard cards (maybe will make a st un effect for enemeies later)
        {
            Player_data player = (Player_data)target;
            Debug.Log("discarding "+pow+" cards");
            //player.hand_ui.discard_x(pow);
        }
    }
}
