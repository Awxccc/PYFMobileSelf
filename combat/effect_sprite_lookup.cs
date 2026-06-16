using UnityEngine;

[CreateAssetMenu(fileName = "EffectSpriteLookup", menuName = "ScriptableObjects/EffectSpriteLookup", order = 1)]
public class effect_sprite_lookup : ScriptableObject
{
    public Sprite damage_effect_sprite;
    public Sprite block_effect_sprite;
    public Sprite heal_effect_sprite;
}
