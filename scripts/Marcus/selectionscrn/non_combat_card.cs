using UnityEngine;
using UnityEngine.EventSystems;

public class non_combat_card : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler//this is for displaying card info when not in combat(acts like a striped down version of combat_card_ui)
{
    public TMPro.TMP_Text card_name;
    public UnityEngine.UI.Image icon;
    public UnityEngine.UI.Image border;
    public TMPro.TMP_Text description;
    public hat.hat_data ref_hat;
    public Animation shake_anim ;
    public Transform effect_disp;
    public virtual void setup_card(hat.hat_data hat_to_use,entity owner)
    {
        ref_hat = hat_to_use;
        icon.sprite = hat_to_use.icon;
        card_name.text = hat_to_use.name;
        description.text = hat_to_use.get_detailed_description();//for now 

    }
    virtual public void OnPointerEnter(PointerEventData eventData)
    {
    
        card_name.gameObject.SetActive(true);
        icon.color = new Color(0.5f,0.5f,0.5f,1);
        effect_disp.gameObject.SetActive(true);
    }
    virtual public void OnPointerExit(PointerEventData eventData)
    { 
        card_name.gameObject.SetActive(true);
        
        icon.color = Color.white;
        effect_disp.gameObject.SetActive(true);
    }
    virtual public void OnPointerClick(PointerEventData eventData)
    {
    }
}
