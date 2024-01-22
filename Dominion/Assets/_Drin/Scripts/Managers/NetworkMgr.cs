using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public class Token
{
    public string token;
}

public class NetworkMgr: MonoBehaviour
{
    public static NetworkMgr Instance;
    private string jwtToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VybmFtZSI6InBsYXllckIiLCJpYXQiOjE2OTg3MTA3MTN9.Hf9XuprlMHIqfFu1YMOJG9mYkWioT__V97OL-ecvY9A";
    [SerializeField]
    public PlayerData playerData;
    //public static string apiEndpoint = "localhost:8080";
    public static string apiEndpoint = "https://dominion-backend.ue.r.appspot.com";
    private void Awake()
    {
        if (GameObject.FindObjectsOfType<NetworkMgr>().Length == 1)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        AdManager.Instance.Init();
    }
    private void Start()
    {
        if (IsAuthed())
        {
            PhotonMgr.PUNSetUsername();
        }
        else
        {

        }
    }
    public static void SignOut()
    {
        PlayerPrefs.DeleteAll();
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
    public static bool IsAuthed()
    {
        return PlayerPrefs.HasKey("jwtToken");
    }
    public static string GetUsername()
    {
        return PlayerPrefs.GetString("username");
    }
    public static string GetEmail()
    {
        return PlayerPrefs.GetString("email");
    }
    public static int GetHumans()
    {
        return PlayerPrefs.GetInt("humans", 0);
    }
    public static string GetJWT()
    {
        return PlayerPrefs.GetString("jwtToken");
    }
    public static void RegisterFunc(MenuMgr mn, string username, string dob, string email, string password)
    {
        Instance.StartCoroutine(Register(mn, username, dob, email, password));
    }
    public static void LoginFunc(MenuMgr mn, string username, string password)
    {
        Instance.StartCoroutine(Login(mn, username, password));
    }
    public static IEnumerator Register(MenuMgr mn, string username, string dob, string email, string password)
    {
        using (UnityWebRequest www = new UnityWebRequest(apiEndpoint + "/register", "POST"))
        {
            MenuMgr.Load();
            string jsonData = 
                "{ " +
                    "\"username\" : \"" + username + "\", " +
                    "\"password\" : \"" + password + "\", " +
                    "\"dateOfBirth\" : \"" + dob + "\", " +
                    "\"email\" : \"" + email + "\" " +
                "}";
            print(jsonData);
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
                Debug.Log(www.result);
                Debug.Log(www.downloadHandler.text);
                Debug.Log(www.downloadHandler.data);
                mn.DebugText(www.downloadHandler.text);
            }
            else
            {
                Instance.jwtToken = www.downloadHandler.text; 
                PlayerPrefs.SetString("jwtToken", Instance.jwtToken);
                PlayerPrefs.SetString("username", username);
                PlayerPrefs.SetString("email", email);
                PlayerPrefs.SetString("dob", dob);
                PhotonMgr.PUNSetUsername();
                mn.SuccessfulAuth(username);
            }
            MenuMgr.UnLoad();
        }
    }

    public static IEnumerator Login(MenuMgr mn, string username, string password)
    {
        using (UnityWebRequest www = new UnityWebRequest(apiEndpoint + "/login", "POST"))
        {
            MenuMgr.Load();
            string jsonData = "{ \"username\" : \"" + username + "\", \"password\" : \"" + password + "\" }";
            print(jsonData);
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
                Debug.Log(www.result);
                Debug.Log(www.downloadHandler.text);
                Debug.Log(www.downloadHandler.data);
                mn.DebugText(www.downloadHandler.text);
            }
            else
            {
                print(www.downloadHandler.text);
                Instance.jwtToken = www.downloadHandler.text; // JsonUtility.FromJson<Token>(www.downloadHandler.text).token;
                PlayerPrefs.SetString("jwtToken", Instance.jwtToken);
                PlayerPrefs.SetString("username", username);
                PhotonMgr.PUNSetUsername();
                mn.SuccessfulAuth(username);
            }
            MenuMgr.UnLoad();
        }
    }
    public static void DeleteAccountFunc(MenuMgr mn)
    {
        Instance.StartCoroutine(DeleteAccount(mn));
    }

    private static IEnumerator DeleteAccount(MenuMgr mn)
    {
        using (UnityWebRequest www = new UnityWebRequest(apiEndpoint + "/delete", "GET"))
        {
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString("jwtToken"));

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
                Debug.Log(www.result);
                Debug.Log(www.downloadHandler.text);
                mn.DebugText(www.downloadHandler.text);
            }
            else
            {
                Debug.Log("Account deleted successfully");
                SignOut(); // Optionally sign out the user after account deletion
            }
        }
    }
    public static void NewMatchFunc(string matchProps)
    {
        Instance.StartCoroutine(NewMatch(matchProps));
    }
    public static IEnumerator NewMatch(string matchProps)
    {
        using (UnityWebRequest www = new UnityWebRequest(apiEndpoint + "/newMatch", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(matchProps);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString("jwtToken"));
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
                Debug.Log(www.result);
                Debug.Log(www.downloadHandler.text);
                Debug.Log(www.downloadHandler.data);
            }
            else
            {
                Debug.Log("Match created successfully");
            }
        }
    }
    public static void GetLatestFunc(MenuMgr mm)
    {
        Instance.StartCoroutine(GetLatest(mm));
    }
    public static IEnumerator GetLatest(MenuMgr m)
    {
        MenuMgr.Load();
        using (UnityWebRequest www = new UnityWebRequest(apiEndpoint + "/getLatest", "GET"))
        {
            www.downloadHandler = new DownloadHandlerBuffer();
            print("JWT: " + PlayerPrefs.GetString("jwtToken"));
            www.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString("jwtToken"));

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
                Debug.Log(www.result);
                Debug.Log(www.downloadHandler.text);
                Debug.Log(www.downloadHandler.data);
                m.DebugText(www.downloadHandler.text);
            }
            else
            {
                Instance.playerData = JsonUtility.FromJson<PlayerData>(www.downloadHandler.text);

                print("NEW DATA: " + www.downloadHandler.text);

                if (((Instance.playerData.matches.Count + 2) % 10) == 0)
                {
                    AdManager.ShowAd();
                }

                m.SetPlayerData(Instance.playerData);

                PlayerPrefs.SetString("playerData", JsonUtility.ToJson(Instance.playerData));
            }
        }
        MenuMgr.UnLoad();
    }


    [Serializable]
    public class MyCharacterData
    {
        public string name;
        public int plays;
        public string type;
    }

    [Serializable]
    public class MatchData
    {
        public string _id;
        public string winnerUsername;
        public string loserUsername;
        public int crownsAmount;
        public List<MyCharacterData> blueCharactersUsed;
        public List<MyCharacterData> redCharactersUsed;
        // Add other match properties as needed
    }

    [Serializable]
    public class PlayerData
    {
        public int humansTotal, gems, barrels;
        public List<MatchData> matches;
        public List<MyCharacterData> characters;
    }
}

