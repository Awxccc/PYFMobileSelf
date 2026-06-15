using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
/** 
@addtogroup entities
enemies are the base class for all enemy combatants in the game. they also handle ai and hand stuff
*/
public class enemy : entity
{
    /** 
    @brief The hat currently equipped by the enemy.
    */
    public static Action<enemy> defeated_event;
    public bool isDying = false;
    private bool hasDied = false;


    public bool pattern = false;
    public int pattern_index = 0;
    [HideInInspector] public enemy_ui ui;
    public static Action<GameObject, float> spawn_additional_enemy;
    public virtual void init()
    {


    }
    public override void Start()
    {
        base.Start();

        ui = burnout_Bar as enemy_ui;
        burnout_Bar.set_hp(current_hp, max_hp, shield);
        queue_next_atk();
    }

    public virtual IEnumerator enemy_turn()
    {
        Debug.Log("Enemy turn started");
        activate_use_effects();
        yield return new WaitForSeconds(1f);
        while (!anim.GetCurrentAnimatorStateInfo(0).IsTag("idle"))
        {

            Debug.Log("Waiting for enemy animation to finish : " + gameObject.name + " current anim state " + anim.GetCurrentAnimatorStateInfo(0).tagHash);
            if (!anim.GetCurrentAnimatorStateInfo(0).IsTag("idle"))
            {
                Debug.Log("Current anim state is not idle: " + anim.GetCurrentAnimatorStateInfo(0).tagHash);
            }
            yield return null;
        }
        Debug.Log("Enemy turn ended");
        queued_hat = null;

    }
    public virtual void queue_next_atk()
    {
        if (deck.Count == 0)
        {
            Debug.LogError("Enemy has no attacks in deck to queue");
            return;
        }
        if (pattern)
        {
            queued_hat = deck[pattern_index].copy();
            pattern_index++;
            if (pattern_index >= deck.Count)
            {
                pattern_index = 0;
            }
        }
        else//queue random atk
        {
            UnityEngine.Random.InitState((int)DateTime.Now.Millisecond + System.Environment.TickCount + System.Diagnostics.Process.GetCurrentProcess().Id);
            queued_hat = deck[UnityEngine.Random.Range(0, deck.Count)].copy();
        }
        if (queued_hat.type == hat_type.Transferable)
        {
            queued_hat.pow_mod = atk_mod;
        }
        else if (queued_hat.type == hat_type.Social)
        {
            queued_hat.pow_mod = shield_mod;
        }
        if (burnout_Bar is enemy_ui)
        {
            Debug.Log("Setting up forcast for enemy " + gameObject.name + " with hat " + queued_hat.name);
            ui.setup_disc(queued_hat.get_icon_description());
        }
        else
        {
            Debug.LogError("Burnout bar is not of type enemy_ui, cannot set forcast text");
            return;
        }

    }
    public override void activate_use_effects()
    {

        Debug.Log("start" + gameObject.name);
        if (queued_hat == null || current_target == null)
        {
            Debug.LogWarning("No hat equipped to activate use effects on");
            return;
        }
        if (queued_hat.name == "")
        {
            Debug.LogWarning("Queued hat has no name, cannot activate use effects");
            return;
        }
        if (queued_hat.type == hat_type.Transferable)
        {
            Debug.Log("Activating use effects for Technical hat");
            float y_rot = math.atan2(transform.position.x - current_target.transform.position.x, current_target.transform.position.z - transform.position.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, -y_rot + 90, 0);
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

        Debug.Log("done");
    }
    public override void change_atk_mod(int amount)
    {
        atk_mod += amount;
        if (queued_hat != null)
        {
            if (queued_hat.type == hat_type.Transferable)
            {
                Debug.Log("Updating queued hat power mod by " + amount);
                queued_hat.pow_mod = atk_mod;
                ui.setup_disc(queued_hat.get_icon_description());
            }
        }
    }
    public override void change_shield_mod(int amount)
    {
        shield_mod += amount;
        if (queued_hat != null)
        {
            if (queued_hat.type == hat_type.Social)
            {
                Debug.Log("Updating queued hat power mod by " + amount);
                queued_hat.pow_mod = shield_mod;
                ui.setup_disc(queued_hat.get_icon_description());
            }
        }
    }
    public override void activete_hat_effects()
    {
        if (queued_hat == null || current_target == null)
        {
            Debug.LogError("No hat equipped/target to activate effects on");
            return;
        }
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
        Debug.Log("Activated hat effects for " + queued_hat.name);
        //add on get attacked effects later for dmg taken up effects,for dmg increase effects need to figure out how to allpy with the desc
        if (queued_hat.type == hat_type.Transferable)
        {
            //set up for dmg
            current_target.change_hp(-queued_hat.get_pow());
        }
        else if (queued_hat.type == hat_type.Social)
        {
            //set up for block
            Debug.Log("Activating block for Social hat");
            this.shield += queued_hat.get_pow();
            burnout_Bar.set_hp(current_hp, max_hp, shield);
        }
        else if (queued_hat.type == hat_type.Critical)
        {
            //set up for support
        }
    }
    public override void change_hp(int amount)
    {
        if (isDying || hasDied) return;

        base.change_hp(amount);

        if (current_hp <= 0)
        {
            isDying = true;

            Debug.Log("Enemy entered dying state: " + gameObject.name);

            anim.Play("death", 0, 0f);

            // Backup in case Animation Event fails.
            StartCoroutine(DeathBackupTimer());
        }
    }

    private IEnumerator DeathBackupTimer()
    {
        yield return new WaitForSeconds(2f);

        if (!hasDied)
        {
            Debug.LogWarning("Death Animation Event did not fire. Backup die() called for: " + gameObject.name);
            die();
        }
    }

    public virtual void die()
    {
        if (hasDied) return;

        hasDied = true;

        Debug.Log("enemy.die() was called on: " + gameObject.name);

        defeated_event?.Invoke(this);
    }
}
