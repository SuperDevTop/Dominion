using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class TowerDestroyedEvent
{
    public int pl;
    public int type; // 1 archer, 2 main

    public TowerDestroyedEvent(int pl, int type)
    {
        this.pl = pl;   
        this.type = type;
    }
} 

public class EndedEvent
{

}

[Serializable]
public class SingleCharData
{
    public string name;
    public string type;
    public int plays;
} 

public class GameStateMgr : MonoBehaviourPunCallbacks
{
    public GameObject confetti;
    public AudioSource win, lose, towerDest, normalMusic, doubleMusic;
    public static bool ended = false;
    public static GameStateMgr instance;
    public int score0 = 0, score1 = 0;
    public string blueUsr, redUsr;
    public List<SingleCharData> blueCharacters = new List<SingleCharData>(), redCharacters = new List<SingleCharData>();
    private void Awake()
    {
        instance = this;
        ended = false;
        score0 = score1 = 0;
    }
    void Start()
    {
        EventBus.Subscribe<TowerDestroyedEvent>(TDHandler);
        blueUsr = MulMgr.BlueUsername();
        redUsr = MulMgr.RedUsername();
    }
    public static void End()
    {
        instance.HandleEnd();
    }
    private bool played = false;
    private void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            blueUsr = "dada";
            redUsr = "playerB";
            redCharacters = blueCharacters;
            string jsonRes = JsonifyNewMatchProps();
            print("JSONRES:  \n" + jsonRes);
            NetworkMgr.NewMatchFunc(jsonRes);
        }
        
        
        if (played) return;
        if (DeckMgr.instance.elixirRateMultiplier > 1)
        {
            played = true;
            normalMusic.Stop();
            doubleMusic.Play();
        }
    }
    private void TDHandler(TowerDestroyedEvent e)
    {
        towerDest.Play();
        if (e.pl == 0)
            score1 += 1;
        else if (e.pl == 1)
            score0 += 1;

        if (e.type == 2) // Someone won
            HandleEnd(); 
    }
    public void AddPlay(SingleCharData character, bool isBlue)
    {
        List<SingleCharData> characterList = isBlue ? blueCharacters : redCharacters;
        SingleCharData existingCharacter = characterList.Find(c => c.name == character.name);
        if (existingCharacter != null)
        {
            existingCharacter.plays += 1;
        }
        else
        {
            character.plays = 1;
            characterList.Add(character);
        }
    }
    public string JsonifyNewMatchProps()
    {
        StringBuilder jsonBuilder = new StringBuilder();
        jsonBuilder.Append("{");
        jsonBuilder.Append("\"blueUsername\":\"" + blueUsr + "\",");
        jsonBuilder.Append("\"redUsername\":\"" + redUsr + "\",");
        jsonBuilder.Append("\"blueScore\":" + score0 + ",");
        jsonBuilder.Append("\"redScore\":" + score1 + ",");
        jsonBuilder.Append("\"blueCharactersUsed\":[");

        for (int i = 0; i < blueCharacters.Count; i++)
        {
            SingleCharData character = blueCharacters[i];
            jsonBuilder.Append("{");
            jsonBuilder.Append("\"name\":\"" + character.name + "\",");
            jsonBuilder.Append("\"type\":\"" + character.type + "\",");
            jsonBuilder.Append("\"plays\":" + character.plays);
            jsonBuilder.Append("}");

            if (i < blueCharacters.Count - 1)
            {
                jsonBuilder.Append(",");
            }
        }

        jsonBuilder.Append("],");
        jsonBuilder.Append("\"redCharactersUsed\":[");

        for (int i = 0; i < redCharacters.Count; i++)
        {
            SingleCharData character = redCharacters[i];
            jsonBuilder.Append("{");
            jsonBuilder.Append("\"name\":\"" + character.name + "\",");
            jsonBuilder.Append("\"type\":\"" + character.type + "\",");
            jsonBuilder.Append("\"plays\":" + character.plays);
            jsonBuilder.Append("}");

            if (i < redCharacters.Count - 1)
            {
                jsonBuilder.Append(",");
            }
        }

        jsonBuilder.Append("]}");

        return jsonBuilder.ToString();
    }
    public void HandleEnd()
    {
        int yourScore = MulMgr.GetPl() == 0 ? score0 : score1;
        int otherScore = MulMgr.GetPl() == 0 ? score1 : score0;
        ended = true;
        int state = yourScore > otherScore ? 0 : yourScore < otherScore ? 1 : -1;

        doubleMusic.Stop();
        confetti.SetActive(true);

        if (state == 1) lose.Play();
        else win.Play();
        StartCoroutine(Ender(yourScore));    
    }
    private IEnumerator Ender(int urScore)
    {
        if(!PhotonNetwork.IsMasterClient)
        {
            for(int i = 0; i < 3 - score0; i++)
            {
                GameUIMgr.instance.redGasmarksEnd[i].SetActive(true);
            }

            for(int i = 0; i < 3 - score1; i++)
            {
                GameUIMgr.instance.blueGasmarksEnd[i].SetActive(true);
            }
        }
        else
        {
            for(int i = 0; i < 3 - score0; i++)
            {
                GameUIMgr.instance.blueGasmarksEnd[i].SetActive(true);
            }

            for(int i = 0; i < 3 - score1; i++)
            {
                GameUIMgr.instance.redGasmarksEnd[i].SetActive(true);
            }
        }
        
        for (float i = 1f; i >= 0f; i -= Time.unscaledDeltaTime * 0.2f)
        {
            Time.timeScale = i;
            yield return null;
        }

        Time.timeScale = 0.05f;        

        UIMgr.SetWinText(score0 > score1 ? 0 : score1 > score0 ? 1 : -1, urScore);
        EventBus.Publish(new EndedEvent());
        string jsonRes = JsonifyNewMatchProps();
        print("JSONRES:  \n" + jsonRes);
        NetworkMgr.NewMatchFunc(jsonRes);
        yield return new WaitForSecondsRealtime(4f);
        /*
        int res = PlayerPrefs.GetInt("SCORE");
        PlayerPrefs.SetInt("SCORE", res + urScore);
        */

        Time.timeScale = 1f;
        PhotonNetwork.LeaveRoom();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
    }

    public override void OnLeftRoom()
    {
        StartCoroutine(WaitAndDisconnect());
    }

    private IEnumerator WaitAndDisconnect()
    {
        yield return new WaitForSeconds(1f);  // Wait for 1 second
        PhotonNetwork.Disconnect();
    }

}
