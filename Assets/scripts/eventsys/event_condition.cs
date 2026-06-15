using System.Collections.Generic;
using UnityEngine;

public abstract class event_condition : ScriptableObject
{
    public List<event_reward> fail_reward;
    //maybe add a string description for failed condition?
    public abstract bool check_condition();
}
