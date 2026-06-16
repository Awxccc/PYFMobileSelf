using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "passiveItem", menuName = "Scriptable Objects/passiveItem")]
public class passiveItem : ScriptableObject
{
    public passiveItemData passiveItemInfo;

    [System.Serializable]
    public class passiveItemData 
    {

        public Sprite icon;
        public string name;
        public item_type type;

        [SerializeField] public string description;
        public List<effects> use_effects;

        public int atk_mod;
    }

}
public enum item_type
{
    Transferable, //Affects player damage
    Social,
    Critical
}
