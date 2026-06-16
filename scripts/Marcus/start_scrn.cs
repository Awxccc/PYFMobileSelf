using UnityEngine;
using UnityEngine.SceneManagement;

public class start_scrn : MonoBehaviour
{
    [SerializeField] Animator anim;
    [SerializeField] string next_scene_name;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                play_game();
            }
        }
        else if (Input.GetMouseButtonDown(0))
        {
            play_game();
        }
    }
    public void play_game()
    {
        anim.SetTrigger("start");
    }
    public void next_scene()
    {
        SceneManager.LoadScene(next_scene_name);
    }
}
