using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using System;

[RequireComponent(typeof(ClashObject))]
public class CharController : MonoBehaviourPun
{
    public static Vector3 offset = new Vector3(0.15f, -1.5f, -2.5f);
    private ClashObject myObj;
    private NavMeshAgent agent;
    private Animation anim;
    public GameObject projectile;
    public float projectileFinishYOffset = 0;

    [SerializeField]
    public CharacterData data;
    private string[] animationName = { "walk", "attack" };

    private void OnDrawGizmos()
    {
        if (myObj == null) return;  
        Gizmos.DrawWireSphere(myObj.positionTrack.position, data.attackRange);
        Gizmos.DrawWireSphere(myObj.positionTrack.position, data.visionRange);
    }
    void Awake()
    {
        StopAllCoroutines();
        myObj = GetComponent<ClashObject>();

        // Simplified the logic to determine the player
        int pl = MulMgr.IsMine(photonView);

        myObj.Setup(data.health, data.visionRange, data.dmg, pl);
        GameMgr.Add(myObj, pl);

        anim = GetComponentInChildren<Animation>();

        anim.Play(animationName[0]);

        if (!MulMgr.IsMaster()) return;

        bool isBlue = (pl == 0); // Assuming 0 represents blue and 1 represents red
        SingleCharData characterData = new SingleCharData { name = data.characterName, type = data.characterType };
        GameStateMgr.instance.AddPlay(characterData, isBlue);

        agent = GetComponent<NavMeshAgent>();
        agent.enabled = true;
        //agent.stoppingDistance = data.range * 0.99f;
        agent.speed = data.moveSpeed;
        StartCoroutine(Run());

        //EventBus.Subscribe<EndedEvent>(EndHandler);
    }
    private ClashObject lastTarget = null;
    private bool locked = false;
    IEnumerator Run()
    {
        while (true)
        {
            yield return null;

            if (GameStateMgr.ended) yield break;
            if (!agent.isOnNavMesh || !agent.isActiveAndEnabled) continue;

            if (lastTarget == null || !locked)
            {
                ClashObject[] objs = GameMgr.GetCharacterTarget(myObj);
                if (objs.Length > 0)
                {
                    lastTarget = objs[0];
                    Vector3 newPos = lastTarget.positionTrack.position;
                    newPos.y = myObj.positionTrack.position.y;
                    agent.isStopped = false;
                    agent.SetDestination(newPos);
                }
            }

            if (lastTarget)
            {
                //float dist = agent.GetPathRemainingDistance();
                Vector2 trgt = new Vector2(lastTarget.positionTrack.position.x, lastTarget.positionTrack.position.z);
                Vector2 min = new Vector2(myObj.positionTrack.position.x, myObj.positionTrack.position.z);
                float dist = Vector2.Distance(min, trgt);
                //print("Distance: " + dist);
                //if (lastTarget.GetComponent<TowerController>())
                    //range = 1.25f;
                if (dist <= data.attackRange)
                {
                    locked = true;
                    agent.isStopped = true;
                    //agent.SetDestination(transform.position);
                    transform.LookAt(lastTarget.positionTrack.position, transform.up);

                    
                    if (lastTarget != null)
                    {
                        SetAnim(1);

                        yield return new WaitForSeconds(data.shootRate);
                        
                        if (projectile != null)
                            BulletObject.Create(myObj.positionTrack.position, myObj, lastTarget, projectile, projectileFinishYOffset);
                        else
                            lastTarget.TakeDmg(data.dmg);
                    }
                }
                else
                {
                    locked = false;
                    SetAnim(0);
                }
            }
        }
    }

    private void SetAnim(int which)
    {
        if (MulMgr.instance.TESTMODE)
            SetAnimPun(which);
        else
            photonView.RPC("SetAnimPun", RpcTarget.All, which);
    }

    [PunRPC]
    public void SetAnimPun(int which)
    {
        string objectName = myObj.name.Split('(')[0];
        
        if(objectName == "Kangru" || objectName == "Babs" || objectName == "Cat")
        {
            if(which == 0)
            {
                myObj.transform.GetChild(0).gameObject.SetActive(true);
                myObj.transform.GetChild(1).gameObject.SetActive(false);
            }
            else
            {
                myObj.transform.GetChild(0).gameObject.SetActive(false);
                myObj.transform.GetChild(1).gameObject.SetActive(true);
            }
        }
        else
        {
            anim.Play(animationName[which]);
        }                                
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
