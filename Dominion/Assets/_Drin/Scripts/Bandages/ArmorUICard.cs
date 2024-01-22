using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;

public class ArmorUICard : MonoBehaviour
{
    [SerializeField]
    public Armor armor;
    public Text armorName, armorDetails;
    public GameObject lck;
    public bool locked;
    public GameObject armorBackground;
    public void Setup(Armor armor)
    {
        this.armor = armor;
        armorName.text = armor.armorName;
        lck.SetActive(armor.locked);
        locked = armor.locked;

        string txt = 
            $"Health " +
            $"+{(armor.health) * 100:F0}%\n" +
            $"Speed " +
            $"+{(armor.moveSpeed) * 100:F0}%\n" +
            $"Damage " +
            $"+{(armor.dmg) * 100:F0}%\n" +
            $"Shoot Rate " +
            $"+{(armor.shootRate) * 100:F0}%\n" +
            $"Range " +
            $"+{(armor.range) * 100:F0}%";
        armorDetails.text = txt;

        armorBackground.GetComponent<Image>().sprite = armor.armorImage;
    }
    public void OnEquip()
    {
        if (locked) return;
        MenuDeckMgr.ArmorHandle(this);
    }
}
