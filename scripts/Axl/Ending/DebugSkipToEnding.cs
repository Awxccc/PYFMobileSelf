using UnityEngine;
using UnityEngine.SceneManagement;
/** 
@brief Debugging script to skip to ending
*/
public class DebugSkipToEnding : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            SceneManager.LoadScene("MountainEnding");
        }
    }
}
