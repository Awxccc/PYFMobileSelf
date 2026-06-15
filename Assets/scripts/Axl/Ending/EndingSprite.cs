using UnityEngine;
/** 
@brief Determines which sprite to use in the ending scene according to which character you picked
*/
public class EndingSprite : MonoBehaviour
{
    [SerializeField] private Sprite sdmSprite;
    [SerializeField] private Sprite sitSprite;
    [SerializeField] private Sprite segSprite;
    [SerializeField] private Sprite shssSprite;
    [SerializeField] private Sprite sasSprite;
    [SerializeField] private Sprite sbmSprite;

    [SerializeField] private SpriteRenderer player;

    private void Start()
    {
        player = GetComponent<SpriteRenderer>();

        character_class_data playerClass = Player_data.instance.ClassData;

        switch (playerClass.name)
        {
            case "sdm":
                player.sprite = sdmSprite;
                break;
            case "sit":
                player.sprite = sitSprite;
                break;
            case "seg":
                player.sprite = segSprite;
                break;
            case "shss":
                player.sprite = shssSprite;
                break;
            case "sas":
                player.sprite = sasSprite;
                break;
            case "sbm":
                player.sprite = sbmSprite;
                break;
        }
    }
}
