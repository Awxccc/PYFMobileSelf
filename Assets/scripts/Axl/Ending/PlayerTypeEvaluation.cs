using TMPro;
using System.Linq;
using UnityEngine;
/** 
@brief Calculates and evaluates the player type (After further playtesting from next batch, maybe need buff or debuff some values)
*/
public class PlayerTypeEvaluation : MonoBehaviour
{
    private Player_data player;
    
    [Header("References")]
    // Needed for stats and eval
    [SerializeField] private TextMeshProUGUI playerType;
    [SerializeField] private TextMeshProUGUI playerDesc;
    [SerializeField] private TextMeshProUGUI hatText;
    [SerializeField] private TextMeshProUGUI eventText;
    [SerializeField] private TextMeshProUGUI encounterText;
    [SerializeField] private TextMeshProUGUI peacefulText;
    [SerializeField] private TextMeshProUGUI energyUsedText;
    [SerializeField] private TextMeshProUGUI turnsText;

    // Needed for player type only
    private int eventCount;
    private float eventRate;
    private int encounterCount;
    private float encounterRate;

    private int criticalHatCount;
    private int socialHatCount;
    private int transferHatCount;
    private float criticalHatRate;
    private float socialHatRate;
    private float transferHatRate;

    private int restOptionCount;
    private int encounterOptionCount;
    private int skillOptionCount;
    private float restOptionRate;
    private float encounterOptionRate;
    private float skillOptionRate;

    private float peacefulRate;

    private void Start()
    {
        player = Player_data.instance;

        // Calculate peaceful to hostile encounter ratio
        int totalEncounters = player.turnsBeforeCounselling + player.hostile_encounters;

        if (totalEncounters > 0)
        {
            peacefulRate = (float)player.turnsBeforeCounselling / totalEncounters * 100f;
        }
        else
        {
            peacefulRate = 0f;
        }

        // For stats
        hatText.text = player.deck.Count.ToString();
        eventText.text = player.events_chosen.ToString();
        encounterText.text = player.encounters_chosen.ToString();
        peacefulText.text = peacefulRate.ToString("F1") + "%";
        energyUsedText.text = player.energy_spent.ToString();
        turnsText.text = player.combat_turns_taken.ToString();

        // For eval
        eventCount = player.events_chosen;
        encounterCount = player.encounters_chosen;
        eventRate = (float)eventCount / (eventCount + encounterCount) * 100f;
        encounterRate = (float)encounterCount / (eventCount + encounterCount) * 100f;

        criticalHatCount = player.deck.FindAll(x => x.type == hat_type.Critical).Count;
        socialHatCount = player.deck.FindAll(x => x.type == hat_type.Social).Count;
        transferHatCount = player.deck.FindAll(x => x.type == hat_type.Transferable).Count;
        criticalHatRate = (float)criticalHatCount / player.deck.Count * 100f;
        socialHatRate = (float)socialHatCount / player.deck.Count * 100f;
        transferHatRate = (float)transferHatCount / player.deck.Count * 100f;

        restOptionCount = player.event_options_rest;
        encounterOptionCount = player.event_options_encounter;
        skillOptionCount = player.event_options_skill;
        restOptionRate = (float)restOptionCount / (restOptionCount + encounterOptionCount + skillOptionCount) * 100f;
        encounterOptionRate = (float)encounterOptionCount / (restOptionCount + encounterOptionCount + skillOptionCount) * 100;
        skillOptionRate = (float)skillOptionCount / (restOptionCount + encounterOptionCount + skillOptionCount) * 100f;

        // Run weighted player type evaluation algorithm
        WeightedEvalAlgo();
    }

    private void WeightedEvalAlgo()
    {
        int counter = 0;

        float[] Rates =
        {
            eventRate * 0.5f,
            encounterRate,
            criticalHatRate,
            socialHatRate * 0.75f,
            transferHatRate,
            peacefulRate,
            restOptionRate,
            encounterOptionRate,
            skillOptionRate
        };

        //Determine if All Rounder player type
        for (int i = 0; i < Rates.Length; i++)
        {
            if (Rates[i] < 30)
            {
                counter++;
            }
        }

        if (counter == Rates.Length)
        {
            playerType.text = "The All Rounder";
            playerDesc.text = "You are balanced in all aspects of your hike!";
        }
        else
        {
            // Find highest rate and determine player type
            float highestRate = Rates.Max();

            int maxIndex = System.Array.IndexOf(Rates, highestRate);

            switch (maxIndex)
            {
                case 0:
                    playerType.text = "The Extrovert";
                    playerDesc.text = "You took part in a lot of events!";
                    break;
                case 1:
                    playerType.text = "The Adventurer";
                    playerDesc.text = "You entered a lot of encounters!";
                    break;
                case 2:
                    playerType.text = "The Technician";
                    playerDesc.text = "You obtained a lot of Technical hats!";
                    break;
                case 3:
                    playerType.text = "The Social Butterfly";
                    playerDesc.text = "You obtained a lot of Social hats!";
                    break;
                case 4:
                    playerType.text = "The Leader";
                    playerDesc.text = "You obtained a lot of transferable hats!";
                    break;
                case 5:
                    playerType.text = "The Pacifist";
                    playerDesc.text = "You solved a lot of encounters peacefully!";
                    break;
                case 6:
                    playerType.text = "The Balancer";
                    playerDesc.text = "You chose a lot of options that allowed you to rest!";
                    break;
                case 7:
                    playerType.text = "The Ambitious One";
                    playerDesc.text = "You chose a lot of options that led to encounters!";
                    break;
                case 8:
                    playerType.text = "The High Roller";
                    playerDesc.text = "You chose a lot of options that led to skill checks!";
                    break;
            }
        }
    }
}
