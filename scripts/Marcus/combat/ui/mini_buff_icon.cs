using UnityEngine;
using UnityEngine.UI;

public class mini_buff_icon : MonoBehaviour
{
    public Image icon_image;
    public TMPro.TMP_Text stack_text;
    public buff ref_buff;
    public virtual void setup_icon(buff buff_to_use)
    {
        ref_buff = buff_to_use;
        icon_image.sprite = buff_to_use.buff_icon;
        stack_text.gameObject.SetActive(true);
        if (buff_to_use is stat_change_buff)//kinda hacky way to show negative icons
        {
            stat_change_buff scb = (stat_change_buff)buff_to_use;
            if (scb.pow < 0)
            {
                icon_image.sprite = scb.negative_icon;
            }
        }
        if (buff_to_use.pow > 0)
        {
            stack_text.text = "<color=green>" + (buff_to_use.pow).ToString() + "</color>";
        }
        else if (buff_to_use.pow < 0)
        {

            stack_text.text = "<color=red>" + (buff_to_use.pow).ToString() + "</color>";
        }
        else
        {
        }
    }
}
