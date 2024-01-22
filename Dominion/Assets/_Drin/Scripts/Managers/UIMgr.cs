using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIMgr : MonoBehaviour
{    
    public RectTransform dashboard; 
    public GameObject doubleElixir, towersObj;
    public TextMeshProUGUI winText, towersText, doubleEText;
    private static UIMgr instance;
    public UICard reserve;
    public List<UICard> hand = new List<UICard>();
    public Image elixirSlider;
    public TextMeshProUGUI countDown, elixirAmount;
    public Canvas mainCanvas; 
    public Sprite[] winSprites;
    //public static Sprite blueWin;
    //public static Sprite redWin;
    public GameObject winImage;   
    public UICard lastSelected;     

    private void Awake() => instance = this;         

    public static void UpdateCards(List<CharacterData> newCards)
    {
        for (int i = 0; i < 4; i++)
            instance.hand[i].Setup(instance.dashboard, newCards[i], instance);
        instance.reserve.Setup(instance.dashboard, newCards[4], instance, true);
    }
    public static void DisplayElixir(float elixir, float maxElixir)
    {
        instance.elixirSlider.fillAmount = elixir / maxElixir;
        instance.elixirAmount.text = ((int)elixir).ToString();
    }
    public static void DisplayTime(float timeToDisplay)
    {
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        instance.countDown.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
    public static float CanvasScale()
    {
        return instance.mainCanvas.scaleFactor; 
    }

    private void ResetDE()
    {
        doubleElixir.SetActive(false);
    }
    public static void SetWinText(int pl, int towers)
    {
        string text = "Tie";
        if (pl == 0)
        {
            //text = GameStateMgr.instance.blueUsr + " Wins";
            text = "Player 1 Wins";
            instance.winText.color = new Color(0.1568627f, 0.3457898f, 0.7372549f, 0.7f);
            instance.winImage.GetComponent<Image>().sprite = instance.winSprites[0];
        }
        else if (pl == 1)
        {
            //text = GameStateMgr.instance.redUsr + " Wins";
            text = "Player 2 Wins";
            instance.winText.color = new Color(0.735849f, 0.1561944f, 0.3146843f, 0.7f);
            instance.winImage.GetComponent<Image>().sprite = instance.winSprites[1];
        }
        else if (pl == -1)
        {
            text = GameStateMgr.instance.blueUsr + " Tied " + GameStateMgr.instance.redUsr;
            instance.winText.color = new Color(0.1568627f, 0.7372549f, 0.3624311f, 0.7f);
        }
        instance.StartCoroutine(instance.SetText(text, 2f, towers));
        instance.winText.gameObject.SetActive(true);
    }
    public IEnumerator SetText(string text, float total, int towers)
    {
        string s = "";
        for (int i = 0; i < text.Length; i++)
        {
            s += text[i];
            winText.text = s;
            yield return new WaitForSecondsRealtime(total / text.Length);
        }
        instance.towersText.text = towers.ToString();
        instance.towersObj.SetActive(true);
    }
    public static void DoubleElixir()
    {
        instance.StartCoroutine(instance.SetEText(1f));
    }
    public IEnumerator SetEText(float total)
    {
        instance.doubleElixir.SetActive(true);
        string s = "", goal = "2x Elixir";
        for (int i = 0; i < goal.Length; i++)
        {
            s += goal[i];
            doubleEText.text = s;
            yield return new WaitForSecondsRealtime(total / goal.Length);
        }
        yield return new WaitForSecondsRealtime(1f);
        instance.doubleElixir.SetActive(false);
    }
}
