using UnityEngine;

/** 
@brief Simple quit code
*/
public class TapAnywhereToChangeScene : MonoBehaviour
{

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Application.Quit();
        }
    }
}
