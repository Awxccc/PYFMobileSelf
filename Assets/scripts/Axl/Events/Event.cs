using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
/** 
@brief Main script that handles events and their options
*/
public class Event : MonoBehaviour
{
    public Transform btn_parent;

    [SerializeField] private string title;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private string description;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private UnityEngine.UI.Button confirmButton;
    [SerializeField] private GameObject skillCheckPanel;
    [SerializeField] private SkillCheck skillCheck;
    private HatPlacement hatPlacementPlayer;
    private event_option currentSkillCheckOption;
    private event_btn currentSkillCheckButton;

    [Header("event data")]
    [SerializeField] List<event_option> options = new List<event_option>();

    private Player_data player;

    public static Action enable_combat;
    public static Action end_scene;

    private event_btn pendingButton;
    /** 
    @brief Possible outcomes of skill checks
    */
    public enum SkillCheckOutcome
    {
        None,
        HatReward, //If skill check win get hat, else nothing happens
        Outcome, // If skill check win nothing happens, else go into combat
        HatRewardOrCombat // If skill check wins get hat, else go into combat
    }
    /** 
    @brief Skill check type that determines how much discount on the target number you can get based on the hats you have.
    */
    public enum SkillCheckType
    {
        Social,
        Transferable,
        Critical
    }

    private void Awake()
    {
        GameObject playerObject = GameObject.FindWithTag("Player");

        if (playerObject != null)
        {
            hatPlacementPlayer = playerObject.GetComponent<HatPlacement>();
        }
    }

    private void Start()
    {
        confirmButton.gameObject.SetActive(false);

        player = Player_data.instance;
        //player.current_hp = player.max_hp;

        foreach (event_option option in options)
        {
            GameObject btn_instance = Instantiate(Resources.Load<GameObject>("btn"), btn_parent);

            event_btn btn_script = btn_instance.GetComponent<event_btn>();

            // Calculate skill check target BEFORE setting up the button
            if (option.hasSkillCheck)
            {
                option.finalSkillCheckTarget = CalculateFinalSkillCheckTarget(option);
            }

            btn_script.setup_button(option, this);
        }

        titleText.text = title;
        descriptionText.text = description;
    }

    private void Update()
    {
        // Testing
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            player.change_hp(-2);
        }
    }
    /** 
    @brief Handles options lighting up when you click on them
    */
    public void select_option(event_btn btn)
    {
        if (pendingButton == btn) return;

        if (pendingButton != null)
        {
            pendingButton.anim.Play("ButtonDown");
            pendingButton.SetSelected(false);
        }

        pendingButton = btn;

        btn.anim.Play("ButtonUp");
        btn.SetSelected(true);
        confirmButton.gameObject.SetActive(true);

        //if (btn.ref_option.hasSkillCheck)
        //{
        //    btn.ref_option.finalSkillCheckTarget = CalculateFinalSkillCheckTarget(btn.ref_option);

        //    // Update the button text with the new target
        //    btn.btn_descText.text = $"{btn.ref_option.option_description}\nTarget: {btn.ref_option.finalSkillCheckTarget}";
        //}
    }

    public void OnDestroy()
    {
        Debug.Log("Event destroyed, cleaning up skill check panel, name"+ title);
    }

    public void confirm_selected_option()
    {
        if (pendingButton == null)
        {
            Debug.Log("No option selected!");
            return;
        }

        pendingButton.SetSelected(false);

        execute_option(pendingButton);
        pendingButton = null;
    }

    public void OnConfirmButtonPressed()
    {
        confirm_selected_option();
    }

    private void OnSkillCheckResult(bool success)
    {
        currentSkillCheckOption.skillCheckAttempted = true;
        currentSkillCheckOption.skillCheckSuccess = success;

        execute_option(currentSkillCheckButton);

        currentSkillCheckOption = null;
        currentSkillCheckButton = null;
    }
    /** 
    @brief Handles what happens when you confirm your selection. Handles if skill check, what reward given, etc.
    */
    private void execute_option(event_btn btn)
    {
        Debug.Log("executed times");
        event_option selected_option = btn.ref_option;

        // Check if player already failed this skill check
        //if (selected_option.hasSkillCheck && selected_option.skillCheckAttempted && !selected_option.skillCheckSuccess)
        //{
        //    StartCoroutine(ButtonShakeAndDeselect(btn));
        //    return;
        //}

        if (selected_option.hasSkillCheck && !selected_option.skillCheckAttempted) //First iteration/called
        {
            skillCheck.target = selected_option.finalSkillCheckTarget;
            /** 
            @brief Does not allow you to choose skill checks that could potentially kill you
            */
            if (WouldSkillCheckCauseGameOver(selected_option))
            {
                StartCoroutine(ButtonShakeAndDeselect(btn));
                return;
            }

            currentSkillCheckOption = selected_option;
            currentSkillCheckButton = btn;

            skillCheck.OnSkillCheckFinished = OnSkillCheckResult;
            skillCheckPanel.SetActive(true);
            return;
        }
        bool isSuccess = true;
        /** 
        @brief Handles skill check outcome
        */
        if (selected_option.hasSkillCheck)//Called again after done spinning dice
        {
            Debug.Log("Option has skill check");
            isSuccess = selected_option.skillCheckSuccess;

            if (!isSuccess)
            {
                btn.anim.Play();
                selected_option.giveHat = false;
                if (selected_option.skillCheckOutcome == SkillCheckOutcome.Outcome || selected_option.skillCheckOutcome == SkillCheckOutcome.HatRewardOrCombat)
                {
                    // If player LOST SKILL CHECK and consequence is to go into combat
                    // Go into combat
                    ResolveOutcome(selected_option.failureOutcome);// Failure outcome will always be trigger combat because of SkillCheckOutcome
                    gameObject.SetActive(false);
                    return;
                }
            }
        }

        // Give hat reward
        if (selected_option.giveHat && selected_option.hatToGive.Length > 0)
        {
            foreach (var hat in selected_option.hatToGive)
            {
                Debug.Log("hat given");
                Player_data.instance.addHatCoroutine = StartCoroutine(Player_data.instance.add_hat(hat.hat_info));
                //hatPlacementPlayer.PlaceHat(hat.hat_info);
                player.add_hat(hat.hat_info);
            }
        }

        // Handle event outcome
        if (selected_option.hasSkillCheck && isSuccess) //If skill checked and passed
        {
            ResolveOutcome(selected_option.successOutcome);
            gameObject.SetActive(false);
            return;
        }

        foreach (event_reward reward in selected_option.rewards)
        {
            reward.grant_reward(player);
        }

        if (selected_option.condition != null && !selected_option.condition.check_condition())
        {
            btn.anim.Play();
            foreach (event_reward reward in selected_option.condition.fail_reward)
            {
                reward.grant_reward(player);
            }
            return;
        }

        if (selected_option.affectHandSize)
            player.change_handsize(selected_option.changeHandSizeAmount);

        if (selected_option.affectEnergy)
        {
            if (player.current_hp + selected_option.changeEnergyAmount <= 0)
            {
                StartCoroutine(ButtonShakeAndDeselect(btn));
                return;
            }
            else
            {
                player.change_hp(selected_option.changeEnergyAmount);
            }
        }

        if (selected_option.affectMaxEnergy)
            player.change_maxhp(selected_option.changeMaxEnergyAmount);

        if (selected_option.upgradeHatsRandomly)
            player.upgrade_hats_random(selected_option.amountOfHatsToUpgrade);

        if (selected_option.upgradeBaseHats)
            player.upgrade_base_hats();

        switch (selected_option.outcome)
        {
            case event_option.event_outcome.end_scene:
                end_scene?.Invoke();
                break;

            case event_option.event_outcome.trigger_combat:
                enable_combat?.Invoke();
                break;
        }
        gameObject.SetActive(false);
    }

    private void ResolveOutcome(event_option.event_outcome outcome)
    {
        switch (outcome)
        {
            case event_option.event_outcome.end_scene:
                end_scene?.Invoke();
                break;

            case event_option.event_outcome.trigger_combat:
                enable_combat?.Invoke();
                break;
        }
    }

    private bool WouldSkillCheckCauseGameOver(event_option option)
    {
        if (!option.affectEnergy)
            return false;

        int projectedHp = player.current_hp + option.changeEnergyAmount;
        return projectedHp <= 0;
    }
    /** 
    @brief Animation for choosing an option you are not allowed to choose
    */
    private IEnumerator ButtonShakeAndDeselect(event_btn btn)
    {
        btn.anim.Play("ButtonShake");
        yield return new WaitForSeconds(0.5f);
        btn.anim.Play("ButtonDown");
    }
    /** 
    @brief Calculate final discount given to skill check target number
    */
    private int CalculateFinalSkillCheckTarget(event_option option)
    {
        int finalTarget = skillCheck.target;

        switch (option.skillCheckType)
        {
            case SkillCheckType.Transferable:
                finalTarget -= Player_data.instance.technical_level;
                break;

            case SkillCheckType.Social:
                finalTarget -= Player_data.instance.communication_level;
                break;

            case SkillCheckType.Critical:
                finalTarget -= Player_data.instance.creativity_level;
                break;
        }

        return Mathf.Max(1, finalTarget);
    }
}
