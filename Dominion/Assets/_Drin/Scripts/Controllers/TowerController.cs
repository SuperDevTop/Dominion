using Photon.Pun;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public class TowerController : MonoBehaviourPun
{
    public GameObject bullet;
    public Transform shootPos;
    public float maxHealth, dmg, shootRate, range;
    public Animation anim;
    private ClashObject myObj;
    public int type = 1;
    public int pl = -1;
    private bool locked = false;
    private ClashObject lastObject;
    private void Start()
    {
        StopAllCoroutines();
        myObj = GetComponent<ClashObject>();
        myObj.Setup(maxHealth, range, dmg, pl, type);
        if (!MulMgr.IsMaster()) return;

        StartCoroutine(RunTower());
        //EventBus.Subscribe<EndedEvent>(EndHandler);
    }
    private void EndHandler(EndedEvent e)
    {
        //StopAllCoroutines();
    }
    IEnumerator RunTower()
    {
        while (true)
        {
            if (GameStateMgr.ended) yield break;
            if (lastObject == null || !locked) { 
                ClashObject[] cos = GameMgr.GetObjectsWithin(myObj);
                if (cos != null && cos.Length != 0)
                {
                    lastObject = cos[0];
                    locked = true;
                }
            }
            else if (lastObject != null)
            {
                BulletObject.Create(shootPos.position, myObj, lastObject, bullet);
                GameMgr.TowerFire();
                if (MulMgr.instance.TESTMODE)
                    PlayAnim();
                else
                    photonView.RPC("PlayAnim", RpcTarget.All);
                yield return new WaitForSeconds(shootRate);
            }
            yield return null;
        }
    }
    [PunRPC]
    public void PlayAnim()
    {
        anim.Play();
        GameMgr.TowerFire();
    }
}
