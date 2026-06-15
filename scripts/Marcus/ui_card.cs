
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class ui_card : non_combat_card
{
    [HideInInspector] public handui parent_hand;
    public Transform card_effects_parent;
    private List<icondsp> topLicons = new List<icondsp>();
    private List<icondsp> middleLicons = new List<icondsp>();
    public entity owner;
    public override void setup_card(hat.hat_data hat_to_use,entity owner)
    {
        icon.color = Color.white;
        base.setup_card(hat_to_use,owner);
        if(topLicons.Count<=0)
        {
            icondsp temp =Resources.Load<icondsp>("topL");
            for(int i=0;i<=2;i++)
            {
                icondsp new_icon = Instantiate(temp,card_effects_parent);
                new_icon.gameObject.SetActive(false);
                topLicons.Add(new_icon);
            }
        }
        if(middleLicons.Count<=0)
        {
            icondsp temp =Resources.Load<icondsp>("middle");
            for(int i=0;i<=2;i++)
            {
                icondsp new_icon = Instantiate(temp,card_effects_parent);
                new_icon.gameObject.SetActive(false);
                middleLicons.Add(new_icon);
            }
        }
        setup_disc(hat_to_use.get_icon_description());
        description.text = hat_to_use.get_detailed_description();
        this.owner = owner;
    }
    public void Update()
    {

    }
    public override void OnPointerEnter(PointerEventData eventData)
    {
        parent_hand.hover_card(this);
    }
    public override void OnPointerExit(PointerEventData eventData)
    {
        parent_hand.unhover_card(this);
    }
    public override void OnPointerClick(PointerEventData eventData)
    {
        parent_hand.select_card(this);
    }
    public void setup_disc(List<effect_disc> discs)
    {
        foreach(icondsp icon in topLicons)
        {
            icon.gameObject.SetActive(false);
        }
        foreach(icondsp icon in middleLicons)
        {
            icon.gameObject.SetActive(false);
        }
        foreach (effect_disc disc in discs)
        {
            if (disc.middle)
            {
                foreach (icondsp icon in middleLicons)
                {
                    if (!icon.gameObject.activeSelf)
                    {
                        icon.setup_icon(disc);
                        icon.gameObject.SetActive(true);
                        break;
                    }
                }
            }
            else
            {
                foreach (icondsp icon in topLicons)
                {
                    if (!icon.gameObject.activeSelf)
                    {
                        icon.setup_icon(disc);
                        icon.gameObject.SetActive(true);
                        break;
                    }
                }
            }
        }
    }
}
