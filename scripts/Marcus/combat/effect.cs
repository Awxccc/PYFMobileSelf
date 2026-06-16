using UnityEngine;
    
    /** 
    @brief effect base class
    @details effects work by ovveriding activate_effect to apply the effect to the target entity and are used by buffs and hats and others
    */
public abstract class effects : ScriptableObject
{
    
    public int pow;
    public bool selftarget;
    public abstract void activate_effect( entity target);
}
