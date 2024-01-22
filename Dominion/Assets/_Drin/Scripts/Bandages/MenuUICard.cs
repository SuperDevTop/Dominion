using UnityEngine;
using UnityEngine.UI;

public class MenuUICard : MonoBehaviour
{
    [SerializeField]
    public Card card;
    public Image image;
    public Text elixir, cardName;
    public Slider power;
    public GameObject lck;
    public Text actionText;       

    public void Setup(Card card, bool inDeck = false)
    {
        this.card = card;
        image.sprite = card.character.sprite;
        elixir.text = card.character.cost.ToString();
        cardName.text = card.character.characterName;
        lck.SetActive(card.locked);
        // Equip = +, Unequip = -
        actionText.text = inDeck ? "-" : "+";
        power.value = card.power / 250;  
    }
    public void OnEquip()
    {
        actionText.text = MenuDeckMgr.Equip(this);
    }
    public void OnSynth()
    {        
        MenuDeckMgr.Synch(this);
    }
}
