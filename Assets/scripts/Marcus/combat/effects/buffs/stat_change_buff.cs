using UnityEngine;

[CreateAssetMenu(fileName = "stat_change_buff", menuName = "Scriptable Objects/stat_change_buff")]
public class stat_change_buff : buff
{
    public enum stat_to_change
    {
        atk,
        shield,
        defense,
    }
    
    public Sprite negative_icon;
    public stat_to_change stat_type;
    public override void activate_effect(entity target)
    {
        switch (stat_type)
        {
            case stat_to_change.atk:
                target.change_atk_mod(pow);
                Debug.Log("Increased atk mod by " + pow + " to " + target.atk_mod);
                break;
            case stat_to_change.shield:
                target.change_shield_mod(pow);
                Debug.Log("Increased shield mod by " + pow + " to " + target.shield_mod);
                break;
            case stat_to_change.defense:
                target.defense_mod += pow;
                break;
        }
    }
    public override void on_expire(entity target)
    {
        switch (stat_type)
        {
            case stat_to_change.atk:
                target.change_atk_mod(-pow) ;
                break;
            case stat_to_change.shield:
                target.change_shield_mod(-pow);
                break;
            case stat_to_change.defense:
                target.defense_mod -= pow;
                break;
        }
    }
}
