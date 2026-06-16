using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class enemy_ui : BurnoutBar
{
    public Transform forcastatk;

    private List<icondsp> topLicons = new List<icondsp>();
    private List<icondsp> middleLicons = new List<icondsp>();
        
    /** 
    @brief loads and inits the enemy ui
    */
    protected override void Start()
    {
        base.Start();

        if (topLicons.Count <= 0)
        {
            icondsp temp = Resources.Load<icondsp>("topL");
            for (int i = 0; i <= 1; i++)
            {
                icondsp new_icon = Instantiate(temp, forcastatk);
                new_icon.gameObject.SetActive(false);
                topLicons.Add(new_icon);
            }
        }
        if (middleLicons.Count <= 0)
        {
            icondsp temp = Resources.Load<icondsp>("middle");
            for (int i = 0; i <= 1; i++)
            {
                icondsp new_icon = Instantiate(temp, forcastatk);
                new_icon.gameObject.SetActive(false);
                middleLicons.Add(new_icon);
            }
        }
    }
    public void clear_forcast()
    {
        foreach (icondsp icon in topLicons)
        {
            icon.gameObject.SetActive(false);
        }
        foreach (icondsp icon in middleLicons)
        {
            icon.gameObject.SetActive(false);
        }
    }
        
    /** 
    @brief loads and inits the enemy ui
    @param discs list of "icons" to display on the forcast attack ui
    */
     public void setup_disc(List<effect_disc> discs)
    {
        clear_forcast();
        if (topLicons.Count <= 0)
        {
            icondsp temp = Resources.Load<icondsp>("topL");
            for (int i = 0; i <= 1; i++)
            {
                icondsp new_icon = Instantiate(temp, forcastatk);
                new_icon.gameObject.SetActive(false);
                topLicons.Add(new_icon);
            }
        }
        if (middleLicons.Count <= 0)
        {
            icondsp temp = Resources.Load<icondsp>("middle");
            for (int i = 0; i <= 1; i++)
            {
                icondsp new_icon = Instantiate(temp, forcastatk);
                new_icon.gameObject.SetActive(false);
                middleLicons.Add(new_icon);
            }
        }
        foreach (effect_disc disc in discs)
        {
            Debug.Log("Setting up disc icon: " + disc.effect_string);
            if (disc.middle)
            {
                Debug.Log("Trying to set up topL icon for disc: " + disc.effect_string);
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

                Debug.Log("Trying to set up topL icon for disc: " + disc.effect_string + " " + topLicons.Count);
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
