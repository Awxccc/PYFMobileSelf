using UnityEngine;
/** 
@brief Script to teleport the player instance to the peak of the mountain in the player type evaluation scene
*/
public class TPPlayerToPeak : MonoBehaviour
{
    [SerializeField] private Transform peak;

    private void Start()
    {
        Player_data.instance.transform.position = peak.position;
        Player_data.instance.transform.rotation = peak.rotation;
        Debug.Log(Player_data.instance.transform.position);
        Debug.Log(peak.position);
    }
}
