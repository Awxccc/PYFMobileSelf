using System;
using UnityEngine;
[Serializable]
public class progress_data
{
    public string scene_name;
    public int player_hp;
    public int path_taken; //0-left,1-center,2-right,3-back(failed encounter, cross and reverse),4(skipped event, cross but no reverse)
    public scene_type selection_made;
    public string scene_description;
}
