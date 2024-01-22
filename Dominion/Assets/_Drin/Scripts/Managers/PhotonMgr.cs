using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Net;

public class PhotonMgr : MonoBehaviourPunCallbacks
{
    // old app code 0cfeafbc-d1d9-4c96-ad5a-e63437de081a
    //public string roomName = "MainRoom";
    public bool TESTMODE = false;
    public string gameVersion = "1";

    public static PhotonMgr instance;
    public byte maxPlayersPerRoom = 2;

    public bool isConnecting;

    RoomOptions roomOptions;
    public bool connected;
    public bool inRoom;

    public GameObject roomListPrefab;
    public GameObject waitingPanel;
    public GameObject loadingPanel;
    public GameObject gamePanel;
    public TMP_InputField createRoom;
    public TextMeshProUGUI info;

    public GameObject auth;
    public Text username, password;

    public GameObject cancelObj;
    public static string player1Name;
    public static string player2Name;
    // Basically the auth object contains two fields for username and password, and I need these two functions to be implemented:
    public void Register()
    {
        StartCoroutine(RegisterCoroutine());
    }

    public static string api = "https://demo-auth-n6fdde7yqa-ey.a.run.app/register";
    private IEnumerator RegisterCoroutine()
    {
        // Create the POST request
        using (UnityWebRequest www = new UnityWebRequest(api, "POST"))
        {
            string jsonData = "{ \"username\" : \"" + username.text + "\", \"password\" : \"" + password.text + "\" }";
            print(jsonData);
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Register failed: " + www.error);
            }
            else
            {
                Debug.Log("Register succeeded: " + www.downloadHandler.text);
                Login(username.text, password.text);
            }
        }
    }

    public void DefaultLogin()
    {
        Login(username.text, password.text);
    }
    public void Login(string user, string passwd)
    {
        info.text = "Logging in Server...";
        loadingPanel.SetActive(true);
        var authValues = new AuthenticationValues();
        authValues.AuthType = CustomAuthenticationType.Custom;
        authValues.AddAuthParameter("username", user);
        authValues.AddAuthParameter("password", passwd);

        PhotonNetwork.AuthValues = authValues;

        ConnectToRegion();
    }
    public override void OnCustomAuthenticationFailed(string debugMessage)
    {
        Debug.LogError("Authentication failed: " + debugMessage);
    }
    void Awake()
    {
        print("PHOOTON MGR AWAKE");
        instance = this;
        PhotonNetwork.AutomaticallySyncScene = true;
    }
    private void Start()
    {
        if (TESTMODE) return;
        MenuMgr.Load();
        loadingPanel.SetActive(true);
        //auth.SetActive(false);
        if (PhotonNetwork.IsConnected)
        {
            loadingPanel.SetActive(true);
            //auth.SetActive(false);
            PhotonNetwork.JoinLobby();
            Debug.Log("Connected to Master ");
        }
        else
        {
            ConnectToRegion();
        }

        player1Name = "";
        player2Name = "";
    }

    void ConnectToRegion()
    {
        AppSettings regionSettings = new AppSettings();
        regionSettings.UseNameServer = true;
        regionSettings.FixedRegion = "ussc";
        regionSettings.AppIdRealtime = "a16d4389-5e62-4b30-9234-181f7818083f";
        regionSettings.AppVersion = gameVersion;
        PhotonNetwork.ConnectUsingSettings(regionSettings);

        //PhotonNetwork.ConnectUsingSettings();
        //PhotonNetwork.GameVersion = gameVersion;
    }

    public override void OnConnectedToMaster()
    {
        //auth.SetActive(false);
        PhotonNetwork.JoinLobby();
        Debug.Log("Connected to Master ");
    }
    public static void PUNSetUsername()
    {
        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
        {
            { "username", NetworkMgr.GetUsername() }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }
    public override void OnJoinedLobby()
    {
        cancelObj.SetActive(true);
        Debug.Log("In Lobby ");
        loadingPanel.SetActive(false);
        gamePanel.SetActive(true);
        MenuMgr.UnLoad();
        PhotonNetwork.NickName = MenuMgr.Instance.user.text;           
    }
    public void CancelRoom()
    {
        
    }
    public void CreateRoom()
    {
        if (!MenuDeckMgr.CanBattle())
        {
            MenuMgr.Instance.DebugText("Not enough cards in deck");
            return;
        }
        MenuMgr.Load();
        roomOptions = new RoomOptions()
        {
            IsVisible = true,
            IsOpen = true,
            MaxPlayers = maxPlayersPerRoom,
            CleanupCacheOnLeave = false
        };
        //PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
        PhotonNetwork.JoinRandomOrCreateRoom();
    }

    public override void OnJoinedRoom()
    {
        info.text = "Waiting for opponent to join...";
        waitingPanel.SetActive(true);
        MenuMgr.UnLoad();

        if(PhotonNetwork.IsMasterClient)
        {
            player1Name = PhotonNetwork.NickName;            
        }
        else
        {                                    
            player1Name = PhotonNetwork.PlayerListOthers[0].NickName;
            player2Name = PhotonNetwork.NickName;            
        }
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        //Debug.Log("New Player");
        if (PhotonNetwork.CurrentRoom.PlayerCount == maxPlayersPerRoom)
        {
            //Debug.Log(PhotonNetwork.CurrentRoom.PlayerCount);
            player2Name = newPlayer.NickName;
            PhotonNetwork.CurrentRoom.IsVisible = false;
            PhotonNetwork.CurrentRoom.IsOpen = false;            
            PhotonNetwork.LoadLevel("GamePlay");            
        }
    }
}
