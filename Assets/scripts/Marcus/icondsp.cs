using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class icondsp : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Text icon_text;
    [SerializeField] private Image sprite;
    public void setup_icon(effect_disc data_to_use)
    {
        sprite.sprite = data_to_use.effect_icon;
        icon_text.text = data_to_use.effect_string;
    }

}
