using System.Net.Mail;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UI.Dates;
using UnityEngine.Networking;
using UnityEngine.Video;
using UnityEngine.EventSystems;
using static NetworkMgr;

public class MenuMgr : MonoBehaviour
{
    //public GameObject mainUI;
    public static MenuMgr Instance { get; private set; }
    public Text headerHumans, headerGems, headerGold;
    public Text profileHumans, saviorHumans, captorHumans;
    public Text user;
    public GameObject registerObj, loginObj;
    public Text debugger;
    public DatePicker dob;
    public GameObject loader;
    [SerializeField] private VideoPlayer[] videoPlayer;

    public Text registerUsername, registerEmail, registerPassword, registerConfirmPassword, loginUsername, loginPassword;
    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        loader.SetActive(false);
        headerHumans.text = headerGems.text = headerGold.text = profileHumans.text = saviorHumans.text = captorHumans.text = "0";
        profileHumans.text += " Humans";

        //mainUI.transform.localScale = new Vector3(Screen.width / 768f, Screen.height / 1366f, 1f);

        if (NetworkMgr.IsAuthed())
        {
            UpdateUsername(NetworkMgr.GetUsername());
            NetworkMgr.GetLatestFunc(this);
            registerObj.SetActive(false);
            loginObj.SetActive(false);
        }
        else
        {
            registerObj.SetActive(true);
            loginObj.SetActive(false);
        }
        debugger.text = "";
        return;
        if (PlayerPrefs.HasKey("SCORE"))
        {
            string amnt = (PlayerPrefs.GetInt("SCORE") * 100).ToString();
            headerHumans.text = profileHumans.text = saviorHumans.text = captorHumans.text = amnt;
            profileHumans.text += " Humans";
        }        
    }
    public void DeleteAccount()
    {
        NetworkMgr.DeleteAccountFunc(this);
    }
    public static void Load()
    {
        Instance.loader.SetActive(true);
    }
    public static void UnLoad()
    {
        Instance.loader.SetActive(false);
    }
    public void DebugText(string text)
    {
        debugger.text = text;
        Invoke("ResetDebugger", 5f);
    }
    public void ResetDebugger()
    {
        debugger.text = "";
    }
    public void UpdateUsername(string username)
    {
        user.text = username;
    }
    public void SuccessfulAuth(string username)
    {
        UpdateUsername(username);
        registerObj.SetActive(false);
        loginObj.SetActive(false);
        NetworkMgr.GetLatestFunc(this);
    }
    public void Register()
    {
        if (!registerPassword.text.Equals(registerConfirmPassword.text))
        {
            DebugText("Password mismatch");
            return;
        }
        else if (registerPassword.text.Length < 8)
        {
            DebugText("Password needs to be at least 8 characters");
            return;
        }
        else if (registerUsername.text.Length < 3 || registerUsername.text.Length  > 20)
        {
            DebugText("Username should be between 4 and 20 characters");
            return;
        }
        try
        {
            if (registerEmail.text.Length == 0)
            {
                DebugText("Invalid email");
                return;
            }
            MailAddress m = new MailAddress(registerEmail.text);
            if (!dob.SelectedDate.HasValue || dob.SelectedDate.Date >= DateTime.Now)
            {
                DebugText("Invalid date");
                return;
            }
            string dobStr = dob.SelectedDate.Date.ToString("yyyy-MM-dd");
            NetworkMgr.RegisterFunc(this, registerUsername.text, dobStr, registerEmail.text, registerPassword.text); ;
        }
        catch (FormatException)
        {
            DebugText("Invalid email");
        }
    }
    public void Login()
    {
        if (loginPassword.text.Length < 8)
        {
            DebugText("Password needs to be at least 8 characters");
            return;
        }
        else if (loginUsername.text.Length == 0)
        {
            DebugText("Username cannot be empty");
            return;
        }

        NetworkMgr.LoginFunc(this, loginUsername.text, loginPassword.text);
    }
    public void SetPlayerData(PlayerData playerData)
    {
        headerHumans.text = playerData.humansTotal.ToString();
        profileHumans.text = playerData.humansTotal.ToString() + " Humans";
        int captorCount = 0;
        int saviorCount = 0;
        foreach (MyCharacterData character in playerData.characters) // Use the actual name of your CharacterData object
        {
            if (character.type == "captor")
                captorCount++;
            else if (character.type == "savior")
                saviorCount++;
        }

        MenuDeckMgr.UpdateHumans(playerData.characters);

        captorHumans.text = captorCount.ToString();
        saviorHumans.text = saviorCount.ToString();
        headerGems.text = playerData.gems.ToString();
        headerGold.text = playerData.barrels.ToString();
    }

    public void ShopBtnClick()
    {
        //Play video
        StartCoroutine(PlayVideo(0));
        StartCoroutine(PlayVideo(1));
    }

    IEnumerator PlayVideo(int videoIndex)
    {
        videoPlayer[videoIndex].gameObject.SetActive(true);
        int tempIndex = videoIndex;

        string videoFileName = "video" + videoIndex.ToString() + ".mp4";
        //string videoPath = Application.persistentDataPath + "/" + videoFileName;
        string videoURL = Application.streamingAssetsPath + "/" + videoFileName;

        UnityWebRequest www = UnityWebRequest.Get(videoURL);
        www.SendWebRequest();

        while (!www.isDone)
        {
            yield return null;
        }

        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(www.error);
        }
        else
        {
            //byte[] videoBytes = www.downloadHandler.data;
            //System.IO.File.WriteAllBytes(videoPath, videoBytes);

            videoPlayer[videoIndex].url = videoURL;
            videoPlayer[videoIndex].Play();
        }
    }
}
