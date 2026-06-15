using UnityEngine;
using UnityEngine.SceneManagement;

public class game_over_manager : MonoBehaviour
{
    public TMPro.TMP_Text remaining_retries_text;
    public selection_manager selection_mgr;
    public Transform ui_transform;
    public Light environment_light;
    public Light spot_light;
    public combat_manager combat_manager_ref;
    public void Start()
    {
        Player_data.Player_died += activate_game_over;
        Player_data.done_with_die += enable_ui;
        ui_transform.gameObject.SetActive(false);
    }
    public void OnDestroy()
    {
        Player_data.Player_died -= activate_game_over;
        Player_data.done_with_die -= enable_ui;
    }
    public void retry_scene()
    {
        if (Player_data.instance.remaining_retries > 0)
        {
            Player_data.instance.remaining_retries -= 1;
            Player_data.instance.reset_player();

            Player_data.instance.reset_anim();
            Player_data.instance.anim.Play("test_trans_in");
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }
    }
    public void back_to_scene_selection()
    {
        // Player_data.load_next_scene?.Invoke();

        environment_light.intensity = 1f;
        spot_light.gameObject.SetActive(false);
        Debug.Log("Going back to scene selection");
        Player_data.instance.reset_player();
        Player_data.instance.reset_anim();
        Player_data.instance.anim.SetTrigger("mid");
        selection_mgr.gameObject.SetActive(true);
        selection_mgr.block_path();
        ui_transform.gameObject.SetActive(false);
        combat_manager_ref.end_combat();
    }
    public void quit_game()
    {
        Destroy(Player_data.instance.gameObject);
        SceneManager.LoadScene("start");

        //Application.Quit();//for now quits out but probably will go back to main menu later
    }
    public void enable_ui()
    {
        ui_transform.gameObject.SetActive(true);
    }
    public void activate_game_over()
    {
        environment_light.intensity = 0.12f;
        spot_light.gameObject.SetActive(true);
        remaining_retries_text.text = Player_data.instance.remaining_retries.ToString() + "x";
    }

}
