using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
[CreateAssetMenu(fileName = "New Hat", menuName = "Hat")]
public class hat : ScriptableObject
{
        
    /** 
    @brief stores data for hats
    @details abit hacky since i wanted data to be seralizable.
    */
    public hat_data hat_info;
    //might add upgrades?
    [System.Serializable]
    public class hat_data
    {
            
        public Sprite icon;
        public string name;
        public hat_type type;
        public int level = 0;
        public bool exhaust = false;//if it is true, will not be added back into deck after use
        public bool retain = false;//if true, will not be discarded at end of turn
        public GameObject hat_model;//the 3d model to be used for the hat
        [SerializeField]public List<effect_disc> description;//for all values that can are effected by pow , use <pow> in the description string to indicate where the value should go
        
        [SerializeField]private string detailed_description="";
        [SerializeField]private int pow;//for dmg and block etc
        public int pow_mod=0;
        public List<effects> use_effects;
        [Header("upgraded stats")]
        
        [SerializeField]private int upgraded_pow=-1; //-1 means no upgrade
        [SerializeField]public List<effect_disc> upgraded_description=new List<effect_disc>();
        [SerializeField]private string upgraded_detailed_description="";
        
        public List<effects> upgraded_use_effects;
    
        public hat_data copy()
        {
            hat_data new_hat = new hat_data();
            new_hat.icon = icon;
            new_hat.name = name;
            new_hat.type = type;
            new_hat.description = description;
            new_hat.detailed_description = detailed_description;
            new_hat.upgraded_detailed_description = upgraded_detailed_description;
            new_hat.pow = pow;
            new_hat.pow_mod = pow_mod;
            new_hat.level = level;
            new_hat.use_effects = new List<effects>(use_effects);
            new_hat.upgraded_pow = upgraded_pow;
            new_hat.upgraded_use_effects = new List<effects>(upgraded_use_effects);
            new_hat.hat_model = hat_model;
            return new_hat;
        }

        public int get_pow()
        {
            if(level>0 && upgraded_pow != -1)
            {
                return upgraded_pow+pow_mod;
            }
            return pow+pow_mod;
        }
            
        /** 
        @brief returns a list of effect_discs for displaying hat effects and stats, with pow subbed in for icons
        */
        public List<effect_disc> get_icon_description()
        {
            List<effect_disc>result = new List<effect_disc>();
            if(level>0 && upgraded_detailed_description != "")//if left blank will assume no upgraded to description needed
            {
                foreach(effect_disc disc in upgraded_description)
                {
                    //Debug.Log("Adding disc string: " + disc.effect_string);
                    result.Add(disc.copy());
                }
            }
            else
            {
                foreach(effect_disc disc in description)
                {
                    //Debug.Log("Adding disc string: " + disc.effect_string);
                    result.Add(disc.copy());
                }
            }
            string temp=pow.ToString();
            if (level > 0 && upgraded_pow != -1)
            {
                temp = "<color=yellow>" + upgraded_pow.ToString() + "</color>";
            }
            if (pow_mod < 0)
            {
                temp += "(<color=red>" + pow_mod.ToString() + "</color>)";
                //Debug.Log("Hat power mod is negative: " + temp);
            }
            else if (pow_mod > 0)
            {
                temp += "(+<color=green>" + pow_mod.ToString() + "</color>)";

            }
            //Debug.Log(name + " " + temp);
            foreach (effect_disc disc in result)
            {
                
                //Debug.Log("Trying to add string: " + disc.effect_string);
                if (disc.effect_string.Contains("<pow>"))
                {
                    disc.effect_string = disc.effect_string.Replace("<pow>", temp);
                }
                else if (disc.colored)
                {
                    if (int.TryParse(disc.effect_string, out int n))
                    {
                        
                        //Debug.Log("Trying to int: " + n);
                        if (n < 0)
                        {
                            disc.effect_string = "<color=red>" + disc.effect_string + "</color>";
                        }
                        else if (n > 0)
                        {
                            disc.effect_string = "<color=green>+" + disc.effect_string + "</color>";
                        }
                    }
                }
            }
            return result;
        }
            
        /** 
        @brief returns a detailed description of the hat, with pow subbed in for appropriate vals + clr
        */
        public string get_detailed_description()
        {
             string result;
            if(level>0 && upgraded_detailed_description != "")//if left blank will assume no upgraded to description needed
            {
                result = upgraded_detailed_description;
            }
            else
            {
                result = detailed_description;
            }
            if (type == hat_type.Critical)
            {
                return result;
            }
            string temp=pow.ToString();
            if(level>0 && upgraded_pow != -1)
            {
                temp  = "<color=yellow>"+upgraded_pow.ToString()+"</color>";
            }
            if (pow_mod < 0)
            {
                temp += "(" + pow_mod.ToString() + ")";
                Debug.Log("Hat power mod is negative: " + temp);
            }
            else if (pow_mod > 0)
            {
                temp += "(+" + pow_mod.ToString() + ")";
               
            }
            
            Debug.Log("new desc" + result.Replace("<pow>", temp));
            return result.Replace("<pow>", temp);
        }
    }
}
[System.Serializable]
    
/** 
@brief stores the data for displaying icons for things such as hat effects, enemy forcasts etc
*/
public class effect_disc
{
    public string effect_string;
    public Sprite effect_icon;
    public bool middle=false;
    public bool colored=false;
    public effect_disc copy()
    {
        effect_disc new_disc = new effect_disc();
        new_disc.effect_string = effect_string;
        new_disc.effect_icon = effect_icon;
        new_disc.middle = middle;
        new_disc.colored = colored;
        return new_disc;
    }
}
    
/** 
@brief types of hats
*/
public enum hat_type
{
    Transferable,
    Social,
    Critical
}
