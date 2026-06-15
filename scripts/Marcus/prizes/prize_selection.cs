using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class prize_selection : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public List<prize_option> prize_options;//max 3
    prize_option selected_option;
    [SerializeField] private Transform confirm_button;
    [SerializeField] private Transform image_parent;
    public combat_manager combat_manager_ref;

    void Start()
    {

    }
    public void load_prizes(List<hat.hat_data> avaliable_prizes)
    {
        confirm_button.gameObject.SetActive(false);
        foreach(var option in prize_options)
        {
            option.gameObject.SetActive(false);
            option.anim.SetBool("selected", false);
        }
        for(int i=0;i<prize_options.Count && i<avaliable_prizes.Count;i++)
        {
            
            prize_options[i].gameObject.SetActive(true);
            prize_options[i].setup_card(avaliable_prizes[i], this);
        }
    }
    public void select_card(prize_option new_selected_option)
    {
        
        confirm_button.gameObject.SetActive(true);
        if(selected_option != null)
        {
            selected_option.anim.SetBool("selected", false);
        }
        selected_option = new_selected_option;
        selected_option.anim.SetBool("selected", true);
    }
    public void confirm_selection(){
        
    
            StartCoroutine(getprizer());
    }
    public  IEnumerator getprizer()
    {
        image_parent.gameObject.SetActive(false);
        if(selected_option != null)
        {
            yield return StartCoroutine(Player_data.instance.add_hat(selected_option.ref_hat));
        
        }
        
        this.gameObject.SetActive(false);
        
        combat_manager_ref.end_combat(); 
    }
    public void cancel_selection()
    {
        selected_option = null;
        
        combat_manager_ref.end_combat();
        gameObject.SetActive(false);
    }

}
