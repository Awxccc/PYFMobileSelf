using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class presentation_boss : enemy
{
    private bool open_mode=false;
    [SerializeField] private  int phase_damage_shift_amount = 30;
    [SerializeField] private  int current_phase_damage_progress = 0;//for transitioning from open -> closed modes
    [SerializeField] private  int current_phase_ally_units_progress = 0;//for transitioning from closed -> open modes
    

    [SerializeField] private  List<hat.hat_data> open_deck_deck;
    [SerializeField] private  List<hat.hat_data> closed_deck_deck;
    [SerializeField] private GameObject additional_enemy_prefab;
    public static Action Boss_defeated_event ;
    public static Action<string, float> announce_message;
    private bool queue_spawn_adds=false;
    public override void Start()
    {
        base.Start();
        defeated_event += allydie;
        queue_spawn_adds=false;
        
        deck = closed_deck_deck;
        open_mode=false;
        anim.SetBool("open", open_mode);
        spawn_additional_enemy?.Invoke(additional_enemy_prefab,-1f);
        spawn_additional_enemy?.Invoke(additional_enemy_prefab,1f);
    }
    public override void init()
    {
        deck = closed_deck_deck;
        open_mode=false;
        anim.SetBool("open", open_mode);
        announce_message?.Invoke("Started your internship!",3f);
        announce_message?.Invoke("defeat the minions in 4 turns to avoid getting fired!",3f);
        spawn_additional_enemy?.Invoke(additional_enemy_prefab,-1.1f);
        spawn_additional_enemy?.Invoke(additional_enemy_prefab,1.1f);
        
        base.init();        
    }
    public override void queue_next_atk()
    {
        if(deck.Count==0)
        {
            Debug.LogError("Enemy has no attacks in deck to queue");
            return;
        }
        if (pattern)
        {
            queued_hat = deck[pattern_index].copy();
            pattern_index++;
            if(pattern_index>=deck.Count)
            {
                pattern_index = 0;
            }
        }
        else//queue random atk
        {
            UnityEngine.Random.InitState((int)DateTime.Now.Millisecond+System.Environment.TickCount+System.Diagnostics.Process.GetCurrentProcess().Id);
            queued_hat = deck[UnityEngine.Random.Range(0, deck.Count)].copy(); 
        }

        if(burnout_Bar is enemy_ui)
        {
            ui.setup_disc(queued_hat.get_icon_description());
     
        }
        else
        {
            Debug.LogError("Burnout bar is not of type enemy_ui, cannot set forcast text");
            return; 
        }
        
    }
    public override void die()
    {
        Boss_defeated_event?.Invoke();  
    }
    public override void change_hp(int amount)
    {
        if (amount > 0)
        {
            //play heal effect
        }
        else
        {
            //play damage effect
            int damage_to_shield = Mathf.Min(-amount, shield);//need to allow for debuffs that increase damage taken
            shield -= damage_to_shield;
            amount += damage_to_shield;
            current_hp += amount;
            current_phase_damage_progress += -amount;
            Debug.Log("Enemy took damage");
            Debug.Log("Activating use effects for Communication hat");
            
            if(current_phase_damage_progress>=phase_damage_shift_amount)
            {
                prepare_mode_change();
                    queue_spawn_adds=true;
            }
            else
            {
                if(open_mode)
                {
                    anim.Play("takedmg_open");
                }
                else
                {
                    anim.Play("takedmg_closed");
                }
            }
  
            
            dmg_num_disp.init(-amount, false, damage_to_shield > 0);
            ui.set_hp(current_hp, max_hp, shield);
        }
        if (current_hp > max_hp)
        {
            current_hp = max_hp;
        }
        if (current_hp <= 0)
        {
            current_hp = 0;
            anim.Play("death");
            return;
        }
    }
    public void allydie(enemy dead_ally)
    {
        current_phase_ally_units_progress++;
        if(!open_mode && current_phase_ally_units_progress>=2)
        {
            
            this.shield += 10;
            burnout_Bar.set_hp(current_hp, max_hp, shield);
            prepare_mode_change();
        }
    }
    public void prepare_mode_change()
    {
        open_mode =!open_mode;
        current_phase_damage_progress = 0;
        current_phase_ally_units_progress = 0;
        pattern_index=0;
        anim.SetBool("open", open_mode);
        queued_hat = null;
        //ui.forcast_text.text = "";
        if (open_mode)
        {
            
        announce_message?.Invoke("Deal 20 dmg to the boss in 4 turns to avoid getting fired!",3f);
         deck = open_deck_deck;
        }
        else
        {
        deck = closed_deck_deck;    
        }
        
    }
    public override IEnumerator enemy_turn()
    {
        Debug.Log("Enemy turn started");
        activate_use_effects();
        yield return new WaitForSeconds(1f);
        while (!anim.GetCurrentAnimatorStateInfo(0).IsTag("idle"))
        {
            yield return null;
        }
        Debug.Log("Enemy turn ended");
        queued_hat = null;
        if(queue_spawn_adds)
            {
                
            announce_message?.Invoke("defeat the minions in 4 turns to avoid getting fired!",3f);
        spawn_additional_enemy?.Invoke(additional_enemy_prefab,-1.1f);
        spawn_additional_enemy?.Invoke(additional_enemy_prefab,1.1f);
            queue_spawn_adds=false;
        }
    }
    public override void activate_use_effects()
    {
        
            Debug.Log("start"+ gameObject.name);
        if (queued_hat == null|| current_target==null)
        {
            Debug.LogWarning("No hat equipped to activate use effects on");
            return;
        }
        if(queued_hat.name=="")
        {
            Debug.LogWarning("Queued hat has no name, cannot activate use effects");
            return;
        }
        if(queued_hat.type==hat_type.Transferable)
        {
            Debug.Log("Activating use effects for Technical hat");
            float y_rot = math.atan2(transform.position.x-current_target.transform.position.x, current_target.transform.position.z - transform.position.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, -y_rot+90, 0);
            if(open_mode)
            {
                anim.Play("attack_open");
            }
            else
            {
                 anim.Play("attack_closed");
            }
        }
        else if(queued_hat.type==hat_type.Social)
        {
            Debug.Log("Activating use effects for Communication hat");
            if(open_mode)
            {
                anim.Play("shield_open");
            }
            else
            {
                 anim.Play("shield_closed");
            }
        }
        else if(queued_hat.type==hat_type.Critical)
        {
            Debug.Log("Activating use effects for Creativity hat");
            anim.Play("buff");//might change this later
        }
        
            Debug.Log("done");
    }
    public void OnDestroy()
    {
        defeated_event -= allydie;
    }
}
