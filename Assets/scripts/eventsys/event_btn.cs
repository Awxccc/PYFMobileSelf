using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class event_btn : MonoBehaviour
{
    public Event owner;
    public TMP_Text btn_titleText;
    public TMP_Text btn_descText;
    public event_option ref_option;
    public Animation anim;
    //private int skillCheckTarget;

    [SerializeField] private Image selectedImage;
    [SerializeField] private TextMeshProUGUI optionDesc;
    [SerializeField] private GameObject hatDisplay;
    [SerializeField] private Image hatIcon;
    [SerializeField] private TextMeshProUGUI hatCost;
    [SerializeField] private TextMeshProUGUI hatEffect;
    private Sprite hatIconSprite;
    [SerializeField] private GameObject hatDisplay2;
    [SerializeField] private Image hatIcon2;
    [SerializeField] private TextMeshProUGUI hatCost2;
    [SerializeField] private TextMeshProUGUI hatEffect2;
    private Sprite hatIconSprite2;

    private void Awake()
    {
        selectedImage.enabled = false;
        optionDesc.gameObject.SetActive(false);
        hatDisplay.SetActive(false);
        hatDisplay2.SetActive(false);
    }

    private void Start()
    {
        if (ref_option.hatToGive.Length == 1)
        {
            hatIcon.sprite = ref_option.hatToGive[0].hat_info.icon;
            hatEffect.text = ref_option.hatToGive[0].hat_info.get_detailed_description();
        }
        else if (ref_option.hatToGive.Length > 1)
        {
            hatIcon.sprite = ref_option.hatToGive[0].hat_info.icon;
            hatEffect.text = ref_option.hatToGive[0].hat_info.get_detailed_description();
            hatIcon2.sprite = ref_option.hatToGive[1].hat_info.icon;
            hatEffect2.text = ref_option.hatToGive[1].hat_info.get_detailed_description();
        }
    }

    private void Update()
    {
        int target = ref_option.finalSkillCheckTarget;
    }

    public void btnpressed()
    {
        owner.select_option(this);
    }

    public void setup_button(event_option option, Event parent_event)
    {
        ref_option = option;
        btn_titleText.text = option.option_title;

        // Only show target if there's a skill check
        if (option.hasSkillCheck)
        {
            int displayTarget = option.finalSkillCheckTarget;
            float successChance = 100f - ((displayTarget / 10f) * 100f);
            string color = option.skillCheckType switch
            {
                Event.SkillCheckType.Critical => "red",
                Event.SkillCheckType.Social => "purple",
                Event.SkillCheckType.Transferable => "green",
                _ => "white"
            };

            btn_descText.text = $"{ref_option.option_description}<color={color}> ({successChance:0}%)</color>";
        }
        else
        {
            // Don't show target for non-skill check options
            btn_descText.text = ref_option.option_description;
        }

        owner = parent_event;
    }

    public void SetSelected(bool selected)
    {
        selectedImage.enabled = selected;
        optionDesc.gameObject.SetActive(selected);
        if (ref_option.hatToGive.Length == 1)
        {
            hatDisplay.SetActive(selected);
        }
        else if (ref_option.hatToGive.Length > 1)
        {
            hatDisplay.SetActive(selected);
            hatDisplay2.SetActive(selected);
        }
    }
}
