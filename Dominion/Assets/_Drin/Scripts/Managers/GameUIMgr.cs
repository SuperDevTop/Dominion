using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System;
using Photon.Realtime;

public class GameUIMgr : MonoBehaviourPunCallbacks
{
    public static GameUIMgr instance;
    public GameObject blueImage;
    public GameObject redImage;
    public GameObject blueImageEnd;
    public GameObject redImageEnd;
    public GameObject[] blueGasmarks;
    public GameObject[] redGasmarks;
    public GameObject[] blueGasmarksEnd;
    public GameObject[] redGasmarksEnd;
    public GameObject fightText;
    public Sprite blueGasmarkSprite;
    public Sprite redGasmarkSprite;  
    public Sprite blueBannerSprite;
    public Sprite redBannerSprite;      
    public Text blueText;
    public Text redText;
    public Text blueTextEnd;
    public Text redTextEnd;
    
    public void Start()
    {                
        instance = this;
        StartCoroutine(DelayToShowFightText());                                     
    } 

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator DelayToShowFightText()
    {
        blueImage.SetActive(true);
        redImage.SetActive(true);

        if(!MulMgr.instance.TESTMODE)
        {
            if(PhotonNetwork.IsMasterClient)
            {
                for(int i = 0; i < 3; i++)
                {
                    blueGasmarks[i].GetComponent<Image>().sprite = redGasmarkSprite;
                    redGasmarks[i].GetComponent<Image>().sprite = blueGasmarkSprite;
                    blueGasmarksEnd[i].GetComponent<Image>().sprite = redGasmarkSprite;
                    redGasmarksEnd[i].GetComponent<Image>().sprite = blueGasmarkSprite;
                }

                blueImage.GetComponent<Image>().sprite = redBannerSprite;
                redImage.GetComponent<Image>().sprite = blueBannerSprite;
                blueImageEnd.GetComponent<Image>().sprite = redBannerSprite;
                redImageEnd.GetComponent<Image>().sprite = blueBannerSprite;

                redText.text = PhotonMgr.player1Name;
                blueText.text = PhotonMgr.player2Name;
                redTextEnd.text = PhotonMgr.player1Name;
                blueTextEnd.text = PhotonMgr.player2Name;
            }
            else
            {
                redText.text = PhotonMgr.player2Name;
                blueText.text = PhotonMgr.player1Name;
                redTextEnd.text = PhotonMgr.player2Name;
                blueTextEnd.text = PhotonMgr.player1Name;
            }        
        }                 

        yield return new WaitForSeconds(1f);
        fightText.SetActive(true);
        yield return new WaitForSeconds(2.5f);
        fightText.SetActive(false);
    }
}
