using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/** 
@brief Health
*/
public class BurnoutBar : MonoBehaviour
{
    [SerializeField] private Image health_bar;
    
    [SerializeField] private  TMPro.TMP_Text hp_text;
    [SerializeField] private  TMPro.TMP_Text shield_text;
    [SerializeField] private  Transform shield_transform;
    [SerializeField]private Transform buff_disp;
    [HideInInspector] public List<mini_buff_icon> buff_icons = new List<mini_buff_icon>();
    [SerializeField] private bool force_attach=false;

    protected virtual void Start()
    {
        for(int i = 0 ;i<5;i++)//i really hope there are never more than 5 buffs
        {
            mini_buff_icon new_icon = Instantiate(Resources.Load<mini_buff_icon>("mini_buff_icon"), buff_disp);
            new_icon.gameObject.SetActive(false);
            buff_icons.Add(new_icon);
        }
        health_bar.type = Image.Type.Filled;
        health_bar.fillMethod = Image.FillMethod.Horizontal;
        if (force_attach)
        {
            Player_data playerData = Player_data.instance;
            playerData.burnout_Bar = this;

            playerData.shield = 0;
            playerData.burnout_Bar.set_hp(playerData.current_hp, playerData.max_hp, playerData.shield);
        }
    }
    /** 
    @brief Setter function for health
    */
    public void set_hp(int current, int max,int shield)
    {
        if (health_bar.fillAmount != (float)current / (float)max)
        {
            StartCoroutine(lerphp(health_bar.fillAmount, (float)current / (float)max, 0.5f * Mathf.Abs(health_bar.fillAmount - (float)current / (float)max)));
            
        }
        hp_text.text = current.ToString() + " / " + max.ToString();
        if (shield > 0)
        {
            shield_transform.gameObject.SetActive(true);
            shield_text.text = shield.ToString();
        }
        else
        {
            shield_transform.gameObject.SetActive(false);
        }
    }
    /** 
    @brief Lerp the health bars for smooth and aesthetic increase and decrease
    */
    public IEnumerator lerphp(float start, float end, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float current = Mathf.LerpUnclamped(start, end, elapsed / duration);
            health_bar.fillAmount = current;
            yield return null;
        }
        health_bar.fillAmount = end;
    }
    public IEnumerator lersp(float start, float end, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float current = Mathf.LerpUnclamped(start, end, elapsed / duration);
            yield return null;
        }
    }
    /** 
    @brief Handling buff icons
    */
    public void addbuff(buff buff_to_add)
    {
        for (int i = 0; i < buff_icons.Count; i++)
        {
            if (!buff_icons[i].gameObject.activeSelf)
            {
                buff_icons[i].gameObject.SetActive(true);
                buff_icons[i].setup_icon(buff_to_add);
                return;
            }
        }
        Debug.LogWarning("No space to add buff icon for buff " + buff_to_add.buff_name);
    }
    public void update_buff(buff buff_to_update)
    {
        mini_buff_icon icon_to_update = buff_icons.Find(x => x.gameObject.activeSelf && x.ref_buff.buff_name == buff_to_update.buff_name);
        if (icon_to_update != null)
        {
            icon_to_update.setup_icon(buff_to_update);
            
        }
        else
        {
            addbuff(buff_to_update);
            Debug.LogWarning("No buff icon found to update for buff " + buff_to_update.buff_name); ;
        }
    }
    public void removebuff(string buff_name)
    {
        for (int i = 0; i < buff_icons.Count; i++)
        {
            if (buff_icons[i].gameObject.activeSelf && buff_icons[i].ref_buff.buff_name == buff_name)
            {
                buff_icons[i].gameObject.SetActive(false);
                return;
            }
        }
        Debug.LogWarning("No buff icon found to remove for buff " + buff_name);
    }
}
