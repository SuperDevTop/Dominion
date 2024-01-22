using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncedTimer : MonoBehaviourPun
{
    public static float introTime = 6f;
    public float time = 180;
    private void Start()
    {
        if (!MulMgr.IsMaster()) return;

        print("SyncedTimer: Started on Master");
        StartCoroutine(Timer());
    }
    IEnumerator Timer()
    {
        yield return new WaitForSecondsRealtime(introTime);
        while (true)
        {
            float newTime = Mathf.Clamp(time - 1, 0, 180);

            if (MulMgr.instance.TESTMODE)
                UpdateTime(newTime);
            else
                photonView.RPC("UpdateTime", RpcTarget.All, newTime);

            yield return new WaitForSecondsRealtime(1f);
        }
    }

    [PunRPC]
    void UpdateTime(float newTime)
    {
        if (GameStateMgr.ended) return;

        time = newTime;

        if (time <= 60)
            DeckMgr.SetEMul(2);

        if (time <= 0)
        {
            time = 0;
            GameStateMgr.End();
            StopAllCoroutines();
        }

        UIMgr.DisplayTime(time);
    }
}
