using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;
/** 
@brief combat manager handles the main combat flow
@details A rather complicated script that handles the combat gameplay(clicking characters to attack, character info)
combat works by getting targets and having the user call their own animations/effects on the target
@author Marcus
 */
public class combat_manager : MonoBehaviour
{
    /** 
    @brief counts the number of turns passed,when at 10 turns auto wins
    */
    private int turn_counter = 1;
    /** 
    @brief the player's hand ui
    */
    public handui hand;
    public Transform enemy_parent;
    /** 
    @brief cam used for playing combat anims
    */
    public CinemachineCamera observer_camera;

    /** 
    @brief base cam attached to player
    */
    public CinemachineCamera default_camera;
    private HatPlacement hatPlacementPlayer;
    [Header("Enemy Spawning")]
    /** 
    @brief settings for enemy spawn spacing
    */
    public float enemy_spawn_spacing = 2.5f;
    /** 
    @brief handles selection of prizes after combat
    */
    public selection_manager selectionManager;


    private List<GameObject> enemies_for_spawning = new List<GameObject>();

    [Header("Combat State")]
    public bool done_with_combat;//this is to force end combat from outside scripts
    public TMPro.TMP_Text turn_counter_text;
    public Event scene_event;//leave null if no event is assosiated with this combat
    [Header("buff detail ui")]
    public Transform detailed_buff_parent;
    public Transform detailed_buff_grp;
    public Transform detailed_cards_grp;

    [SerializeField] private Vector3 pos_offsets;
    [SerializeField] private Vector3 look_offsets;

    public TMPro.TMP_Text character_name_text;
    private List<enemy> enemies = new List<enemy>();
    private Vector3 target_origin;

    private Vector3 target_look;
    private List<detailed_buff> detailed_buffs_in_combat = new List<detailed_buff>();
    private List<non_combat_card> card_buffs_in_combat = new List<non_combat_card>();
    private float buff_hold_timer = 0f;
    public Collider buff_hold_target = null;//note make privaste later
    private bool combat_ongoing;

    private bool enemySelected;
    public GameObject currentTargetIcon;
    /** 
    @brief only spawns additional enemies after current enemy turn ends
    */
    private List<GameObject> spawnqueue = new List<GameObject>();
    [Header("prize_selection")]
    public prize_selection prize_selection_panel;

    [Header("Counsellor")]
    public GameObject counsellorPanel;
    public TMPro.TMP_Text counsellorMessageText;
    public Button counsellorContinueButton;
    public Button counsellorSkipButton;
    [SerializeField] private int counsellorTurnInterval = 5;

    [Header("Boss Overtime Counsellor")]
    [SerializeField] private bool isBossFight = false;
    [SerializeField] private int bossOvertimeStartTurn = 5;
    [SerializeField] private int bossOvertimeAttackGainPerRound = 1;
    [SerializeField] private int bossOvertimeShieldGainPerRound = 1;
    [SerializeField] private int bossOvertimeHealPerRound = 1;

    private bool waitingForCounsellorChoice = false;
    private int nextCounsellorTurn = 5;
    /** 
    @brief used for hiding ui
    */
    public static Action<bool> hide_ui_event;
    /** 
    @brief grabs info drom scene and events data, also spawns enemies
    */
    public TutorialSceneScript tutorialSceneScript;
    public bool tutorialBool;

    [SerializeField] private Button combatSpeedButton;
    [SerializeField] private TMPro.TMP_Text combatSpeedButtonText;

    [SerializeField] private float normalCombatSpeed = 1f;
    [SerializeField] private float fastCombatSpeed = 2f;

    private bool combatFastMode = false;
    private float defaultFixedDeltaTime;
    void Start()
    {
        Event.enable_combat += init_spawn;
        Event.end_scene += end_combat;
        advance.skip_turn += adv_trn;
        enemy.defeated_event += remove_enemy;
        Player_data.done_with_startup += startup_event_combat;
        enemy.spawn_additional_enemy += spawn_additional_enemy;
        presentation_boss.Boss_defeated_event += end_combat_boss;
        spawn_enemy.spawn_ads += spawn_additional_enemy;
        hatPlacementPlayer = Player_data.instance.GetComponent<HatPlacement>();
        nextCounsellorTurn = Mathf.Max(1, counsellorTurnInterval);
        SetupCounsellorUI();
        if (Player_data.instance.current_scene == null)
        {
            Debug.LogError("Combat started without proper scene assosiation, defaulting to first scene in list");
        }
        if (Player_data.instance.current_scene.enemies_for_spawning.Count > 0)
        {
            enemies_for_spawning = Player_data.instance.current_scene.enemies_for_spawning;
        }
        if (Player_data.instance.current_scene.event_scene_prefab != null)
        {
            Debug.Log("Event assosiated with this combat, spawning event first in " + Player_data.instance.current_scene.scene_name);
            GameObject event_instance = Instantiate(Player_data.instance.current_scene.event_scene_prefab, this.transform);
            scene_event = event_instance.GetComponentInChildren<Event>();

            scene_event.gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("No event assosiated with this combat, spawning enemies directly in " + Player_data.instance.current_scene.scene_name);
            init_spawn();
        }
        if (tutorialSceneScript == null)
        {
            tutorialSceneScript = GameObject.FindAnyObjectByType<TutorialSceneScript>();
        }
        if (combatSpeedButton == null)
        {
            //combatSpeedButton.gameObject = GameObject.Find("2xSpeedBtn");
            //combatSpeedButtonText.gameObject = GameObject.Find("2xSpeedText");
        }


        Debug.Log("Initializing player data for combat");


        //hand.stamina_text.text = Player_data.instance.current_stamina.ToString() + " / " + Player_data.instance.max_stamina.ToString();
        for (int i = 0; i < 5; i++)
        {
            detailed_buffs_in_combat.Add(Instantiate(Resources.Load<detailed_buff>("detailed_buff"), detailed_buff_grp));
            detailed_buffs_in_combat[i].gameObject.SetActive(false);
        }
        for (int i = 0; i < 5; i++)
        {
            card_buffs_in_combat.Add(Instantiate(Resources.Load<non_combat_card>("non combat card_desc"), detailed_cards_grp));
            card_buffs_in_combat[i].gameObject.SetActive(false);
        }


        hide_ui_event?.Invoke(false);
        detailed_buff_parent.gameObject.SetActive(false);
        Player_data.instance.burnout_Bar.set_hp(Player_data.instance.current_hp, Player_data.instance.max_hp, Player_data.instance.shield);
        if (enemies.Count != 0)
        {
            Player_data.instance.current_target = enemies[0];

            SetEnemyTargetIcon();
        }
        defaultFixedDeltaTime = Time.fixedDeltaTime;

        if (combatSpeedButton != null)
        {
            combatSpeedButton.onClick.AddListener(ToggleCombatSpeed);
        }

        SetCombatSpeed(false);
    }
    void OnDestroy()
    {
        Event.enable_combat -= init_spawn;
        Event.end_scene -= end_combat;
        advance.skip_turn -= adv_trn;
        enemy.defeated_event -= remove_enemy;
        Player_data.done_with_startup -= startup_event_combat;
        enemy.spawn_additional_enemy -= spawn_additional_enemy;
        presentation_boss.Boss_defeated_event -= end_combat_boss;
        spawn_enemy.spawn_ads -= spawn_additional_enemy;

        if (counsellorContinueButton != null)
        {
            counsellorContinueButton.onClick.RemoveListener(ContinueBattleAfterCounsellor);
        }
        if (counsellorSkipButton != null)
        {
            counsellorSkipButton.onClick.RemoveListener(SkipBattleFromCounsellor);
        }
        if (combatSpeedButton != null)
        {
            combatSpeedButton.onClick.RemoveListener(ToggleCombatSpeed);
        }

        ResetCombatSpeed();
    }
    // Update is called once per frame
    /** 
    @brief handles all input related features, clicking enemies to attack, hovering for buff details etc
    @details also has some cheat inputs for testing purposes
    */
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            // Debug skip: ends the fight without granting rewards.
            SkipBattleWithoutRewards();
        }
        if (done_with_combat)
        {
            end_combat();
            done_with_combat = false;
        }
        if (waitingForCounsellorChoice)
        {
            return;
        }
        if (detailed_buff_parent.gameObject.activeSelf)
        {
            return;//dont process input if detailed buff ui is open
        }
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (hand.selected_card != null)
            {
                if (touch.phase == TouchPhase.Began)
                {
                    if (hand.selected_card != null)
                    {
                        evaluate_combat_click(touch.position);
                    }
                }
            }
            else if (touch.phase == TouchPhase.Stationary || touch.phase == TouchPhase.Moved)
            {
                evaluate_combat_hover(Input.mousePosition);
            }
            else
            {
                reset_hold_icon();
            }

        }
        else if (Input.GetMouseButtonDown(0))
        {
            if (hand.selected_card != null)
            {
                evaluate_combat_click(Input.mousePosition);
            }

        }
        else if (Input.GetMouseButton(0))
        {
            evaluate_combat_hover(Input.mousePosition);
        }
        if (Input.GetMouseButtonUp(0))
        {
            reset_hold_icon();
        }

    }

    //Function called by button to confirm attack
    public void ConfirmAttack()
    {
        if (hand.selected_card == null) return;
        if (hand.selected_card.ref_hat.type == hat_type.Transferable)
        {
            if (Player_data.instance.current_target == null) return;
            currentTargetIcon.SetActive(false);
        }
        else
        {
            Player_data.instance.targetIcon.SetActive(false);
        }

        foreach (passiveItem passive in Player_data.instance.passiveItemList)
        {
            if (passive.passiveItemInfo.type == item_type.Transferable)
            {
                Player_data.instance.activate_passive_items(passive);
            }
        }

        //activate card effect here
        Player_data.instance.queued_hat = hand.selected_card.ref_hat;
        Player_data.instance.activate_use_effects();
        hand.removecard(hand.selected_card);
        hand.RemoveCardFromCurrentHand(hand.selected_card);
        hand.selected_card = null;

        if (tutorialBool == true)
        {
            if (tutorialSceneScript == null)
                tutorialSceneScript = FindAnyObjectByType<TutorialSceneScript>();

            tutorialSceneScript?.ReportAction(TutorialAction.UseHat);
        }
        end_turn();
    }

    /** 
    @brief advances the turn and checks whether the counsellor should intervene
    */
    public void end_turn()
    {
        if (currentTargetIcon != null) currentTargetIcon.SetActive(false);

        AdvanceTurnCounter(1);

        if (TryHandleCounsellorAfterPlayerTurn())
        {
            return;
        }

        foreach (passiveItem passive in Player_data.instance.passiveItemList)
        {
            if (passive.passiveItemInfo.type != item_type.Transferable)
            {
                Debug.Log("Found medkit");
                Player_data.instance.activate_passive_items(passive);
            }
        }

        WaitForDelay(2);

        StartCoroutine(start_enemy_turn());
        //enemy turn logic here
    }
    /** 
    @brief checks if valid target for targeting
    */
    public void evaluate_combat_click(Vector2 screen_pos)
    {
        entity hit_enemy = null;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(screen_pos), out RaycastHit hitInfo, 1000f, LayerMask.GetMask("enemy", "player")))
        {
            hit_enemy = hitInfo.collider.GetComponent<entity>();

        }

        if (hit_enemy != null)//this check is unneccary since the card cant be selected if there isnt enough stamina
        {
            StartCoroutine(active_click(hit_enemy));

        }

    }
    /** 
    @brief checks if valid target checking details
    */
    public void evaluate_combat_hover(Vector2 screen_pos)
    {
        //may add hover effects later

        if (Physics.Raycast(Camera.main.ScreenPointToRay(screen_pos), out RaycastHit hitInfo, 1000f, LayerMask.GetMask("enemy", "player")))
        {

            if (buff_hold_target != hitInfo.collider)
            {
                reset_hold_icon();
            }
            if (buff_hold_target == null)
            {
                entity temp = hitInfo.collider.GetComponent<entity>();
                if (temp != null)
                {
                    buff_hold_target = hitInfo.collider;
                }
                else
                {
                    return;
                }
            }
            buff_hold_timer += Time.deltaTime;
            if (buff_hold_timer >= 0.30f && buff_hold_target != null)
            {
                open_detailed_buff_ui(buff_hold_target.GetComponent<entity>());
            }

        }
    }
    /** 
    @brief starts the sequence for using a card on an enemy
    */
    IEnumerator active_click(entity hit_enemy)
    {
        if (hand.selected_card.ref_hat.type == hat_type.Transferable)
        {
            Player_data.instance.current_target = hit_enemy;

            if (hit_enemy.gameObject.layer == LayerMask.NameToLayer("enemy"))
            {
                tutorialSceneScript?.ReportAction(TutorialAction.SelectEnemyTarget);

                RectTransform[] children = Player_data.instance.current_target.GetComponentsInChildren<RectTransform>(true);
                foreach (RectTransform t in children)
                {
                    if (t.CompareTag("TargetIcon"))
                    {
                        if (currentTargetIcon != null) currentTargetIcon.SetActive(false);

                        currentTargetIcon = t.gameObject;
                        currentTargetIcon.SetActive(true);
                    }
                }
            }
            else
            {
                tutorialSceneScript?.ReportAction(TutorialAction.SelectPlayerTarget);
            }
        }

        yield return null;
    }
    /** 
    @brief enemy turn sequence
    @details processes enemy turns one by one, then handles delayed spawning of additional enemies afterwards also process player turn
    */
    IEnumerator start_enemy_turn()
    {
        combat_ongoing = true;
        hide_ui_event?.Invoke(false);
        yield return new WaitForSeconds(1f);
        foreach (var e in enemies.ToList())
        {
            if (e == null) continue;
            if (e.isDying) continue;

            e.shield = 0;
            e.burnout_Bar.set_hp(e.current_hp, e.max_hp, e.shield);

            yield return e.enemy_turn();

            if (Player_data.instance.current_hp <= 0)
            {
                yield break;
            }
        }
        foreach (var spawn in spawnqueue)
        {

            Debug.Log("delayed spawning additional enemy    at pos " + spawn.transform.localPosition.z / enemy_spawn_spacing);
            enemy enemy_instance = spawn.GetComponent<enemy>();
            enemy_instance.init();

            enemy_instance.Start();
            enemy_instance.current_target = Player_data.instance;
            enemies.Add(enemy_instance);
        }
        spawnqueue.Clear();
        foreach (var e in enemies.ToList())
        {
            if (e == null) continue;
            if (e.isDying) continue;

            e.process_buffs();
            e.queue_next_atk();
        }
        //do turn end reset stuff for player here
        Player_data.instance.shield = 0;
        Player_data.instance.process_buffs();
        //hand.stamina_text.text = Player_data.instance.current_stamina.ToString() + " / " + Player_data.instance.max_stamina.ToString();
        Player_data.instance.burnout_Bar.set_hp(Player_data.instance.current_hp, Player_data.instance.max_hp, Player_data.instance.shield);
        Debug.Log("Player turn started");
        hand.reset_hand();
        hand.draw_hand();

        yield return new WaitForSeconds(2);

        hide_ui_event?.Invoke(true);

        combat_ongoing = false;
    }

    /** 
    @brief some additionaly initalization for spawning enemies
    */
    public void init_spawn()
    {

        Debug.Log("initing");
        Player_data.instance.anim.SetTrigger("combat");

        hide_ui_event?.Invoke(true);
        for (int i = 0; i < enemies_for_spawning.Count; i++)
        {
            enemy enemy_instance = Instantiate(enemies_for_spawning[i], enemy_parent).GetComponent<enemy>();
            float val = i - (enemies_for_spawning.Count - 1) / 2.0f;

            enemy_instance.transform.localPosition = new Vector3(0, 0, val * enemy_spawn_spacing);
            enemy_instance.init();
            enemy_instance.current_target = Player_data.instance;
            enemies.Add(enemy_instance);
            //might add intergration for not spawning enemies if it's an event...
        }
    }

    /** 
    @brief for spawning additional enemies during combat
    @param enemy_to_spawn the enemy prefab to spawn
    @param pos the position index to spawn at, please avoid spawning in middle of existing enemies
    */
    public void spawn_additional_enemy(GameObject enemy_to_spawn, float pos)//please do not spawn in middle of existing enemies(will look reallll weird)
    {
        Debug.Log("Spawning additional enemy at pos " + pos);
        if (enemies.Count >= 3)
        {
            return;
        }
        if (combat_ongoing == true)
        {
            GameObject temp = Instantiate(enemy_to_spawn, enemy_parent);
            temp.transform.localPosition = new Vector3(0, 0, pos * enemy_spawn_spacing);
            spawnqueue.Add(temp);
            return;
        }

        enemy enemy_instance = Instantiate(enemy_to_spawn, enemy_parent).GetComponent<enemy>();

        enemy_instance.transform.localPosition = new Vector3(0, 0, pos * enemy_spawn_spacing);

        enemy_instance.current_target = Player_data.instance;
        enemies.Add(enemy_instance);
    }
    /** 
    @brief for spawning additional enemies during combat at a valid pos
    */
    public void spawn_additional_enemy(GameObject enemy_to_spawn)//varient that spawns at a auto pos
    {

        if (enemies.Count >= 3)
        {
            return;
        }
        int pos = enemies.Count % 2 == 0 ? 1 : -1;

        Debug.Log("Spawning additional enemy at pos " + pos);
        if (combat_ongoing == true)
        {
            GameObject temp = Instantiate(enemy_to_spawn, enemy_parent);
            temp.transform.localPosition = new Vector3(0, 0, pos * enemy_spawn_spacing);
            spawnqueue.Add(temp);
            return;
        }

        Debug.Log("sp ");
        enemy enemy_instance = Instantiate(enemy_to_spawn, enemy_parent).GetComponent<enemy>();

        enemy_instance.transform.localPosition = new Vector3(0, 0, pos * enemy_spawn_spacing);
        enemy_instance.init();
        enemy_instance.Start();
        enemy_instance.current_target = Player_data.instance;
        enemies.Add(enemy_instance);
    }

    /** 
    @brief for removing enemies when defeated
    */
    public void remove_enemy(enemy enemy_to_remove)
    {
        Debug.Log("remove_enemy() called for: " + enemy_to_remove.gameObject.name);

        enemies.Remove(enemy_to_remove);
        Player_data.instance.enemies_defeated++;
        Destroy(enemy_to_remove.gameObject);

        if (enemies.Count == 0)
        {
            hide_ui_event?.Invoke(false);
            prize_selection_panel.gameObject.SetActive(true);

            List<hat.hat_data> temp = Player_data.instance.gain_hat_reward(Player_data.instance.current_scene.scene_gacha_rate);
            prize_selection_panel.load_prizes(temp);

            turn_counter = 1;
        }
        else
        {
            Player_data.instance.current_target = enemies[0];
            SetEnemyTargetIcon();
        }
    }
    public void reset_hold_icon()
    {
        buff_hold_timer = 0f;
        buff_hold_target = null;
    }

    /** 
    @brief loads prizes and cleans up post combat
    */
    public void end_combat()
    {
        ResetCombatSpeed();
        Player_data.instance.current_target = null;
        selectionManager.gameObject.SetActive(true);
        turn_counter = 1;
        nextCounsellorTurn = Mathf.Max(1, counsellorTurnInterval);
        //hatPlacementPlayer.PlaceHat(temp[0]);
        foreach (var e in enemies)
        {
            Destroy(e.gameObject);
        }
        enemies.Clear();
        if (Player_data.instance.current_hp <= 0)
        {
            Debug.Log("Player died during combat, not proceeding to scene selection");
            return;
        }
        Player_data.instance.anim.SetTrigger("mid");
        Player_data.instance.active_buffs.Clear();
        for (int i = 0; i < Player_data.instance.burnout_Bar.buff_icons.Count; i++)
        {
            Player_data.instance.burnout_Bar.buff_icons[i].gameObject.SetActive(false);
        }
        Player_data.instance.atk_mod = 0;
        Player_data.instance.shield_mod = 0;

        Player_data.instance.defense_mod = 0;


        Debug.Log("Combat ended");
    }

    public void end_combat_boss()
    {
        ResetCombatSpeed();
        selectionManager.gameObject.SetActive(true);
        turn_counter = 1;
        nextCounsellorTurn = Mathf.Max(1, counsellorTurnInterval);
        //hatPlacementPlayer.PlaceHat(temp[0]);
        foreach (var e in enemies)
        {
            Destroy(e.gameObject);
        }
        enemies.Clear();
        if (Player_data.instance.current_hp <= 0)
        {
            Debug.Log("Player died during combat, not proceeding to scene selection");
            return;
        }
        Player_data.instance.active_buffs.Clear();
        for (int i = 0; i < Player_data.instance.burnout_Bar.buff_icons.Count; i++)
        {
            Player_data.instance.burnout_Bar.buff_icons[i].gameObject.SetActive(false);
        }
        Player_data.instance.atk_mod = 0;
        Player_data.instance.shield_mod = 0;

        Player_data.instance.defense_mod = 0;
        selectionManager.proceed_to_selected_scene("MountainEnding");
    }
    public void startup_event_combat()
    {
        if (scene_event != null)
        {
            scene_event.gameObject.SetActive(true);
            Debug.Log("loaded event");
        }
        else
        {

            hide_ui_event?.Invoke(true);
        }
    }

    /** 
    @brief opens the b uff ui and populates it with the target entity's buffs and hats
    */
    public void open_detailed_buff_ui(entity target_entity)
    {

        hide_ui_event?.Invoke(false);
        detailed_buff_parent.gameObject.SetActive(true);
        character_name_text.text = target_entity.gameObject.name + " " + target_entity.current_hp + "/" + target_entity.max_hp + " hp";
        List<hat.hat_data> hats_to_display = new List<hat.hat_data>();
        foreach (var buff in target_entity.active_buffs)
        {
            foreach (var detailed_buff in detailed_buffs_in_combat)
            {
                if (!detailed_buff.gameObject.activeSelf)
                {
                    detailed_buff.gameObject.SetActive(true);
                    detailed_buff.setup_icon(buff);
                    break;
                }
            }
        }
        bool highlightcard = false;
        if (target_entity is Player_data)
        {
            hats_to_display = ((Player_data)target_entity).deck;
            ui_card temp = ((Player_data)target_entity).hand_ui.selected_card;
            if (temp != null)
            {
                hats_to_display.RemoveAll(x => x.name == temp.ref_hat.name);
                hats_to_display.Insert(0, temp.ref_hat);
                highlightcard = true;
            }

        }
        else if (target_entity is enemy)
        {
            hats_to_display = ((enemy)target_entity).deck;
            if (target_entity.queued_hat != null)
            {
                if (!((enemy)target_entity).pattern)
                {
                    hats_to_display.RemoveAll(x => x.name == target_entity.queued_hat.name);
                    hats_to_display.Insert(0, target_entity.queued_hat);
                }
                highlightcard = true;
            }
        }// ill do some special stuff for the boss later
        else
        {
            Debug.LogError("Target entity is neither player nor enemy, cannot display hats");
            return;
        }


        foreach (var hats in hats_to_display)
        {
            foreach (var card in card_buffs_in_combat)
            {
                if (!card.gameObject.activeSelf)
                {
                    if (highlightcard)
                    {
                        card.border.color = Color.red;
                        highlightcard = false;
                    }
                    else if (hats.level > 0)
                    {
                        card.border.color = Color.orange;
                    }
                    else
                    {
                        card.border.color = new Color(0.3283019f, 0.3283019f, 0.3283019f, 0.5529412f);
                    }
                    card.gameObject.SetActive(true);
                    card.setup_card(hats, target_entity);
                    break;
                }
            }
        }
        target_origin = target_entity.transform.position + target_entity.transform.forward * pos_offsets.x + target_entity.transform.right * pos_offsets.y + target_entity.transform.up * pos_offsets.z;
        target_look = target_entity.transform.position + target_entity.transform.forward * look_offsets.x + target_entity.transform.right * look_offsets.y + target_entity.transform.up * look_offsets.z;
        observer_camera.transform.DOMove(target_origin, 1);
        observer_camera.transform.DOLookAt(target_look, 1f);
        observer_camera.Priority = 999;

    }
    public void close_detailed_buff_ui()
    {
        foreach (var detailed_buff in detailed_buffs_in_combat.Where(x => x.gameObject.activeSelf))
        {
            detailed_buff.gameObject.SetActive(false);
        }
        foreach (var card_buff in card_buffs_in_combat.Where(x => x.gameObject.activeSelf))
        {
            card_buff.gameObject.SetActive(false);
        }

        detailed_buff_parent.gameObject.SetActive(false);

        hide_ui_event?.Invoke(true);
        StartCoroutine(transitionbackto_default_camera());
    }
    public IEnumerator transitionbackto_default_camera()
    {

        observer_camera.transform.DOMove(default_camera.transform.position, 0.5f);
        observer_camera.transform.DORotateQuaternion(default_camera.transform.rotation, 0.5f);
        while (Vector3.Distance(observer_camera.transform.position, default_camera.transform.position) > 0.1f)
        {
            yield return null;
        }
        observer_camera.Priority = 0;
    }

    private void SetupCounsellorUI()
    {
        if (counsellorPanel != null)
        {
            counsellorPanel.SetActive(false);
        }

        if (counsellorContinueButton != null)
        {
            counsellorContinueButton.onClick.RemoveListener(ContinueBattleAfterCounsellor);
            counsellorContinueButton.onClick.AddListener(ContinueBattleAfterCounsellor);
        }

        if (counsellorSkipButton != null)
        {
            counsellorSkipButton.onClick.RemoveListener(SkipBattleFromCounsellor);
            counsellorSkipButton.onClick.AddListener(SkipBattleFromCounsellor);
        }
    }

    private void AdvanceTurnCounter(int amount)
    {
        turn_counter += amount;
        Player_data.instance.combat_turns_taken++;
        UpdateTurnCounterText();
    }

    private void UpdateTurnCounterText()
    {
        if (turn_counter_text == null) return;

        if (IsBossFight())
        {
            if (turn_counter < bossOvertimeStartTurn)
            {
                turn_counter_text.text = turn_counter.ToString() + "/" + bossOvertimeStartTurn.ToString();
            }
            else
            {
                turn_counter_text.text = turn_counter.ToString() + " OT";
            }
        }
        else
        {
            turn_counter_text.text = turn_counter.ToString() + "/" + nextCounsellorTurn.ToString();
        }
    }

    private bool TryHandleCounsellorAfterPlayerTurn()
    {
        if (IsBossFight())
        {
            if (turn_counter > bossOvertimeStartTurn)
            {
                Player_data.instance.turnsBeforeCounselling++;
                ApplyBossOvertimeCounsellorBuff();
            }

            return false;
        }

        if (counsellorTurnInterval <= 0) return false;

        if (turn_counter > nextCounsellorTurn)
        {
            Player_data.instance.turnsBeforeCounselling++;
            nextCounsellorTurn += counsellorTurnInterval;
            OpenCounsellorPrompt();
            return true;
        }

        return false;
    }

    private void OpenCounsellorPrompt()
    {
        if (counsellorPanel == null)
        {
            Debug.LogWarning("Counsellor panel is not assigned. Continuing battle automatically.");
            StartCoroutine(start_enemy_turn());
            return;
        }

        waitingForCounsellorChoice = true;
        hide_ui_event?.Invoke(false);

        if (counsellorMessageText != null)
        {
            counsellorMessageText.text = "The counsellor has arrived. Do you want to continue fighting, or skip this battle with no rewards?";
        }

        if (counsellorSkipButton != null)
        {
            counsellorSkipButton.gameObject.SetActive(true);
            counsellorSkipButton.interactable = true;
        }

        counsellorPanel.SetActive(true);
    }

    public void ContinueBattleAfterCounsellor()
    {
        if (!waitingForCounsellorChoice) return;

        waitingForCounsellorChoice = false;
        if (counsellorPanel != null)
        {
            counsellorPanel.SetActive(false);
        }

        StartCoroutine(start_enemy_turn());
    }

    public void SkipBattleFromCounsellor()
    {
        if (!waitingForCounsellorChoice) return;

        if (IsBossFight())
        {
            ContinueBattleAfterCounsellor();
            return;
        }

        Player_data.instance.battlesSkippedByCounsellor++;
        Player_data.instance.AddHatFeedback("Battle skipped. No rewards gained.", 3);
        SkipBattleWithoutRewards();
    }

    private void SkipBattleWithoutRewards()
    {
        waitingForCounsellorChoice = false;

        if (counsellorPanel != null)
        {
            counsellorPanel.SetActive(false);
        }

        if (prize_selection_panel != null)
        {
            prize_selection_panel.gameObject.SetActive(false);
        }

        hide_ui_event?.Invoke(false);
        end_combat();
    }

    private bool IsBossFight()
    {
        return isBossFight || FindAnyObjectByType<presentation_boss>() != null;
    }

    private void ApplyBossOvertimeCounsellorBuff()
    {
        Player_data.instance.bossOvertimeCounsellorBuffs++;

        if (bossOvertimeAttackGainPerRound != 0)
        {
            Player_data.instance.change_atk_mod(bossOvertimeAttackGainPerRound);
        }

        if (bossOvertimeShieldGainPerRound != 0)
        {
            Player_data.instance.change_shield_mod(bossOvertimeShieldGainPerRound);
        }

        if (bossOvertimeHealPerRound > 0 && Player_data.instance.current_hp < Player_data.instance.max_hp)
        {
            Player_data.instance.change_hp(bossOvertimeHealPerRound, true, true);
        }

        Player_data.instance.AddHatFeedback("Overtime counselling: you feel stronger!", 2f);
    }

    /** 
    @brief activates when player presses end turn button
    */
    public void adv_trn(int i)
    {
        if (waitingForCounsellorChoice) return;

        AdvanceTurnCounter(i);

        if (TryHandleCounsellorAfterPlayerTurn())
        {
            return;
        }

        StartCoroutine(start_enemy_turn());
    }
    public void auto_atk()
    {
        if (hand.selected_card == null) return;
        entity target = enemies.OrderBy(e => e.current_hp).First();
        StartCoroutine(active_click(target));
    }

    public void SetEnemyTargetIcon()
    {
        RectTransform[] children = Player_data.instance.current_target.GetComponentsInChildren<RectTransform>(true);
        foreach (RectTransform t in children)
        {
            if (t.CompareTag("TargetIcon"))
            {
                if (currentTargetIcon != null) currentTargetIcon.SetActive(false);

                currentTargetIcon = t.gameObject;
            }
        }
    }
    public void ToggleCombatSpeed()
    {
        SetCombatSpeed(!combatFastMode);
        if (tutorialBool == true)
        {
            if (tutorialSceneScript == null)
                tutorialSceneScript = FindAnyObjectByType<TutorialSceneScript>();

            tutorialSceneScript?.ReportAction(TutorialAction.ToggleSpeed);
        }
    }

    public void SetCombatSpeed(bool fast)
    {
        combatFastMode = fast;

        Time.timeScale = combatFastMode ? fastCombatSpeed : normalCombatSpeed;

        Time.fixedDeltaTime = defaultFixedDeltaTime * Time.timeScale;

        UpdateCombatSpeedButtonText();
    }

    private void UpdateCombatSpeedButtonText()
    {
        if (combatSpeedButtonText == null) return;

        combatSpeedButtonText.text = combatFastMode ? "2x Speed" : "1x Speed";
    }

    private void ResetCombatSpeed()
    {
        Time.timeScale = 1f;

        if (defaultFixedDeltaTime > 0f)
        {
            Time.fixedDeltaTime = defaultFixedDeltaTime;
        }
    }

    IEnumerator WaitForDelay(int delayTime)
    {
        yield return new WaitForSeconds(delayTime);
    }
}
