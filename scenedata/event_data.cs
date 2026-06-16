using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "event_data", menuName = "Events/event_data")]
public class event_data : ScriptableObject
{
    public string scene_name= "";
    public int scene_gacha_rate=1;//used for getting hats
    public string scene_description;
    public bool override_scene = false;
    public string override_scene_name = "";
    public List<GameObject> enemies_for_spawning = new List<GameObject>();
    public GameObject event_scene_prefab;//the prefab to be used for the event scene
}
