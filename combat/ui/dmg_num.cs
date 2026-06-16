using UnityEngine;

public class dmg_num : MonoBehaviour
{
    public TMPro.TMP_Text dmg_text;
    public Animation dmg_anim;
    public void init(int dmg_amount, bool is_heal, bool is_shield)
    {
        dmg_text.enabled = true;
        if (is_heal)
        {
            dmg_text.color = Color.green; 
            dmg_text.text = dmg_amount.ToString()+" <sprite=17>";
        }
        else if (is_shield)
        {
            dmg_text.color = Color.cyan;
            dmg_text.text = dmg_amount.ToString()+" <sprite=15>";
        }
        else
        {
            dmg_text.color = Color.white;
            dmg_text.text = dmg_amount.ToString()+" <sprite=6>";
        }
        dmg_anim.Play();
    }
}
