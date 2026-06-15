using UnityEngine;
using UnityEngine.SceneManagement;
/** 
@brief Script for buttons in the player eval scene
*/
public class ExitButton : MonoBehaviour
{
    public void BackToStartButton()
    {
        Application.Quit();
    }

    public void CreditsButton()
    {
        SceneManager.LoadScene("Credits");
    }
}
