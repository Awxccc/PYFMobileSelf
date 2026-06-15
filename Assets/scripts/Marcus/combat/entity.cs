using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
/** 
@brief Base class for all entities in combat, including players and enemies.
This class manages health, shields, buffs, and hat effects for combat entities.
 */
 
 /** 
@defgroup entities Entities
entities are the base class for all combatants in the game, including players and enemies.
 */

public abstract class entity : MonoBehaviour
{
    /** 
    @brief current health points of the entity,
    */
    public int current_hp;// use this for burnout
    public int max_hp;
    public int shield;
    
    public List<hat.hat_data> deck;
    /** 
    @brief currently active buffs on the entity
    */
    public List<buff> active_buffs;
    /** 
    @brief this is used for enemies quing attacks and players selecting hats
    */
     public hat.hat_data queued_hat;//currently equipped hat
    [HideInInspector] public entity current_target;//current target of player actions

    public int atk_mod;//modifies atk done by this entity
    public int shield_mod;//modifies shield given by this entity
    public int defense_mod;//modifies dmg taken by this entity
    public Animator anim;
    /** 
    @brief universal combat ui for the entity(sorry foir the weird name lol)
    */
    public BurnoutBar burnout_Bar;  
    /** 
    @brief displayes damage numbers when taking dmg or healing
    */
    public dmg_num dmg_num_disp;
    [Header("vfx transforms")]
    public Transform hit_vfx_transform;
    public Transform buffvfx_transform;
    public Transform debuffvfx_transform;
    
    public GameObject buff_fx_prefab;
    [HideInInspector]public ParticleSystem buff_fx_instance;
    public GameObject debuff_fx_prefab;
      [HideInInspector]public ParticleSystem debuff_fx_instance;
    
    public GameObject hit_fx_prefab;
    [HideInInspector]public ParticleSystem hit_fx_instance;
    [Header("optional vfx ")]
    public Transform projectilevfx_transform;// this is optional, only used for ranged attacks
    public GameObject projectilevfx_prefab;//optional vfx for vommit attack
     [HideInInspector] public ParticleSystem vommitvfx_instance;
    /** 
    @brief cfor activating screen shake on entity actions
    */
    public static Action<float> play_impulse_event;
    private bool loaded=false;
    /** 
    @brief this is to run initialization code for the entity
    */
    public virtual void Start()
    {
        if (!loaded)
        {
            hit_fx_instance = Instantiate(hit_fx_prefab, hit_vfx_transform).GetComponent<ParticleSystem>();
            hit_fx_instance.transform.localPosition= Vector3.zero;
            hit_fx_instance.Clear();
            hit_fx_instance.Pause();
            buff_fx_instance = Instantiate(buff_fx_prefab, buffvfx_transform).GetComponent<ParticleSystem>();
            
            buff_fx_instance.transform.localPosition= Vector3.zero;
            buff_fx_instance.Clear();
            buff_fx_instance.Pause();
            debuff_fx_instance = Instantiate(debuff_fx_prefab, debuffvfx_transform).GetComponent<ParticleSystem>();
            debuff_fx_instance.transform.localPosition= Vector3.zero;
            debuff_fx_instance.Clear();
            debuff_fx_instance.Pause();
            if(projectilevfx_prefab!=null)
            {
                vommitvfx_instance = Instantiate(projectilevfx_prefab, projectilevfx_transform).GetComponent<ParticleSystem>();
                vommitvfx_instance.gameObject.SetActive(false);
            }
            current_hp = max_hp;
            loaded=true;
        }

    }    
    /** 
    @brief for changing entities hp, ex dmg and healing
    @param amount the amount to change hp by(negative for dmg, positive for healing)
    */
    public virtual void change_hp(int amount)
    {
        if (amount < 0)
        {
            //not a heal
            amount+=defense_mod;
            int damage_to_shield = Mathf.Min(-amount, shield);
            shield -= damage_to_shield;
            amount += damage_to_shield;
            Debug.Log("Enemy took damage");
            dmg_num_disp.init(-amount, false, damage_to_shield > 0);
            hit_fx_instance.Clear();
            hit_fx_instance.Play();
            if(anim.GetCurrentAnimatorStateInfo(0).IsTag("idle"))
            {
            anim.Play("hurt");
            }
        }
        else
        {
            //play heal effect
            Debug.Log("healing player for " + amount);
            dmg_num_disp.init(amount, true, false);
        }

        current_hp += amount;
        if (current_hp > max_hp)
        {
            current_hp = max_hp;
        }
        if (current_hp <= 0)
        {
            current_hp = 0;
            anim.Play("death");
        }
        burnout_Bar.set_hp(current_hp, max_hp, shield);
    }
    
    /** 
    @brief used for loading animations and facing the correct direction when using hats
    */
    public virtual void activate_use_effects()
    {
        if (queued_hat == null || current_target == null)
        {
            Debug.LogError("No hat equipped to activate use effects on");
            return;
        }
        if (queued_hat.type == hat_type.Transferable)
        {
            Debug.Log("Activating use effects for Technical hat");
            float y_rot = math.atan2(transform.position.x - current_target.transform.position.x, current_target.transform.position.z - transform.position.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, -y_rot - 90, 0);
            anim.Play("attack");
        }
        else if (queued_hat.type == hat_type.Social)
        {
            Debug.Log("Activating use effects for Communication hat");
            anim.Play("defend");
        }
        else if (queued_hat.type == hat_type.Critical)
        {
            Debug.Log("Activating use effects for Creativity hat");
            anim.Play("buff");//might change this later
        }
    }

    /** 
    @brief used activating the effects of associated with the currently equipped hat
    */
    public virtual void activete_hat_effects()
    {
        if (queued_hat == null || current_target == null)
        {
            Debug.LogError("No hat equipped/target to activate effects on");
            return;
        }
        if(queued_hat.level == 0)
        {
            foreach (var effect in queued_hat.use_effects)
            {
                if (effect.selftarget)
                {
                    effect.activate_effect(this);
                }
                else
                {
                    effect.activate_effect(current_target);
                }
            }
        }
        else
        {
            foreach (var effect in queued_hat.upgraded_use_effects)
            {
                if (effect.selftarget)
                {
                    effect.activate_effect(this);
                }
                else
                {
                    effect.activate_effect(current_target);
                }
            }
        }

        //add on get attacked effects later for dmg taken up effects,for dmg increase effects need to figure out how to allpy with the desc
        if (queued_hat.type == hat_type.Transferable)
        {
            //set up for dmg
            current_target.change_hp(-queued_hat.get_pow());
        }
        else if (queued_hat.type == hat_type.Social)
        {
            //set up for block
            Debug.Log("Shield: " + queued_hat.get_pow());
            this.shield += queued_hat.get_pow();
            burnout_Bar.set_hp(current_hp, max_hp, shield);
        }
        else if (queued_hat.type == hat_type.Critical)
        {
            //set up for support
        }
    }
    
    
    /** 
    @brief changes the atk modifier and updates their hats
    */
    public abstract void change_atk_mod(int amount);
     /** 
    @brief changes the shield modifier and updates their hats
    */
    public abstract void change_shield_mod(int amount);
    public void reset_rot()
    {
        transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    public void TriggerScreenShake(float intensity)
    {
        play_impulse_event?.Invoke(intensity);
    }
   /** 
    @brief for adding and updaing buffs
    
    @details also checks if buffs are duplicate and stacks them if so. for buffs that are remove on expire it also applies the effect on add
    @param new_buff the buff to add( should be a copy.) checks based on name
    */
    public virtual void add_buff(buff new_buff)
    {
        buff duplicate_buff = active_buffs.Find(x => x.buff_name == new_buff.buff_name);
        if (new_buff.type == buff.buff_type.remove_on_expire)
        {
            new_buff.activate_effect(this);//apply on add effects(atkup, deg up etc)
        }
        if (duplicate_buff != null)//active effect first then add to existing.
        {
            Debug.Log("Found duplicate buff " + new_buff.buff_name + ", stacking effects");
            int index = active_buffs.IndexOf(duplicate_buff);
            if (active_buffs[index].pow + new_buff.pow == 0)
            {
                if (active_buffs[index].type == buff.buff_type.remove_on_expire)
                {
                    active_buffs[index].on_expire(this);//remove effects if expired
                }
                active_buffs.RemoveAt(index);
                burnout_Bar.removebuff(duplicate_buff.buff_name);
                return;
            }
            active_buffs[index].pow += new_buff.pow;//increment potency
            active_buffs[index].duration = Math.Max(active_buffs[index].duration, new_buff.duration);//refresh duration if new buff is longer
            burnout_Bar.update_buff(active_buffs[index]);//visually updates
        }
        else
        {
            if (new_buff.pow > 0)
            {
                buff_fx_instance.Clear();
                buff_fx_instance.Play();
            }
            else if (new_buff.pow < 0)
            {
                
                debuff_fx_instance.Clear();
                debuff_fx_instance.Play();
            }
            active_buffs.Add(new_buff);
            burnout_Bar.addbuff(new_buff);
            Debug.Log("Added new buff " + new_buff.buff_name);
        }
    }
    /** 
    @brief ticks down the buffs durations and removes expired buffs
    @details this is called at the end of each turn and also updates the burnoutbar.
    when removing a buff it also calls the on_expire effect if applicable
    */
    public virtual void process_buffs()
    {
        List<buff> buffs_to_remove = new List<buff>();
        for(int i = active_buffs.Count - 1; i >= 0; i--)
        {
            buff b = active_buffs[i];
            //may add on turn start effects later
            b.duration--;
            if (b.duration <= 0)
            {

                if (b.type == buff.buff_type.remove_on_expire)
                {
                    b.on_expire(this);
                    removebuff_vfx(i);
                }
                active_buffs.RemoveAt(i);
                
                burnout_Bar.removebuff(b.buff_name);
            }
        }
    }
    void OnDestroy()
    {
        play_impulse_event-= TriggerScreenShake;
    }
    public void removebuff_vfx(int b)
    {
       buff temp = active_buffs[b];
       if(temp.pow>0)
       {//check if other positive buff of same name exists
           for(int i=0;i<active_buffs.Count;i++)
           {
               if(i!=b&&active_buffs[i].buff_name==temp.buff_name&&active_buffs[i].pow>0)
               {
                    buff_fx_instance.Pause();
                   return;
               }
           }
       }
       else if(temp.pow<0)
        {
            for(int i=0;i<active_buffs.Count;i++)
            {
                if(i!=b&&active_buffs[i].buff_name==temp.buff_name&&active_buffs[i].pow<0)
                {
                        debuff_fx_instance.Pause();
                    return;
                }
            }
        }
    }
    public void spawn_projectile_vfx()
    {
        if(vommitvfx_instance!=null)
        {
            vommitvfx_instance.gameObject.SetActive(true);
            vommitvfx_instance.Play();
        }
    }
    public void active_hitstop(float duration)
    {
        StartCoroutine(hitstop( duration));
    }
    public IEnumerator hitstop(float duration)
    {
        float val = duration;
        anim.speed = 0;
        yield return new WaitForSeconds(val);
        anim.speed = 1;
    }
}
