using UnityEngine;

[CreateAssetMenu(fileName = "empty_buff", menuName = "Scriptable Objects/empty_buff")]
    
/** 
@brief generic buff class, overrides activate_effect to apply buff to target entity
@details buffs are a type of effect that have duration and can apply various effects to entities over4
*/
public class buff : effects
{
    public string buff_name;
    public string buff_description;
    public Sprite buff_icon;
    public GameObject buff_vfx;//note when removing buff make sure to remove vfx as well
    public int duration = 1;//in turns
    public buff_type type;
    bool middle = false;// this is for some buffs which need to display their potency in the middle vs top L 
    public virtual void on_expire(entity target)
    {//this is called when the buff expires(use this to remove stat changes etc)
    
    }
    public override void activate_effect(entity target)
    {//this is called when the buff is applied(use this to apply stat changes etc)

    }
    public enum buff_type
    {
        remove_on_expire,
        on_atk,
        on_defend,
        on_get_atk,
    }
}
