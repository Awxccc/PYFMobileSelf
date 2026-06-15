using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterClassData", menuName = "CharacterClassData")]
public class character_class_data : ScriptableObject
{
    public int max_health;
    public int max_energy;
    public int handsize;
    //heck even remaining retries???
    public string option_name;
    public string option_desc;
    public Color ui_color;//color theme for character select ui may use this for other uis
    public Texture[] textures= new Texture[3];//for expression changes
    public  List<hat> starting_hats = new List<hat>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
}
