using System.Collections.Generic;
using UnityEngine;

public class hideable_ui : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private List<Transform> ui_elements = new List<Transform>();
    public Animator anim;
    void Awake()
    {
        Player_data.hide_ui_event += toggle;
        combat_manager.hide_ui_event += toggle;
        for (int i = 0; i < transform.childCount; i++)
        {
            ui_elements.Add(transform.GetChild(i));
        }
    }
    public void toggle(bool show)
    {
        if (anim!= null)
        {
            Debug.Log("Toggling UI to "+show);
            anim.SetBool("fade", show);
        }

    }
    void OnDestroy()
    {
        
        Player_data.hide_ui_event -= toggle;
        combat_manager.hide_ui_event -= toggle;
    }
}
