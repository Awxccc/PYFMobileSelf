using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class character_select : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Carousel Settings")]
    public int radius = 5;

    public float turnspeed = 5f;//speed for rotating the wheel
    public float dragspeed = 0.1f;//drag sense
    public float tilt_angle=0;//may  remove later
    public float currentangle=0f;
    [HideInInspector] public bool dragging = false;
    
    public  float target_angle = 0f;//target angle to rotate to
    private float yvelocity=0f;//current y rotation speed
    private float angle = 0f;//angle between options
    public int selected_option_index = 4;
    private globaldataholder globalData;
    private bool valid_selection = false;
    [Header("Character card layout")]
    
    [SerializeField] private List<carousel_option> option_scripts = new List<carousel_option>();
    [SerializeField] private Animator cardlayout_animator;
    [SerializeField] private TMPro.TMP_Text class_name_text;
    [SerializeField] private TMPro.TMP_Text class_desc_text;
    [SerializeField] private List<non_combat_card> card_layout_cards = new List<non_combat_card>();
    [SerializeField] private Image class_color_bg;
    [SerializeField] private Image HP_bar;
    
    [SerializeField] private Image hand_bar;
public  Transform light_transform;
    TutorialSceneScript tutorialSceneScript;
    [Header("Confirmation stuff")]
    //public string next_scene_name = "combat_blockout";
    public string next_scene_name = "TutorialCombatV2";
    //will probably add some animation stuff here as well....

    void Start()
    {
        if (tutorialSceneScript == null)
            tutorialSceneScript = FindAnyObjectByType<TutorialSceneScript>();
        option_scripts = new List<carousel_option>();
        globalData = Resources.Load<globaldataholder>("Globaldata");
        GameObject[] options = Resources.LoadAll<GameObject>("Character_classes");//loads all character class prefabs from resources folder
        angle = 360f / options.Length;
        // Wrap so negative angles correctly pick previous options (e.g. -90 -> last index)
        if(selected_option_index<0)
        {
            selected_option_index+= option_scripts.Count;
        } 
        for (int i = 0; i < options.Length; i++)
        {
            float anglestep = (i-1) * angle * Mathf.Deg2Rad;

            Vector3 offset = new Vector3(
                Mathf.Cos(anglestep) * radius,
                1.5f,
                Mathf.Sin(anglestep) * radius
            );

            Vector3 spawnPos = transform.position + offset;

            GameObject temp = Instantiate(options[i], spawnPos, Quaternion.identity, transform);
            temp.transform.LookAt(transform.position +  new Vector3(
                Mathf.Cos(anglestep) * radius*2,
                1.5f,
                Mathf.Sin(anglestep) * radius*2
            ));
            
            Debug.Log("Spawning option "+ options[i].name+" at angle "+ temp.transform.rotation.eulerAngles.y);
            carousel_option option =temp.GetComponent<carousel_option>();
            option.class_data = globalData.character_classes.TryGetValue(option.option_index,out var classData)? classData : null;
            option_scripts.Add(temp.GetComponent<carousel_option>());
        }

        
        valid_selection = true;            
        for(int i=0;i< option_scripts.Count;i++)
            {
            if (i == selected_option_index)
            {

                Debug.Log("Selected option " + option_scripts[i].class_data.option_name);
                option_scripts[i].anim.SetBool("selected", true);
                class_name_text.text = option_scripts[i].class_data.option_name;
                class_name_text.color = option_scripts[i].class_data.ui_color;
                class_desc_text.text = option_scripts[i].class_data.option_desc;
                class_color_bg.color = option_scripts[i].class_data.ui_color;
                HP_bar.fillAmount = option_scripts[i].class_data.max_health / 20f;
                hand_bar.fillAmount = option_scripts[i].class_data.handsize / 12f;
                for (int j = 0; j < card_layout_cards.Count; j++)
                {
                    Debug.Log("Setting up card " + j);
                    if (j < option_scripts[i].class_data.starting_hats.Count)
                    {
                        card_layout_cards[j].setup_card(option_scripts[i].class_data.starting_hats[j].hat_info, null);
                        card_layout_cards[j].gameObject.SetActive(true);
                    }
                    else
                    {
                        card_layout_cards[j].gameObject.SetActive(false);
                    }
                }
                //also assign cards to card layout here + names etc
                
            }
        }
    }

    // Update is called once per frame
    void Update()
    { 
        currentangle += yvelocity*Time.deltaTime;

        transform.localRotation = Quaternion.AngleAxis(currentangle, Vector3.up);//rotation physics for the whole carousel
        
        cardlayout_animator.SetBool("active", valid_selection);
    }
    void FixedUpdate()
    {        
        if (math.abs(target_angle - currentangle) < 4f)//slow down to position if close enough
        {
            yvelocity =yvelocity*0.7f;
            if (!valid_selection)
                {
                    valid_selection = true;
                    light_transform.gameObject.SetActive(true);
                    cardlayout_animator.SetBool("active", true);
                    for(int i=0;i< option_scripts.Count;i++)
                    {
                        if(i==selected_option_index)
                        {
                            StartCoroutine(lerp_stat_vals(option_scripts[i]));
                            Debug.Log("Selected option "+ option_scripts[i].class_data.option_name);
                            option_scripts[i].anim.SetBool("selected",true);
                            class_name_text.text = option_scripts[i].class_data.option_name;
                            class_name_text.color=option_scripts[i].class_data.ui_color;
                            class_desc_text.text = option_scripts[i].class_data.option_desc;
                            
                            class_color_bg.color = option_scripts[i].class_data.ui_color;
                            for (int j = 0; j < card_layout_cards.Count; j++)
                            {
                                Debug.Log("Setting up card "+j);
                                if (j < option_scripts[i].class_data.starting_hats.Count)
                                {
                                    card_layout_cards[j].setup_card(option_scripts[i].class_data.starting_hats[j].hat_info, null);
                                    card_layout_cards[j].gameObject.SetActive(true);
                                }
                                else
                                {
                                    card_layout_cards[j].gameObject.SetActive(false);
                                }
                            }
                            //also assign cards to card layout here + names etc
                        }
                        else
                        {
                            option_scripts[i].anim.SetBool("selected",false);
                        }
                    }
                }
            if(math.abs(yvelocity)<0.8f) //snap
            {
                yvelocity=0;
                

            }
          //do nothing for now  
        }
        else
        {
            if (valid_selection)
            {
                valid_selection = false;
                cardlayout_animator.SetBool("active", false);
                for(int i=0;i< option_scripts.Count;i++)
                {
                    option_scripts[i].anim.SetBool("selected",false);
                }
            }
            if(target_angle- currentangle >0)
            {
            yvelocity = Time.deltaTime * turnspeed  + yvelocity*0.9f;
            }
            else if(target_angle- currentangle   <0)
            {
                yvelocity = -Time.deltaTime * turnspeed + yvelocity*0.9f;
            }
        }

        yvelocity = math.clamp(yvelocity, -turnspeed, turnspeed);
    }
    public void turnleft()
    {
        if(dragging)
        {
            return;
        }
        if (valid_selection)
        {
            valid_selection = false;
            for(int i=0;i< option_scripts.Count;i++)
            {
                option_scripts[i].anim.SetBool("selected",false);
            }
        }
        
        light_transform.gameObject.SetActive(false);
        valid_selection = false;
        target_angle-=angle;
        selected_option_index--;
        if (selected_option_index < 0)
        {
            selected_option_index = option_scripts.Count - 1;
        }
        tutorialSceneScript.ReportAction(TutorialAction.ChangeCharacter);
    }
    public void turnright()
    {
        if(dragging)
        {
            return;
        }
        if (valid_selection)
        {
            valid_selection = false;
            for(int i=0;i< option_scripts.Count;i++)
            {
                option_scripts[i].anim.SetBool("selected",false);
            }
        }
        target_angle += angle;
        
        light_transform.gameObject.SetActive(false);
        selected_option_index++;
        
        if (selected_option_index >= option_scripts.Count)
        {
            selected_option_index = 0;
        }
        tutorialSceneScript.ReportAction(TutorialAction.ChangeCharacter);

    }
    public void enddrag()
    {
        
        dragging = false;
        
        target_angle = Mathf.Round(target_angle / angle) * angle;
        int steps = Mathf.RoundToInt(target_angle / angle);
        // Wrap so negative angles correctly pick previous options (e.g. -90 -> last index)
        selected_option_index = ((steps % option_scripts.Count) + option_scripts.Count) % option_scripts.Count-2;
        if(selected_option_index<0)
        {
            selected_option_index+= option_scripts.Count;
        } 
    }
    public  void confirm_choice(){
        if(!valid_selection)
        {
            return;
        }
        PlayerPrefs.SetInt("selected_class_index", option_scripts[selected_option_index].option_index);
        UnityEngine.SceneManagement.SceneManager.LoadScene(next_scene_name);
    }
    public IEnumerator lerp_stat_vals(carousel_option option)
    {
        float elapsed = 0f;
        float duration = 0.1f;
        float start_hp = HP_bar.fillAmount;
        float target_hp = option.class_data.max_health/18f;
        float start_hand = hand_bar.fillAmount;
        float target_hand = option.class_data.handsize/15f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            HP_bar.fillAmount = Mathf.SmoothStep(start_hp, target_hp, t);
            hand_bar.fillAmount = Mathf.SmoothStep(start_hand, target_hand, t);
            yield return null;
        }

    }
}
