using System;
using System.Collections.Generic;
using UnityEngine;
using static Event;
[Serializable]
public class event_option 
{
    public enum event_outcome
    {
        end_scene,
        trigger_combat,
        hat_or_combat
    }

    public enum event_type
    {
        Rest,
        Encounter,
        Skill
    }

    public string option_title;
    public string option_description;

    public int baseSkillCheckTarget;
    [HideInInspector] public int finalSkillCheckTarget;

    [Header("Event Type")]
    public event_type eventType;

    [Header("Outcome")]
    public event_outcome successOutcome;
    public event_outcome failureOutcome;

    [Header("Skill Check")]
    public bool hasSkillCheck;
    public SkillCheckOutcome skillCheckOutcome;
    public SkillCheckType skillCheckType;

    [HideInInspector] public bool skillCheckAttempted;
    [HideInInspector] public bool skillCheckSuccess;

    [Header("Rewards")]
    public List<event_reward> rewards = new List<event_reward>();

    public bool giveHat;
    public hat[] hatToGive;
    public event_outcome outcome;
    public event_condition condition;//if condition fails , use other option
    [HideInInspector] public bool skillCheck;
    [HideInInspector] public bool skillCheckEncounter;
    public bool affectEnergy;
    public int changeEnergyAmount;
    public bool affectMaxEnergy;
    public int changeMaxEnergyAmount;
    public bool upgradeHatsRandomly;
    public int amountOfHatsToUpgrade;
    public bool upgradeBaseHats;
    public bool affectHandSize;
    public int changeHandSizeAmount;
    [HideInInspector] public bool skillCheckFailed;
    [HideInInspector] [System.NonSerialized] public int currentHatIndex = 0;
}
