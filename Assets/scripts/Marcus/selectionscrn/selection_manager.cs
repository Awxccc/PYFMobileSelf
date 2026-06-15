using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class selection_manager : MonoBehaviour
{ 
    public event_data selected_choice;//stores the scene selected to load
    public Animation camera_anim;//for transition effects
    public bool options_overide=false;//for debug purposes
    public event_data forced_option;//for debug purposes
    private List<selection_btn> selection_buttons = new List<selection_btn>();
    void Start()
    {
        selection_btn.btn_clicked_event += parse_choice;
        Player_data.load_next_scene += proceed_to_selected_scene;


        load_choices();

    }
    public void parse_choice(scene_type type,int buttondir)
    {
        if(type==scene_type.Invalid)
        {
            Debug.Log("Invalid selection made, ignoring");
            return;
        }
        UnityEngine.Random.InitState((int)DateTime.Now.Millisecond+System.Environment.TickCount+System.Diagnostics.Process.GetCurrentProcess().Id);
        
        int roll = UnityEngine.Random.Range(0, 100);

        //for boss/treasure chance calculation
        int boss_chance= math.max(0,(Player_data.instance.current_boss_progress - Player_data.instance.boss_min_progress)*15);
        int treasure_chance= Player_data.instance.treasure_base_chance + Player_data.instance.current_treasure_progress * 2;
        if(roll<boss_chance)//trigger boss encounter
        {
            type = scene_type.Boss;
            Debug.Log("Boss chance triggered with roll "+roll+" under "+boss_chance);
            Player_data.instance.current_treasure_progress++;//if not treasure, increase progress towards treasure
        }
        else if(roll<treasure_chance)//trigger treasure encounter
        {
             type = scene_type.Treasure;
            Player_data.instance.current_boss_progress++;//if not boss, increase progress towards boss
            Debug.Log("Treasure chance triggered with roll "+roll+" under "+treasure_chance);
        }
        else
        {
            Player_data.instance.current_boss_progress++;//if not boss, increase progress towards boss
            Player_data.instance.current_treasure_progress++;//if not treasure, increase progress towards treasure
        }
        switch(type)
        {
            case scene_type.Encounter:
                Player_data.instance.encounters_chosen++;//if not boss, increase progress towards boss
                selected_choice = Player_data.instance.get_valid_encounter();
                break;

            case scene_type.Event:
                Player_data.instance.events_chosen++;//if not boss, increase progress towards boss
                selected_choice = Player_data.instance.get_valid_event();
                break;

            case scene_type.Treasure:
                selected_choice = Player_data.instance.get_valid_treasure();//placeholder
                break;

            case scene_type.Boss:
                Player_data.instance.current_boss_progress = 1;//reset boss chance on boss encounter
                selected_choice = Player_data.instance.boss_list.First();
                Player_data.instance.boss_list.RemoveAt(0);
                break;
            default:
                Debug.LogError("Invalid selection type");
                break;
        }

        camera_anim.Play( );
        Player_data.instance.anim.SetTrigger("out");//IMPORTANT for future reference!: this actives an animation event which calls playerdata.load_next_scene() which triggers this script to load the selected scene
        Player_data.instance.progress_log.Add(new progress_data { scene_name = Player_data.instance.current_scene.scene_name, selection_made = type, path_taken = buttondir, scene_description = Player_data.instance.current_scene.scene_description });//logs the current scene before moving on
    }
    public void block_path()
    {
        //

        if (selection_buttons.Count >= 3)
        {
            selection_buttons[1].btn_type = scene_type.Invalid;
            selection_buttons[1].description_text.text =  "<sprite=18>";//locked icon
        }
        else
        {
            
        List<int> used_dirs = new List<int>();
        foreach(var btn in selection_buttons)
        {
            used_dirs.Add(btn.buttondir);
        }
        UnityEngine.Random.InitState((int)DateTime.Now.Millisecond+System.Environment.TickCount+System.Diagnostics.Process.GetCurrentProcess().Id);
        int dir=UnityEngine.Random.Range(0,3);//0-left,1-center,2-right
        while(used_dirs.Contains(dir))
        {
            dir = UnityEngine.Random.Range(0,3);//this should not loop infinitely since max choices is 3
        }
        
        selection_btn temp = Instantiate(Resources.Load<selection_btn>("option_button"),this.transform);
        temp.buttondir = dir;//0-left,1-center,2-right//need to change this later
        temp.btn_type = scene_type.Invalid;
        temp.description_text.text =  "<sprite=18>";
        temp.transform.localPosition = new Vector3((1 - dir)*-65.0f,0,0);//spacing out buttons
        selection_buttons.Add(temp);
        }

    }
    public void load_choices()
    {
        UnityEngine.Random.InitState((int)DateTime.Now.Millisecond+System.Environment.TickCount+System.Diagnostics.Process.GetCurrentProcess().Id);
        int choice_amt = UnityEngine.Random.Range(1, 4);//1 or 3 choices
        if(Player_data.instance.current_boss_progress - Player_data.instance.boss_min_progress>0)//after 5 scenes have passed, increase chance of boss encounter
        {
            choice_amt= math.min(3,choice_amt);
        }
        List<int> used_dirs = new List<int>();
        for(int i=0;i<choice_amt;i++)
        {
            
            selection_btn temp = Instantiate(Resources.Load<selection_btn>("option_button"),this.transform);
            if(i==0&&Player_data.instance.current_boss_progress - Player_data.instance.boss_min_progress>0)
            {
                //first choice is always boss on leftmost button
                temp.buttondir = 0;//0-left,1-center,2-right//need to change this later
                temp.description_text.text = "<sprite=2>";//boss icon
                
                temp.btn_type = scene_type.Boss;
                temp.transform.localPosition = new Vector3(-65.0f,0,0);//spacing out buttons
                selection_buttons.Add(temp);
                
                used_dirs.Add(0);
                continue;
            }
            int dir=UnityEngine.Random.Range(0,3);//0-left,1-center,2-right
            while(used_dirs.Contains(dir))
            {
                dir = UnityEngine.Random.Range(0,3);//this should not loop infinitely since max choices is 3
            }
            used_dirs.Add(dir);
            int type_roll=UnityEngine.Random.Range(0,100);
            if (type_roll < 40)
            {
                temp.btn_type = scene_type.Encounter;
                temp.description_text.text = "Challenge\n<sprite=11>";
            }
            else if (type_roll < 80)
            {
                temp.btn_type = scene_type.Event;
                temp.description_text.text = "Venture\n<sprite=12>";
            }
            else
            {
                temp.btn_type = scene_type.Treasure;
                temp.description_text.text = "UpSkill\n<sprite=14>";
            }
            //encounter rules at 5+ scenes garantees boss on leftmost button and always have +1 choices max 3 , treasure is base 15% chance for option

            temp.buttondir = dir;//0-left,1-center,2-right//need to change this later
            temp.transform.localPosition = new Vector3((1 - dir)*-65.0f,0,0);//spacing out buttons
            selection_buttons.Add(temp);

        }
    }
    public event_data getrandom_encounter_or_event()
    {
        int chance = UnityEngine.Random.Range(0, 100);
        if (chance < 50)
        {
            return Player_data.instance.get_valid_encounter();
        }
        else
        {
            return Player_data.instance.get_valid_event();
        }
    }
    public void proceed_to_selected_scene()
    {
        if(options_overide)
        {
            selected_choice=forced_option;
        }
        if(selected_choice==null)
        {
            if (Player_data.instance.boss_list.Count > 0)
            {

                selected_choice = Player_data.instance.boss_list.First();
            }
            else
            {
                selected_choice = Player_data.instance.get_valid_encounter();
            }
        }
        StartCoroutine(loadscene());
    }
    public IEnumerator loadscene()
    {
        Player_data.instance.current_scene=selected_choice;
        Scene temp = SceneManager.GetActiveScene();
        AsyncOperation asyncLoad;
        asyncLoad=SceneManager.LoadSceneAsync("combat_blockout", LoadSceneMode.Additive);//loads this a generic scene which will then load additional data
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        SceneManager.UnloadSceneAsync(temp);

    }    
    public IEnumerator loadscene(string scene_name)
    {
        Player_data.instance.current_scene=selected_choice;
        Scene temp = SceneManager.GetActiveScene();
        AsyncOperation asyncLoad;
        asyncLoad=SceneManager.LoadSceneAsync(scene_name, LoadSceneMode.Additive);//loads this a generic scene which will then load additional data
        Debug.Log("Boss scene go away");
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        Debug.Log("Boss scene went away");
        SceneManager.UnloadSceneAsync(temp);

    }
    public void proceed_to_selected_scene(string scene_name)
    {
        if(scene_name == null) StartCoroutine(loadscene());
        else StartCoroutine(loadscene(scene_name));
    }
    public void OnDestroy()
    {
        
        selection_btn.btn_clicked_event -= parse_choice;
        Player_data.load_next_scene -= proceed_to_selected_scene;
    }
}
