using System;
using UnityEngine;
using UnityEngine.UI;

public class selection_btn : MonoBehaviour
{


    public scene_type btn_type;
    public int buttondir;//0-left,1-center,2-right
    public TMPro.TMP_Text description_text;

    public static Action<scene_type,int> btn_clicked_event;
    public void btn_clicked()
    {
        if(btn_type!=scene_type.Invalid)
        btn_clicked_event?.Invoke(btn_type,buttondir);
    }
}
    public enum scene_type
    {
        Encounter,
        Event,
        
        Treasure,
        Boss,
        Invalid
    }