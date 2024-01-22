using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static NetworkMgr;
using static UnityEngine.UI.Button;

[Serializable]
public class Deck
{
    public List<Card> cards;
}
[Serializable]
public class Card
{
    public CharacterData character;
    public MyCharacterData myData;
    public bool locked;
    public List<Armor> equippedArmor;
    public float power;    

    public override string ToString()
    {
        return character.characterName + " " + power;
    }
}

[Serializable]
public class Armor
{
    public string armorName;
    public int stars;
    public float moveSpeed, dmg, shootRate, health, range;
    public Sprite armorImage;
    public bool locked;
}

[Serializable]
public class StatsAmount
{
    public Image health, defence, range, damage, speed;
}

public class MenuDeckMgr : MonoBehaviour
{
    public static MenuDeckMgr instance;
    public GameObject selectGrid, deckGrid, cardPrefab, armorPrefab, armorGrid, selectedArmorGrid;
    public Text avgElixir;
    [SerializeField]
    public List<Card> availableCards;
    [SerializeField]
    public List<Armor> availableArmor;

    public ButtonClickedEvent onSynch;

    public Deck deck;
    public Card synchedCard;
    public StatsAmount statsFill; 

    public Text synchedName, stats;

    public Image synchImg;   

    public GameObject battleDecBack;
    public GameObject armorBack; 

    private void Awake()
    {
        instance = this;
    }
    public string AvgElixir()
    {
        float e = 0;
        foreach (Card c in deck.cards)
        {
            e += c.character.cost;
        }
        return deck.cards.Count == 0 ? "0.0" : (e / deck.cards.Count).ToString("F1");
    }
    public static void UpdateHumans(List<MyCharacterData> list)
    {
        foreach (MyCharacterData c in list)
        {
            Card crd = instance.availableCards.Find(car => car.character.characterName == c.name);
            if (crd != null)
                crd.power = c.plays;
            Card crd2 = instance.deck.cards.Find(car => car.character.characterName == c.name);
            if (crd2 != null)
                crd2.power = c.plays;
        }

        PlayerPrefs.SetString("Deck_Cards", JsonUtility.ToJson(instance.deck));
    }
    public static void ArmorHandle(ArmorUICard armorCard)
    {
        int i = instance.synchedCard.equippedArmor.FindIndex(a => a.armorName == armorCard.armor.armorName);
        if (i != -1)
        {
            instance.synchedCard.equippedArmor.RemoveAt(i);
            armorCard.transform.SetParent(instance.armorGrid.transform, false);
        }
        else
        {
            instance.synchedCard.equippedArmor.Add(armorCard.armor);
            armorCard.transform.SetParent(instance.selectedArmorGrid.transform, false);
        }
        Card matchingCard = instance.deck.cards.Find(card => card.character.characterName == instance.synchedCard.character.characterName);
        if (matchingCard != null)
        {
            matchingCard.equippedArmor = instance.synchedCard.equippedArmor;
        }
        PlayerPrefs.SetString("Deck_Cards", JsonUtility.ToJson(instance.deck));

        instance.UpdateStats();
    }
    public void UpdateStats()
    {
        // Get character data properties
        float moveSpeed = synchedCard.character.moveSpeed;
        float dmg = synchedCard.character.dmg;
        float shootRate = synchedCard.character.shootRate;
        float health = synchedCard.character.health;
        float range = synchedCard.character.visionRange;

        // Calculate total multipliers from equipped armor : Stats text
        float armorMultiplierSpeed = 1.0f;
        float armorMultiplierDmg = 1.0f;
        float armorMultiplierShootRate = 1.0f;
        float armorMultiplierHealth = 1.0f;
        float armorMultiplierRange = 1.0f;

        foreach (Armor armor in synchedCard.equippedArmor)
        {
            armorMultiplierSpeed += armor.moveSpeed;
            armorMultiplierDmg += armor.dmg;
            armorMultiplierShootRate += armor.shootRate;
            armorMultiplierHealth += armor.health;
            armorMultiplierRange += armor.range;
        }

        string txt = "Stats:\n" +
            $"Health - {health * armorMultiplierHealth:0} " +
            $"(+{(armorMultiplierHealth - 1.0f) * 100:F0}%)\n" +
            $"Speed - {moveSpeed * armorMultiplierSpeed:0} " +
            $"(+{(armorMultiplierSpeed - 1.0f) * 100:F0}%)\n" +
            $"Damage - {dmg * armorMultiplierDmg:0} " +
            $"(+{(armorMultiplierDmg - 1.0f) * 100:F0}%)\n" +
            $"Shoot Rate - {shootRate * armorMultiplierShootRate:0} " +
            $"(+{(armorMultiplierShootRate - 1.0f) * 100:F0}%)\n" +
            $"Range - {range * armorMultiplierRange:0} " +
            $"(+{(armorMultiplierRange - 1.0f) * 100:F0}%)";

        stats.text = txt;

        // Stats image
        foreach (Armor armor in synchedCard.equippedArmor)
        {
            health += armor.health;
            dmg += armor.dmg;
            moveSpeed += armor.moveSpeed;
            range += armor.range;
            shootRate += armor.shootRate;
        }  

        statsFill.speed.GetComponent<Image>().fillAmount = moveSpeed / 3.0f;
        statsFill.health.GetComponent<Image>().fillAmount = health / 3000f;
        statsFill.damage.GetComponent<Image>().fillAmount = dmg / 300f;
        statsFill.range.GetComponent<Image>().fillAmount = range / 6f;
        statsFill.defence.GetComponent<Image>().fillAmount = shootRate / 3f;                   
    }
    public static string Equip(MenuUICard card)
    {
        //Equip
        string ret = "+";
        int i = instance.deck.cards.FindIndex(c => c.character.characterName == card.card.character.characterName);
        if (i != -1)
        {
            instance.deck.cards.Remove(card.card);
            card.transform.parent  = instance.selectGrid.transform;
            card.transform.SetAsFirstSibling();
            // remove it from deckGrid's children
            // add it to selectGrid's children
        }
        else
        {
            instance.deck.cards.Add(card.card);
            card.transform.parent  = instance.deckGrid.transform;
            // Unequip
            ret = "-";
            // add it from deckGrid's children
            // remove it to selectGrid's children
        }

        instance.avgElixir.text = instance.AvgElixir();

        PlayerPrefs.SetString("Deck_Cards", JsonUtility.ToJson(instance.deck));
        return ret;
    }
    public static void Synch(MenuUICard card)
    {        
        // Clear the selectedArmorGrid and armorGrid
        foreach (Transform child in instance.selectedArmorGrid.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in instance.armorGrid.transform)
        {
            Destroy(child.gameObject);
        }

        // Set the new synchedCard
        instance.synchedCard = card.card;
        instance.synchedName.text = card.card.character.name;
        instance.synchImg.sprite = instance.synchedCard.character.fullbodySprite;

        instance.UpdateStats();

        // Add the appropriate armor cards back into the selectedArmorGrid and armorGrid
        foreach (Armor armor in instance.availableArmor)
        {
            GameObject newArmor = Instantiate(instance.armorPrefab);
            newArmor.GetComponent<ArmorUICard>().Setup(armor);

            if (instance.synchedCard.equippedArmor.Any(a => a.armorName == armor.armorName))
            {
                newArmor.transform.SetParent(instance.selectedArmorGrid.transform, false);
            }
            else
            {
                newArmor.transform.SetParent(instance.armorGrid.transform, false);
            }
        }

        // Invoke the onSynch event
        if (instance.firstTime)
        {
            instance.firstTime = false;
            return;
        }
        instance.onSynch.Invoke();

        // Update Design 
        instance.battleDecBack.SetActive(false);
        instance.armorBack.SetActive(true);
    }
    bool firstTime = true;

    void Start() 
    {
        var deckPP = PlayerPrefs.GetString("Deck_Cards");

        if (deckPP == null || deckPP.Length == 0)
        {
            deck = new Deck();
            deck.cards = new List<Card>();
        }
        else {
            print(deckPP);
            deck = JsonUtility.FromJson<Deck>(deckPP);
        }

        MenuUICard firstCrd = null;
        foreach (Card card in availableCards)
        {
            if (deck.cards.Any(c => c.character == card.character)) continue;
            MenuUICard newCard = Instantiate(cardPrefab).GetComponent<MenuUICard>();
            newCard.transform.SetParent(selectGrid.transform, false);
            newCard.Setup(card);
            if (firstCrd == null && !card.locked) firstCrd = newCard;
        }
        foreach (Card card in deck.cards) { 
            MenuUICard newCard = Instantiate(cardPrefab).GetComponent<MenuUICard>();
            newCard.transform.SetParent(deckGrid.transform, false);
            newCard.Setup(card, true);
            if (firstCrd == null && !card.locked) firstCrd = newCard;
        }
        instance.avgElixir.text = instance.AvgElixir();
        Synch(firstCrd);
    }
    public static bool CanBattle()
    {
        return instance.deck.cards.Count == 8;
    }
}
