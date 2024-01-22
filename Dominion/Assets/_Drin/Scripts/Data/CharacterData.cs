using UnityEngine;

[CreateAssetMenu(fileName = "New CharacterData", menuName = "CharacterData")]
public class CharacterData : ScriptableObject
{
    public enum AttackType
    {
        ground,
        air,
        both
    }

    public string characterName;
    public string characterType;
    public GameObject model, visualModel;
    public Sprite sprite;
    public Sprite fullbodySprite;
    public AttackType attackType;
    public int cost;
    public float moveSpeed, dmg, shootRate, health, visionRange, attackRange;
}