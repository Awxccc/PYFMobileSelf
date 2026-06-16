using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Player_data : entity
{
    [Header("Player Stats")]
    private character_class_data class_data;//this is for containing the infop on which school they are from(colour name starting deck etc)
    private globaldataholder globalData;//grabs global data holder for accessing all hats and scenes

    public int handsize = 5;
    [HideInInspector] public handui hand_ui;
    [HideInInspector] public static Player_data instance;//should only have one player data object
    [Header("mats")]
    public Material[] player_mats = new Material[3];

    [Header("Scene List")]
    public event_data current_scene;//will make hidden later
    public List<event_data> easy_encounter_list = new List<event_data>();//list of all scenes in game
    public List<event_data> medium_encounter_list = new List<event_data>();
    public List<event_data> hard_encounter_list = new List<event_data>();
    public List<event_data> easy_event_list = new List<event_data>();//list of currently available scenes //maybe will add difficulty scaling
    public List<event_data> medium_event_list = new List<event_data>();
    public List<event_data> hard_event_list = new List<event_data>();
    public List<event_data> treasure_list = new List<event_data>();
    public List<event_data> boss_list = new List<event_data>();

    public List<progress_data> progress_log = new List<progress_data>();
    public int remaining_retries = 3;
    public int current_boss_progress = 0;
    public int boss_min_progress = 5;//for every scene above 5 increase chance by 10%
    public int current_treasure_progress = 0;
    public int treasure_base_chance = 5;//for with no treasure increase chance by 5% 
    public int difficulty_increase_threshold = 5;//after x scenes increase difficulty by 1 tier
    [Header("Player Progression analytics")]
    public int technical_level => deck.FindAll(x => x.type == hat_type.Transferable).Count;//use this to calc checks

    public int communication_level => deck.FindAll(x => x.type == hat_type.Social).Count();//use this to calc checks

    public int creativity_level => deck.FindAll(x => x.type == hat_type.Critical).Count();//use this to calc checks
    public int events_chosen = 0;
    public int encounters_chosen = 0;
    public int enemies_defeated = 0;
    public int combat_turns_taken = 0;
    public int energy_spent = 0;
    public int event_options_rest = 0;
    public int event_options_encounter = 0;
    public int event_options_skill = 0;
    public int turnsBeforeCounselling = 0;
    public int battlesSkippedByCounsellor = 0;
    public int bossOvertimeCounsellorBuffs = 0;
    public int hostile_encounters = 0;

    [Header("Hat Feedback")]
    [SerializeField] private TextMeshProUGUI hatFeedback1;
    [SerializeField] private TextMeshProUGUI hatFeedback2;
    [SerializeField] private TextMeshProUGUI hatFeedback3;
    private List<TextMeshProUGUI> hatFeedbacks = new List<TextMeshProUGUI>();
    private Queue<string> feedbackQueue = new Queue<string>();
    private bool isDisplayingFeedback = false;
    private float feedbackDuration = 2f; // Duration each message stays visible
    private float timeBetweenMessages = 0.0f; // Time between showing messages
    private Coroutine displayCoroutine;
    public Coroutine addHatCoroutine;

    public character_class_data ClassData => class_data;
    private List<hat.hat_data> ownedHats = new List<hat.hat_data>();

    //actions
    public static Action<bool> hide_ui_event;
    public static Action load_next_scene;
    public static Action done_with_startup;
    public static Action Player_died;
    public static Action done_with_die;

    //Target Icon
    public GameObject targetIcon;

    //Passive Items
    public List<passiveItem> passiveItemList = new List<passiveItem>();

    void Awake()
    {
        if (instance != null && instance != this)//check if another instance exists
        {
            Destroy(this.gameObject);
            return;
        }

        // Initialize feedback system
        if (hatFeedback1 != null && hatFeedback2 != null && hatFeedback3 != null)
        {
            hatFeedbacks.Add(hatFeedback1);
            hatFeedbacks.Add(hatFeedback2);
            hatFeedbacks.Add(hatFeedback3);

            foreach (var feedback in hatFeedbacks)
            {
                feedback.gameObject.SetActive(false);
            }
        }

        if (targetIcon == null)
        {
            targetIcon = GameObject.Find("TargetIcon");
        }
        instance = this;

        deck = new List<hat.hat_data>();
        ownedHats = new List<hat.hat_data>(); // Initialize ownedHats

        globalData = Resources.Load<globaldataholder>("Globaldata");
        globalData.character_classes.TryGetValue(PlayerPrefs.GetInt("selected_class_index", 0), out class_data);

        if (class_data != null)
        {
            init(class_data);

            // Add starting hats to ownedHats
            foreach (var startingHat in class_data.starting_hats)
            {
                if (startingHat != null && startingHat.hat_info != null)
                {
                    ownedHats.Add(startingHat.hat_info.copy());
                }
            }
        }

        base.Start();
        DontDestroyOnLoad(gameObject);
        // Load hats visually when player starts
        Invoke(nameof(DelayedHatLoad), 0.1f);
    }

    private void OnEnable()
    {
        presentation_boss.announce_message += AddHatFeedback;
    }

    private void OnDisable()
    {
        presentation_boss.announce_message -= AddHatFeedback;
    }

    public override void Start()
    {
    }
    private void DelayedHatLoad()
    {
        LoadOwnedHats();
    }

    public void change_handsize(int amount)
    {
        handsize += amount;
        Debug.Log("current hand size is " + handsize);
    }

    public override void change_hp(int amount)
    {

        if (amount < 0)
        {
            //not a heal
            amount += defense_mod;
            int damage_to_shield = Mathf.Min(-amount, shield);
            shield -= damage_to_shield;
            amount += damage_to_shield;
            Debug.Log("Enemy took damage");
            dmg_num_disp.init(-amount, false, damage_to_shield > 0);
            hit_fx_instance.Clear();
            hit_fx_instance.Play();
            if (anim.GetCurrentAnimatorStateInfo(0).IsTag("idle"))
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
            hide_ui_event?.Invoke(false);
        }
        if (current_hp / (float)max_hp < 0.3f)
        {
            foreach (var mat in player_mats)
            {
                mat.mainTexture = class_data.textures[2];
            }
        }
        else if (current_hp / (float)max_hp < 0.6f)
        {
            foreach (var mat in player_mats)
            {
                mat.mainTexture = class_data.textures[1];
            }
        }
        else
        {
            foreach (var mat in player_mats)
            {
                mat.mainTexture = class_data.textures[0];
            }
        }
        burnout_Bar.set_hp(current_hp, max_hp, shield);
    }
    public void change_hp(int amount, bool capped, bool ignore_defense)//this is used for heal and damage that should not be affected by defense mods
    {

        if (amount < 0)
        {
            //not a heal
            if (!ignore_defense)
            {
                amount += defense_mod;
            }
            int damage_to_shield = Mathf.Min(-amount, shield);
            shield -= damage_to_shield;
            amount += damage_to_shield;
            Debug.Log("Enemy took damage");
            dmg_num_disp.init(-amount, false, damage_to_shield > 0);
            hit_fx_instance.Clear();
            hit_fx_instance.Play();
            if (anim.GetCurrentAnimatorStateInfo(0).IsTag("idle"))
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
            current_hp = 1;
        }
        if (current_hp / (float)max_hp < 0.3f)
        {
            foreach (var mat in player_mats)
            {
                mat.mainTexture = class_data.textures[2];
            }
        }
        else if (current_hp / (float)max_hp < 0.6f)
        {
            foreach (var mat in player_mats)
            {
                mat.mainTexture = class_data.textures[1];
            }
        }
        else
        {
            foreach (var mat in player_mats)
            {
                mat.mainTexture = class_data.textures[0];
            }
        }
        burnout_Bar.set_hp(current_hp, max_hp, shield);
    }
    public void change_maxhp(int amount)
    {
        max_hp += amount;
        current_hp = max_hp;
        burnout_Bar.set_hp(current_hp, max_hp, shield);
    }
    public IEnumerator add_hat(hat.hat_data new_hat)
    {
        hat.hat_data existingHat = deck.Find(x => x.name == new_hat.name);

        addHatCoroutine = null;
        if (existingHat != null)
        {
            existingHat.level++;
            Debug.Log($"Increased level of {existingHat.name} to {existingHat.level}");

            AddHatFeedback(FormatHatMessage("upgraded", existingHat));
            yield break;
        }
        yield return StartCoroutine(gainhats(new List<hat.hat_data> { new_hat }));
        Debug.Log($"Added new hat {new_hat.name}");
    }
    public IEnumerator add_hat(List<hat.hat_data> new_hats)//for adding multiple hats at once
    {
        for (int i = 0; i < new_hats.Count; i++)
        {

            hat.hat_data existingHat = deck.Find(x => x.name == new_hats[i].name);
            if (existingHat != null)
            {
                existingHat.level++;
                Debug.Log($"Increased level of {existingHat.name} to {existingHat.level}");

                AddHatFeedback(FormatHatMessage("upgraded", existingHat));
                new_hats.RemoveAt(i);
            }
        }


        yield return StartCoroutine(gainhats(new_hats));
        Debug.Log($"Added new hats");
    }
    public override void change_atk_mod(int amount)
    {
        atk_mod += amount;
        foreach (var hat in hand_ui.card_uis.Where(c => c.gameObject.activeSelf && c.ref_hat != null))
        {
            if (hat.ref_hat.type == hat_type.Transferable)
            {

                hat.ref_hat.pow_mod = atk_mod;
                hat.setup_disc(hat.ref_hat.get_icon_description());
            }
        }
    }
    public override void change_shield_mod(int amount)
    {
        shield_mod += amount;
        foreach (var hat in hand_ui.card_uis.Where(c => c.gameObject.activeSelf && c.ref_hat != null))
        {
            if (hat.ref_hat.type == hat_type.Social)
            {

                hat.ref_hat.pow_mod = shield_mod;
                hat.setup_disc(hat.ref_hat.get_icon_description());
            }
        }
    }

    public List<hat.hat_data> gain_hat_reward(int gatcha_rate)//this is used to give the player a new hat for rewards, rate is from 1-10 10 garrentees hard pool hat
    {
        int option_nums = UnityEngine.Random.Range(1, 3);
        List<hat.hat_data> prizes = new List<hat.hat_data>();
        UnityEngine.Random.InitState(DateTime.Now.Millisecond);
        List<hat.hat_data> valid_hats = new List<hat.hat_data>();
        if (gatcha_rate > UnityEngine.Random.Range(0, 10))
        {
            valid_hats = new List<hat.hat_data>(globalData.get_easyhats());
        }
        else
        {
            valid_hats = new List<hat.hat_data>(globalData.get_hardhats());
        }
        for (int i = valid_hats.Count; i > 0; i--)
        {
            hat.hat_data temp =
deck.Find(x => x.name == valid_hats[i - 1].name);
            if (temp != null)
            {
                if (temp.level >= 1)
                {
                    valid_hats.RemoveAt(i - 1);
                }
            }
        }
        UnityEngine.Random.InitState(DateTime.Now.Millisecond);
        for (int i = 0; i < option_nums; i++)
        {
            hat.hat_data new_hat = valid_hats[UnityEngine.Random.Range(0, valid_hats.Count)];
            valid_hats.Remove(new_hat);

            prizes.Add(new_hat);
        }

        //AddHatFeedback(FormatHatMessage("obtained", new_hat));

        return prizes;
    }
    public void upgrade_hats_random(int amount)//this is used to upgrade a hat for rewards
    {
        for (int i = 0; i < amount; i++)
        {
            UnityEngine.Random.InitState(DateTime.Now.Millisecond);
            List<hat.hat_data> upgradable_hats =
deck.FindAll(x => x.level == 0);
            if (upgradable_hats.Count == 0)
            {
                Debug.Log("No hats available for upgrade");
                return;
            }

            // Get a random hat and upgrade it
            int randomIndex = UnityEngine.Random.Range(0, upgradable_hats.Count);
            var upgradedHat = upgradable_hats[randomIndex];
            upgradedHat.level++;

            AddHatFeedback(FormatHatMessage("upgraded", upgradedHat));
            Debug.Log("Randomly upgraded " + upgradedHat.name);
        }
    }
    public void upgrade_base_hats()
    {
        if (class_data == null)
        {
            Debug.LogWarning("No class data found, cannot upgrade base hats.");
            return;
        }

        foreach (var startingHat in class_data.starting_hats)
        {
            if (startingHat == null || startingHat.hat_info == null)
                continue;

            // Find the matching owned hat
            hat.hat_data ownedHat =
deck.Find(h => h.name == startingHat.hat_info.name);

            if (ownedHat != null)
            {
                ownedHat.level++;
                Debug.Log($"Upgraded base hat {ownedHat.name} to level {ownedHat.level}");
                AddHatFeedback(FormatHatMessage("upgraded", ownedHat));
            }
            else
            {
                Debug.LogWarning($"Base hat {startingHat.hat_info.name} not found in player hat list.");
            }
        }
    }

    public event_data get_valid_event()//use this to get a random event that has not been completed yet
    {

        UnityEngine.Random.InitState((int)DateTime.Now.Millisecond + System.Environment.TickCount + System.Diagnostics.Process.GetCurrentProcess().Id);
        int random_value = UnityEngine.Random.Range(0, 10);
        Vector3 difficulty_rates = get_difficulty_rates();
        List<event_data> event_list = new List<event_data>();
        if (random_value < difficulty_rates.x)
        {
            event_list = new List<event_data>(easy_event_list);
        }
        else if (random_value < difficulty_rates.x + difficulty_rates.y)
        {
            event_list = new List<event_data>(medium_event_list);
        }
        else
        {
            event_list = new List<event_data>(hard_event_list);
        }

        for (int i = event_list.Count; i > 0; i--)
        {
            //remove encounters that have already been completed
            if (progress_log.Exists(x => x.scene_name == event_list[i - 1].name))
            {
                event_list.RemoveAt(i - 1);
            }
        }
        if (event_list.Count == 0)
        {
            Debug.LogWarning("No more available events to select from,using encounter instead");
            return get_valid_encounter();
        }
        event_data selected_event = event_list[UnityEngine.Random.Range(0, event_list.Count)];
        event_list.Remove(selected_event);
        return selected_event;
    }
    public event_data get_valid_encounter()//use this to get a random encounter
    {
        UnityEngine.Random.InitState((int)DateTime.Now.Millisecond + System.Environment.TickCount + System.Diagnostics.Process.GetCurrentProcess().Id);
        Vector3 difficulty_rates = get_difficulty_rates();
        int random_value = UnityEngine.Random.Range(0, 10);
        List<event_data> encounter_list = new List<event_data>();
        if (random_value < difficulty_rates.x)
        {
            encounter_list = new List<event_data>(easy_encounter_list);
        }
        else if (random_value < difficulty_rates.x + difficulty_rates.y)
        {
            encounter_list = new List<event_data>(medium_encounter_list);
        }
        else
        {
            encounter_list = new List<event_data>(hard_encounter_list);
        }
        event_data selected_encounter = encounter_list[UnityEngine.Random.Range(0, encounter_list.Count)];
        return selected_encounter;
    }
    public event_data get_valid_treasure()//use this to get a random treasure
    {
        UnityEngine.Random.InitState((int)DateTime.Now.Millisecond + System.Environment.TickCount + System.Diagnostics.Process.GetCurrentProcess().Id);
        event_data selected_treasure = treasure_list[UnityEngine.Random.Range(0, treasure_list.Count)];
        return selected_treasure;
    }

    public Vector3 get_difficulty_rates()
    {
        int difficulty_tier = (progress_log.Count + 1);
        Vector3 difficulty_rates = new Vector3(0, 0, 0);//x=easy y=medium z=hard
        if (difficulty_tier < difficulty_increase_threshold)
        {
            difficulty_rates.y = (float)10 * (difficulty_tier / (float)difficulty_increase_threshold);
            difficulty_rates.x = Math.Max(0, 10 - difficulty_rates.y);
        }
        else
        {
            difficulty_rates.z = (float)10 * (difficulty_tier / (float)difficulty_increase_threshold);
            difficulty_rates.y = Math.Max(0, 10 - difficulty_rates.y);
        }
        return difficulty_rates;
    }
    public void reset_player()
    {
        current_hp = max_hp;
        active_buffs.Clear();
        buff_fx_instance.Clear();
        debuff_fx_instance.Clear();
        hit_fx_instance.Clear();
        transform.rotation = Quaternion.identity;
        foreach (var mat in player_mats)
        {
            mat.mainTexture = class_data.textures[0];
        }
        atk_mod = 0;//modifies atk done by this entity
        shield_mod = 0;//modifies shield gained by this entity
        defense_mod = 0;//modifies damage taken by this entity
        shield = 0;

        burnout_Bar.set_hp(current_hp, max_hp, shield);
    }
    public void reset_anim()
    {
        foreach (var animparam in anim.parameters)
        {
            if (animparam.type == AnimatorControllerParameterType.Bool)
            {
                anim.SetBool(animparam.name, false);
            }
            else if (animparam.type == AnimatorControllerParameterType.Trigger)
            {
                anim.ResetTrigger(animparam.name);
            }
        }
    }
    public void init(character_class_data class_data)
    {
        max_hp = class_data.max_health;
        handsize = class_data.handsize;

        deck = new List<hat.hat_data>();
        foreach (var mat in player_mats)
        {
            mat.mainTexture = class_data.textures[0];
        }
        for (int i = 0; i < class_data.starting_hats.Count; i++)
        {

            deck.Add(class_data.starting_hats[i].hat_info.copy());
        }
    }

    public void AddAndPlaceHat(hat.hat_data newHat)
    {
        // Check if we already have this hat in ownedHats
        hat.hat_data existingHat = ownedHats.Find(x => x.name == newHat.name);

        if (existingHat != null)
        {
            // Hat already exists, don't place it visually again
            Debug.Log($"Hat {newHat.name} already owned, not placing duplicate");
            return;
        }

        // Add to owned hats list
        ownedHats.Add(newHat.copy()); // Use copy to avoid reference issues

        // Place hat visually
        if (HatManager.Instance != null)
        {
            HatManager.Instance.PlaceHat(newHat);
        }

        Debug.Log($"Added and placed hat: {newHat.name}, Total owned: {ownedHats.Count}");
    }

    public void LoadOwnedHats()
    {
        if (ownedHats == null || ownedHats.Count == 0)
        {
            Debug.Log("No hats to load");
            return;
        }

        Debug.Log($"Loading {ownedHats.Count} owned hats");

        // Clear any existing visual hats first
        if (HatManager.Instance != null)
        {
            HatManager.Instance.ClearAllHats();
        }

        // Place all owned hats visually
        foreach (var hat in ownedHats)
        {
            if (HatManager.Instance != null && hat != null)
            {
                HatManager.Instance.PlaceHat(hat);
            }
        }
    }

    public void AddHatFeedback(string message)
    {
        feedbackQueue.Enqueue(message);

        if (!isDisplayingFeedback && displayCoroutine == null)
        {
            displayCoroutine = StartCoroutine(DisplayFeedbackMessages(2f));
        }
    }

    public void AddHatFeedback(string message, float duration)
    {
        feedbackQueue.Enqueue(message);

        if (!isDisplayingFeedback && displayCoroutine == null)
        {
            displayCoroutine = StartCoroutine(DisplayFeedbackMessages(duration));
        }
    }

    private IEnumerator DisplayFeedbackMessages(float duration)
    {
        isDisplayingFeedback = true;

        yield return null;

        while (feedbackQueue.Count > 0)
        {
            int messagesToShow = Mathf.Min(feedbackQueue.Count, hatFeedbacks.Count);

            for (int i = 0; i < messagesToShow; i++)
            {
                string message = feedbackQueue.Dequeue();
                TextMeshProUGUI feedbackText = hatFeedbacks[i];

                feedbackText.text = message;
                feedbackText.gameObject.SetActive(true);
            }

            yield return new WaitForSeconds(duration);

            for (int i = 0; i < hatFeedbacks.Count; i++)
            {
                StartCoroutine(FadeOutText(hatFeedbacks[i], duration));
            }

            if (feedbackQueue.Count > 0)
            {
                yield return new WaitForSeconds(timeBetweenMessages);
            }
        }

        isDisplayingFeedback = false;
        displayCoroutine = null;
    }

    private string GetHatColorTag(hat_type type)
    {
        switch (type)
        {
            case hat_type.Critical:
                return "red";
            case hat_type.Social:
                return "purple";
            case hat_type.Transferable:
                return "green";
            default:
                return "white";
        }
    }

    private string FormatHatMessage(string text, hat.hat_data hat)
    {
        string color = GetHatColorTag(hat.type);
        return $"You have {text} the <color={color}>{hat.name}</color> hat!";
    }

    private IEnumerator FadeOutText(TextMeshProUGUI text, float duration)
    {
        Color originalColor = text.color;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            text.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        text.gameObject.SetActive(false);
        text.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
    }
    //public void Destroy()
    //{
    //    presentation_boss.announce_message-=AddHatFeedback;
    //} 
    public IEnumerator gainhats(List<hat.hat_data> hats_to_gain)
    {
        anim.SetTrigger("prize");
        yield return new WaitForSeconds(2.2f);
        anim.speed = 0;
        foreach (var hat in hats_to_gain)
        {
            AddAndPlaceHat(hat);

            deck.Add(hat.copy());
            AddHatFeedback(FormatHatMessage("obtained", hat));
            if (hats_to_gain.Count > 1)
            {
                yield return new WaitForSeconds(1.2f);
            }
        }
        anim.speed = 1;
    }

    public void activate_passive_items(passiveItem passiveItem)
    {
        if (passiveItemList == null) return;

        if (passiveItem.passiveItemInfo.type == item_type.Transferable)
        {
            change_atk_mod(passiveItem.passiveItemInfo.atk_mod);
        }
        else
        {
            foreach (var effect in passiveItem.passiveItemInfo.use_effects)
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


    }


    //these are mostly for animation events
    public void die()
    {
        Player_died?.Invoke();
    }
    public void done_dying()
    {
        done_with_die?.Invoke();
    }
    public void next_scene()
    {
        load_next_scene?.Invoke();
    }
    public void toggle_ui(int hide)//0 to hide, 1 to show
    {
        hide_ui_event?.Invoke(hide == 1);
    }
    public void active_done_with_startup()
    {
        done_with_startup?.Invoke();
    }
}
