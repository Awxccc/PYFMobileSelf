using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Splines;

public class handui : MonoBehaviour
{
    private SplineContainer hand_spline;
    [SerializeField]private int selected_card_index = -1;
    [SerializeField]private int hovering_card_index = -1;
    [SerializeField] combat_manager combat_mgr;
    [SerializeField] public List<ui_card> card_uis ;
    [SerializeField] private GameObject leftArrowBtn;
    [SerializeField] private GameObject rightArrowBtn;
    private int maxCardShown = 5;
    private int cardShownIndex = 0;
    public bool uihovering;
    
    public ui_card selected_card;

    public ui_card hovering_card;

    private Vector3 originalCardScale = new Vector3(.7f, .7f, .7f);

    private bool firstDrawnHand = false;
    private ui_card lastSelectedCard;

    public bool tutorialSceneBool;
    public TutorialSceneScript tutorialSceneScript;
    public int typeOfCardUsed;

    public List<ui_card> shownCard_uis;

    void Start()
    {
        tutorialSceneScript = FindAnyObjectByType<TutorialSceneScript>();
        card_uis = new List<ui_card>();
        hand_spline = GetComponent<SplineContainer>();
        for (int i = 0; i < Player_data.instance.handsize; i++)
        {
            GameObject new_card_obj = Instantiate(Resources.Load<GameObject>("card"), transform);
            ui_card new_card = new_card_obj.GetComponent<ui_card>();
            new_card.parent_hand = this;
            card_uis.Add(new_card);
            card_uis[i].gameObject.transform.localScale = originalCardScale;
            card_uis[i].gameObject.SetActive(false);
        }
        for(int i = 0; i < maxCardShown; i++)
        {
            if (i >= Player_data.instance.handsize) break;
            shownCard_uis.Add(card_uis[i]);
        }
        init(Player_data.instance);
    }
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            for (int i = 0; i < Player_data.instance.handsize; i++)
            {
                if (card_uis[i].gameObject.activeSelf) Debug.Log(card_uis[i].ref_hat.name + " is active.");
                else if (!card_uis[i].gameObject.activeSelf) Debug.Log(card_uis[i].ref_hat.name + " is inactive.");
            }
        }

        if (cardShownIndex < Player_data.instance.handsize - maxCardShown)
        {
            rightArrowBtn.SetActive(true);
        }
        else
        {
            rightArrowBtn.SetActive(false);
        }

        if (cardShownIndex > 0)
        {
            leftArrowBtn.SetActive(true);
        }
        else
        {
            leftArrowBtn.SetActive(false);
        }

        int cardcount = math.max(shownCard_uis.Count(c => c.gameObject.activeSelf), shownCard_uis.Count(c => c.gameObject.activeSelf));
        
        if (shownCard_uis.Count == 0||cardcount == 0) return;
        var spacing =1.4f / cardcount - 0.1f;
        var expandedrange = 0.05f+ spacing/5;
        var firstcardpos = 0.5f - (cardcount - 1) * spacing / 2.0f;
        var sp = hand_spline.Spline;
        var num = 0;
        foreach (var c in shownCard_uis)
        {
            if (!c.gameObject.activeSelf)
            {
                c.transform.localPosition = Vector3.zero;
                c.transform.DOKill();
                continue;
            }
            var p = firstcardpos + num * spacing;
            if (selected_card != null)
            {
                if (c != selected_card)
                {
                    if (num < selected_card_index)
                        p -= expandedrange;
                    else
                        p += expandedrange;
                }
            }
            else if (uihovering)
            {
                if (c != hovering_card)
                {
                    if (num < hovering_card_index)
                        p -= expandedrange;
                    else
                        p += expandedrange;
                }
            }

            Vector3 splinepos = sp.EvaluatePosition(p);
            splinepos.z = 0;


            Vector3 fwd = sp.EvaluateTangent(p);
            Vector3 up = sp.EvaluateUpVector(p);
            if (c == hovering_card && c != selected_card)
            {
                
                c.transform.SetSiblingIndex(shownCard_uis.Count - 2);
                splinepos += up * 65* Screen.height / 1000f;
                c.transform.localScale = Vector3.Lerp(c.transform.localScale, originalCardScale * 1.15f, Time.deltaTime * 10);
            }
            else if (c == selected_card)
            {
                
                c.transform.SetSiblingIndex(shownCard_uis.Count - 1);
                splinepos += up * 85 * Screen.height / 1000f;
                c.transform.localScale = Vector3.Lerp(c.transform.localScale, originalCardScale * 1.25f, Time.deltaTime * 10);
            }
            else
            {
                
                c.transform.SetSiblingIndex(num);
                c.transform.localScale = Vector3.Lerp(c.transform.localScale, originalCardScale, Time.deltaTime * 10);
            }
            var rot = quaternion.LookRotation(-Vector3.Cross(up, fwd).normalized, up);

            c.transform.DOLocalMove(splinepos, 1.25f);
            num++;
        }
    }
    public void refresh_cards()
    {
        
    }

    public void init(Player_data player)
    {
        Player_data.instance.hand_ui = this;
        Debug.Log(player.deck.Count + " hats in player inventory");
        draw_hand();
        if (shownCard_uis.Count != 0)
        {
            var spacing = 1.1f / Player_data.instance.handsize - 0.1f;
            var expandedrange = 0.02f;
            var firstcardpos = 0.5f - (Player_data.instance.handsize - 1) * spacing / 2;
            var sp = hand_spline.Spline;
            var num = 0;
            foreach (var c in shownCard_uis)
            {
                if (!c.gameObject.activeSelf) continue;
                var p = firstcardpos + num * spacing;
                if (selected_card != null)
                {
                    if (c != selected_card)
                    {
                        if (num < shownCard_uis.IndexOf(selected_card))
                            p -= expandedrange;
                        else
                            p += expandedrange;
                    }
                }
                else if (uihovering)
                {
                    if (c != hovering_card)
                    {
                        if (num < shownCard_uis.IndexOf(hovering_card))
                            p -= expandedrange;
                        else
                            p += expandedrange;
                    }
                }

                Vector3 splinepos = sp.EvaluatePosition(p);
                Vector3 fwd = sp.EvaluateTangent(p);
                Vector3 up = sp.EvaluateUpVector(p);
                var rot = quaternion.LookRotation(-Vector3.Cross(up, fwd).normalized, up);

                c.transform.localPosition = splinepos;
            }
        }


    }
    public void reset_hand()
    {
        //foreach (var c in card_uis)
        //{
        //    if (!c.ref_hat.retain)
        //    {
        //        removecard(c);
        //    }
        //}
        selected_card = null;
        hovering_card = null;
        uihovering = false;
    }

    public void RemoveCardFromCurrentHand(ui_card card)
    {
        for (int i = 0; i < card_uis.Count; i++)
        {
            if (card_uis[i] == card)
            {
                lastSelectedCard = card;
                Debug.Log("set a go inactive");
                card_uis[i].gameObject.SetActive(false);
                break;
            }
        }
    }

    public void draw_hand()
    {
        int amt_to_draw = Player_data.instance.handsize - card_uis.Count(c => c.gameObject.activeInHierarchy);

        bool containsAttackCard = false;
        bool foundAttackCard = false;
        Debug.Log("Draw: " + amt_to_draw);
        shownCard_uis.Clear();
        if (!firstDrawnHand)
        {
            for (int i = 0; i < amt_to_draw; i++)
            {
                int rand_index = UnityEngine.Random.Range(0, Player_data.instance.deck.Count); //Random hat in player inventory
                card_uis[i].setup_card(Player_data.instance.deck[rand_index], Player_data.instance);
                //card_uis[i].gameObject.SetActive(true);
                if (card_uis[i].ref_hat.type == hat_type.Transferable) containsAttackCard = true;
                if (i == amt_to_draw - 1 && !containsAttackCard)
                {
                    while (!foundAttackCard)
                    {
                        rand_index = UnityEngine.Random.Range(0, Player_data.instance.deck.Count); //Random hat in player inventory
                        if (Player_data.instance.deck[rand_index].type == hat_type.Transferable)
                        {
                            card_uis[i].setup_card(Player_data.instance.deck[rand_index], Player_data.instance);
                            //card_uis[i].gameObject.SetActive(true);
                            foundAttackCard = true;
                        }
                    }
                }
            }
            firstDrawnHand = true;
        }
        else
        {
            for (int i = 0; i < Player_data.instance.handsize; i++)
            {
                if (card_uis[i].gameObject.activeInHierarchy && card_uis[i].ref_hat.type == hat_type.Transferable)
                { 
                    containsAttackCard = true;
                    break;
                }
            }

            if (containsAttackCard)
            {
                for (int i = 0; i < Player_data.instance.handsize; i++)
                {
                    if (card_uis[i] == lastSelectedCard)
                    {
                        Debug.Log("Found card: " + card_uis[i].ref_hat.name);
                        int rand_index = UnityEngine.Random.Range(0, Player_data.instance.deck.Count); //Random hat in player inventory
                        card_uis[i].setup_card(Player_data.instance.deck[rand_index], Player_data.instance);
                        //card_uis[i].gameObject.SetActive(true);
                        break;
                    }
                }
            }
            else
            {
                Debug.Log("does not contain attackcard");
                for (int i = 0; i < Player_data.instance.handsize; i++)
                {
                    if (card_uis[i] == lastSelectedCard)
                    {
                        Debug.Log("Replace with attack card");
                        while (!foundAttackCard)
                        {
                            int rand_index = UnityEngine.Random.Range(0, Player_data.instance.deck.Count); //Random hat in player inventory
                            if (Player_data.instance.deck[rand_index].type == hat_type.Transferable)
                            {
                                card_uis[i].setup_card(Player_data.instance.deck[rand_index], Player_data.instance);
                                //card_uis[i].gameObject.SetActive(true);
                                break;
                            }
                        }
                    }
                }
            }



        }
        for (int i = 0; i < card_uis.Count; i++)
        {
            if (shownCard_uis.Count == 5) break;
            if (i < cardShownIndex)
            {
                continue;
            }
            else
            {
                shownCard_uis.Add(card_uis[i]);
            }
        }
        for (int i = 0; i < shownCard_uis.Count; i++)
        {
            shownCard_uis[i].gameObject.SetActive(true);
        }
    }
    public void hover_card(ui_card card)
    {
        hovering_card = card;
        card.card_name.gameObject.SetActive(true);
        
        card.icon.color = new Color(0.5f,0.5f,0.5f,1);
        card.effect_disp.gameObject.SetActive(true);
        uihovering = true;
        hovering_card_index=0;
        foreach(var c in shownCard_uis)
        {
            if(c.gameObject.activeSelf==false)
            {
                continue;
            }
            if(c==card)
            {
                break;
            }
            hovering_card_index++;
        }
    }
    public void unhover_card(ui_card card)
    {
        if (hovering_card == card)
        {
            hovering_card = null;
            uihovering = false;
            if(card != selected_card)
            {
                
                card.icon.color = Color.white;
                card.card_name.gameObject.SetActive(false);
                card.effect_disp.gameObject.SetActive(false);
            }
        hovering_card_index=-1;

        }
    }
    public void select_card(ui_card card)
    {
        if (selected_card == card) return;

        //if (card.ref_hat.get_cost() > Player_data.instance.current_stamina)
        //{
        //    card.shake_anim.Play();

        //    card.icon.color = new Color(0.5f,0.5f,0.5f,1);
        //    card.card_name.gameObject.SetActive(true);
        //    card.effect_disp.gameObject.SetActive(true);
        //    return;
        //}
        if(selected_card != null)
        {
            selected_card.card_name.gameObject.SetActive(false);
            selected_card.effect_disp.gameObject.SetActive(false);
            selected_card.icon.color = Color.white;
        }
        selected_card = card;
        selected_card_index=0;
        if (selected_card.ref_hat.type == hat_type.Transferable) //Selecting cards
        {
            typeOfCardUsed = 1;
            tutorialSceneScript.ReportAction(TutorialAction.SelectAttackCard);
            tutorialSceneScript.ReportAction(TutorialAction.SelectEnemyTarget);
            //If card is a damaging card then target icon enabled for enemy, disabled for player
            if (Player_data.instance.targetIcon != null) if (Player_data.instance.targetIcon.activeSelf) Player_data.instance.targetIcon.SetActive(false);
            if (Player_data.instance.current_target != null) combat_mgr.currentTargetIcon.SetActive(true);
        }
        else
        {
            typeOfCardUsed = 2;
            tutorialSceneScript.ReportAction(TutorialAction.SelectBuffCard);
            tutorialSceneScript.ReportAction (TutorialAction.SelectPlayerTarget);
            //If card is a buffing card then target icon enabled for player, disabled for enemy
            if (combat_mgr.currentTargetIcon != null) if (combat_mgr.currentTargetIcon.activeSelf) combat_mgr.currentTargetIcon.SetActive(false);
            Player_data.instance.targetIcon.SetActive(true);
        }
        foreach (var c in shownCard_uis)
        {
            if (c.gameObject.activeSelf == false)
            {
                continue;
            }
            if (c == card)
            {
                break;
            }
            selected_card_index++;
        }
        if (tutorialSceneBool == true)
        {
            if (tutorialSceneScript == null)
                tutorialSceneScript = FindAnyObjectByType<TutorialSceneScript>();

            if (selected_card.ref_hat.type == hat_type.Transferable)
                tutorialSceneScript?.ReportAction(TutorialAction.SelectAttackCard);
            else
                tutorialSceneScript?.ReportAction(TutorialAction.SelectBuffCard);
        }
    }
    public void removecard(ui_card card)
    {

        card.icon.color = Color.white;
        card.card_name.gameObject.SetActive(false);
        card.effect_disp.gameObject.SetActive(false);
    
        card.gameObject.SetActive(false);
        
        card.transform.localPosition = Vector3.zero;
    }


    public void RightArrowBtnPressed()
    {
        if (combat_mgr.currentTargetIcon != null) combat_mgr.currentTargetIcon.SetActive(false);
        if (Player_data.instance.targetIcon != null) Player_data.instance.targetIcon.SetActive(false);
        combat_mgr.currentTargetIcon.SetActive(false);
        reset_hand();
        shownCard_uis[0].gameObject.SetActive(false);
        cardShownIndex++;
        shownCard_uis.Clear();
        for (int i = 0; i < card_uis.Count; i++)
        {
            if (shownCard_uis.Count == 5) break;
            if (i < cardShownIndex)
            {
                continue;
            }
            else
            {
                shownCard_uis.Add(card_uis[i]);
            }
        }
        for (int i = 0; i < shownCard_uis.Count; i++)
        {
            shownCard_uis[i].gameObject.SetActive(true);
        }
    }
    public void LeftArrowBtnPressed()
    {
        if (combat_mgr.currentTargetIcon != null) combat_mgr.currentTargetIcon.SetActive(false);
        if (Player_data.instance.targetIcon != null) Player_data.instance.targetIcon.SetActive(false);
        combat_mgr.currentTargetIcon.SetActive(false);
        reset_hand();
        shownCard_uis[maxCardShown - 1].gameObject.SetActive(false);
        cardShownIndex--;
        shownCard_uis.Clear();
        for (int i = 0; i < card_uis.Count; i++)
        {
            if (shownCard_uis.Count == 5) break;
            if (i < cardShownIndex)
            {
                continue;
            }
            else
            {
                shownCard_uis.Add(card_uis[i]);
            }
        }
        for (int i = 0; i < shownCard_uis.Count; i++)
        {
            shownCard_uis[i].gameObject.SetActive(true);
        }
    }


    //public void discard_x(int x)//discards x number of cards
    //{
    //    int discarded = 0;
    //    System.Random rnd = new System.Random();
    //    List<ui_card> shuffled_cards = card_uis.Where(c => c.gameObject.activeSelf&& c!=selected_card&&c.ref_hat != null).OrderBy(c => rnd.Next()).ToList();//
    //    foreach(var c in shuffled_cards)
    //    {
    //        if(c.gameObject.activeSelf&&c!=selected_card&&c.ref_hat != null)
    //        {

    //            removecard(c);
    //            discarded++;
    //            if (discarded >= x) break;
    //        }
    //    }
    //}


    //public void draw_x(int x)
    //{
    //    int amt_to_draw = Mathf.Min(x, max_cards - card_uis.Count(c => c.gameObject.activeSelf));
    //    Debug.Log("Drawing " + amt_to_draw + " cards");
    //    for (int i = 0; i < amt_to_draw; i++)
    //    {
    //        if (deck.Count == 0) break;
    //        int rand_index = UnityEngine.Random.Range(0, deck.Count);
    //        var inactive_card_ui = card_uis.FirstOrDefault(c => !c.gameObject.activeSelf);
    //        if (inactive_card_ui != null)
    //        {
    //            inactive_card_ui.setup_card(deck[rand_index], Player_data.instance);
    //            deck.RemoveAt(rand_index);
    //            inactive_card_ui.gameObject.SetActive(true);
    //            card_uis.Remove(inactive_card_ui);
    //            card_uis.Add(inactive_card_ui);//move to end of list to draw on top 
    //        }
    //        else
    //        {
    //            Debug.LogWarning("No inactive card UI available to draw a card into.");
    //            break;
    //        }
    //    }
    //}
}
