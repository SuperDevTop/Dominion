using Photon.Pun;
using UnityEngine;

public class ClashObject : MonoBehaviourPun
{
    public Transform positionTrack, healthTracker;
    public Vector3 specificHealthOffset = new Vector3(0f, 0f, 0f);
    [HideInInspector]
    public float maxHealth, rangeRadius, dmg;
    private float currentHealth;
    [HideInInspector]
    public int pl = -1;
    private HealthObject visual;
    private int tType = -1;
    private bool isDead = false;
    public void Setup(float maxH, float range, float dmg, int pl, int towerType = -1)
    {
        if (positionTrack == null) positionTrack = transform;
        visual = GetComponentInChildren<HealthObject>();
        maxHealth = currentHealth = maxH;
        this.dmg = dmg;
        rangeRadius = range;
        this.pl = pl;
        this.tType = towerType;
        if (visual != null)
            visual.Init(null, GameMgr.towerDeathPrefab(), positionTrack, currentHealth, maxHealth, pl, Vector3.zero);
        else
        {
            visual = Instantiate(GameMgr.healthPrefab(), healthTracker.position, Quaternion.identity).GetComponent<HealthObject>();
            visual.transform.localScale = Vector3.one / 2;
            visual.Init(transform, GameMgr.deathEffectPrefab(), positionTrack, currentHealth, maxHealth, pl, specificHealthOffset);
        }
    }
    public void TakeDmg(float amnt)
    {
        float newH = currentHealth - amnt;
        if (newH < 0)
            newH = 0;

        if (MulMgr.instance.TESTMODE)
            UpdateHealth(newH);
        else if (!isDead)
            photonView.RPC("UpdateHealth", RpcTarget.All, newH);
    }

    [PunRPC]
    public void UpdateHealth(float newHealth)
    {
        currentHealth = newHealth;
        visual.Upd(currentHealth);

        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        if (visual != null)
            visual.Die();
        if (tType != -1)
            EventBus.Publish(new TowerDestroyedEvent(pl, tType));
        GameMgr.Remove(this);
        if (!MulMgr.IsMaster()) return;
        if (MulMgr.instance.TESTMODE)
            Destroy(gameObject);
        else if (photonView.IsMine)
            PhotonNetwork.Destroy(gameObject);
    }
}
