using UnityEngine;

public class detailed_buff : mini_buff_icon
{
    public TMPro.TMP_Text buff_name_text;
    public TMPro.TMP_Text buff_description_text;
    public TMPro.TMP_Text duration_text;
    public override void setup_icon(buff buff_to_use)
    {
        ref_buff = buff_to_use;
        icon_image.sprite = buff_to_use.buff_icon;
        buff_name_text.text = buff_to_use.buff_name;
        if(buff_to_use.pow < 0)
        {
            
            buff_description_text.text =buff_to_use.buff_description.Replace("<pow>", buff_to_use.pow.ToString());
        }
        else if(buff_to_use.pow > 0)
        {
            buff_description_text.text =buff_to_use.buff_description.Replace("<pow>", "+"+buff_to_use.pow.ToString());
        }
        else
        {
            buff_description_text.text =buff_to_use.buff_description.Replace("<pow>", "");
        }
        duration_text.text = buff_to_use.duration.ToString()+" <sprite=12>";
    }
}
