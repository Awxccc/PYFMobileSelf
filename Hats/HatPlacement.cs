using UnityEngine;
/** 
@brief Code to create a hat manager and use it to place hats
*/
public class HatPlacement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private hat[] hats;

    private void Start()
    {
        if (HatManager.Instance == null)
        {
            GameObject managerObj = new GameObject("HatManager");
            managerObj.AddComponent<HatManager>();
        }
    }

    public void PlaceHat(hat.hat_data hatToPlace)
    {
        if (hatToPlace == null || hatToPlace.hat_model == null) return;

        HatManager.Instance.PlaceHat(hatToPlace);
    }
}
