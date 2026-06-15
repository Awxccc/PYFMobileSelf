using UnityEngine;

using UnityEngine.EventSystems;
public class prize_option : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler//this is for displaying card info when not in combat(acts like a striped down version of combat_card_ui)
{
    public TMPro.TMP_Text card_name;
    public UnityEngine.UI.Image icon;
    public TMPro.TMP_Text description;
    public TMPro.TMP_Text cost;
    public hat.hat_data ref_hat;
    public Animator anim;
    public prize_selection owner;
    public void setup_card(hat.hat_data hat_to_use,prize_selection owner)
    {
        ref_hat = hat_to_use;
        icon.sprite = hat_to_use.icon;
        card_name.text = hat_to_use.name;
        description.text = hat_to_use.get_detailed_description();//for now 

    }
    virtual public void OnPointerEnter(PointerEventData eventData)
    {
    
        card_name.gameObject.SetActive(true);
    }
    virtual public void OnPointerExit(PointerEventData eventData)
    { 
        card_name.gameObject.SetActive(true);
    }
    virtual public void OnPointerClick(PointerEventData eventData)
    {
        owner.select_card(this);
    }
}
