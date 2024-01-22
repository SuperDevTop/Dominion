using UnityEngine;
using Photon.Pun;
using System;
using Photon.Realtime;
using System.Collections;

public class MulMgr : MonoBehaviourPunCallbacks
{
    public bool TESTMODE = false;
    public int TESTPL = 0;
    public GameObject camRef;
    public static MulMgr instance;
    private void Awake()
    {
        instance = this;
        if (TESTMODE) gameObject.AddComponent<NetworkMgr>();
    }
    public static bool IsTest()
    {
        return instance.TESTMODE;
    }
    private void Start()
    {
        SetCameraAngle();
    }
    void SetCameraAngle()
    {
        if (GetPl() == 1) return;

        camRef.transform.eulerAngles = new Vector3(camRef.transform.eulerAngles.x, 180, camRef.transform.eulerAngles.z);
        //camRef.transform.localPosition = new Vector3(camRef.transform.localPosition.x, camRef.transform.localPosition.y, 0.72f);
    }
    public static int i = 1;
    public static void PrintN(PhotonView p)
    {
        /*
        print("PRINT NEW " + i);
        print(p.gameObject.name);
        print(p.ViewID);
        i++;
        */
    }
    public static string BlueUsername()
    {
        if (IsTest())
            return "Blue";
        else
            return (string)PhotonNetwork.PlayerList[0].CustomProperties["username"];
    }
    public static string RedUsername()
    {
        if (IsTest())
            return "Red";
        else
            return (string)PhotonNetwork.PlayerList[1].CustomProperties["username"];
    }
    public static string GetOtherPlayerUsername()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player != PhotonNetwork.LocalPlayer)
            {
                return (string)player.CustomProperties["username"];
            }
        }
        return null;
    }
    public IEnumerator DestroyRoutine(GameObject obj, float after)
    {
        yield return new WaitForSeconds(after);
        if (instance.TESTMODE)
            Destroy(obj);
        else
            PhotonNetwork.Destroy(obj);
        yield break;
    }
    public static void SpawnDie(GameObject spawnObj, Vector3 pos, float after, bool isPlayer = true)
    {
        GameObject spawned = Spawn(spawnObj, pos, isPlayer);
        instance.StartCoroutine(instance.DestroyRoutine(spawned, after));     
    }
    public static GameObject Spawn(GameObject spawnObj, Vector3 pos, bool isPlayer = true)
    {
        GameObject go;
        if (instance.TESTMODE)
            go = Instantiate(spawnObj, pos, CharRot()); // U case
        else
        {
            if (isPlayer)
                go = PhotonNetwork.Instantiate(spawnObj.name, pos, CharRot());
            else
                go = PhotonNetwork.Instantiate(spawnObj.name, pos, Quaternion.identity);
            if (!PhotonNetwork.IsMasterClient)
            {
                PhotonView photonView = go.GetComponent<PhotonView>();
                PrintN(photonView);
                photonView.TransferOwnership(PhotonNetwork.MasterClient);
            }
        }
        return go;
    }
    public static void Health360(Transform trans)
    {
        Vector3 pos = trans.localPosition;
        pos.z += 16;
        trans.localPosition = pos;
        trans.localRotation = Quaternion.Euler(0, 180, 0);
    }
    public static Quaternion CharRot()
    {
        if (GetPl() == 0)
            return Quaternion.Euler(0, 180, 0);
        else
            return Quaternion.identity;
    }
    public static int GetPl()
    {
        if (instance.TESTMODE)
            return instance.TESTPL;

        if (PhotonNetwork.PlayerList[0].IsLocal)
            return 0;
        else
            return 1;
    }
    public static int GetOtherPl()
    {
        return GetPl() == 0 ? 1 : 0;
    }
    public static bool IsMaster()
    {
        return instance.TESTMODE || PhotonNetwork.IsMasterClient;
    }
    public static int IsMine(PhotonView photonView)
    {
        if (instance.TESTMODE)
            return instance.TESTPL;
        return photonView.IsMine ? GetPl() : GetOtherPl();
    }
}
