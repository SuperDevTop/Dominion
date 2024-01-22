using Photon.Pun;
using System.Collections;
using UnityEngine;

public class BulletObject : MonoBehaviourPun
{
    private ClashObject target, from;
    public GameObject deathEffect;
    private float finishYOff = 0f;
    public static void Create(Vector3 spawnPos, ClashObject from, ClashObject target, GameObject newProj = null, float finishYOffset = 0f)
    {
        GameObject proj = newProj ? newProj : GameMgr.bullet();
        BulletObject bullet = MulMgr.Spawn(proj, spawnPos).GetComponent<BulletObject>();
        if (MulMgr.instance.TESTMODE)
            bullet.LocalSetup(from, target, finishYOffset);
        else
            bullet.photonView.RPC("Setup", RpcTarget.All, from.photonView.ViewID, target.photonView.ViewID, finishYOffset);
    }
    void LocalSetup(ClashObject from, ClashObject target, float finishYOffset = 0f)
    {
        this.from = from;
        this.target = target;
        this.finishYOff = finishYOffset;

        if (!MulMgr.IsMaster()) return;

        StartCoroutine(Run());
    }

    [PunRPC]
    void Setup(int fromViewID, int targetViewID, float finishYOffset = 0f)
    {
        var pV = PhotonView.Find(fromViewID);
        var pT = PhotonView.Find(targetViewID);
        this.finishYOff = finishYOffset;

        if (pV == null || pT == null)
        {
            Destroy(gameObject);
            return;
        }

        from = pV.GetComponent<ClashObject>(); 
        target = pT.GetComponent<ClashObject>();

        if (!MulMgr.IsMaster()) return;

        StartCoroutine(Run());
    }
    private IEnumerator Run()
    {
        yield return null;
        while (true)
        {
            if (target == null)
            {
                PhotonNetwork.Destroy(gameObject);
                yield break;
            }

            Vector3 moveDir = (target.positionTrack.position - transform.position).normalized;
            transform.position += moveDir * 15f * Time.deltaTime;

            if (Vector3.Distance(transform.position, target.positionTrack.position) <= 0.2f)
            {
                if (deathEffect)
                {
                    Vector3 newPos = target.positionTrack.position;
                    newPos.y += finishYOff;
                    MulMgr.SpawnDie(deathEffect, newPos, 3f);
                }
                target.TakeDmg(from.dmg);
                PhotonNetwork.Destroy(gameObject);
                yield break;
            }

            yield return null;
        }
    }
}
