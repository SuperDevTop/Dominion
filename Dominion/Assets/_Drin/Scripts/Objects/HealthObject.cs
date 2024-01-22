using System;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthObject : MonoBehaviour
{
    private float maxHealth, currentHealth;
    private TextMeshProUGUI healthText;
    private Image healthBar;
    private GameObject deathEffect;
    private Transform spawnPoint;
    private Transform target;
    private Vector3 healthOffset;
    private void Awake()
    {
        healthText = GetComponentInChildren<TextMeshProUGUI>();
        healthBar = healthText.transform.parent.GetComponent<Image>();
    }
    public void Init(Transform target, GameObject deathEffect, Transform spawnPoint, float current, float max, int ple, Vector3 offset)
    {
        if (target == null)
        {
            if (ple != MulMgr.GetPl())
            {
                MulMgr.Health360(transform);
            }
        }
        else
        {
            transform.localRotation = MulMgr.CharRot();
        }

        if (target != null) 
            this.target = target;
        if (deathEffect != null)
            this.deathEffect = deathEffect;
        this.spawnPoint = spawnPoint;

        healthOffset = offset;
        Color clr = ple == 0 ? GameMgr.GetBlue() : GameMgr.GetRed();
        clr.a = 0.5f;
        healthBar.color = clr;
        maxHealth = max;
        Upd(current);
    }
    public void Update()
    {
        if (this.target == null) return;
        int dir = MulMgr.GetPl() == 0 ? -1 : 1;
        Vector3 newPos = this.target.position;
        if (dir == 1)
            newPos += healthOffset;
        newPos += dir * CharController.offset;
        transform.position = newPos; 
    }
    public void Upd(float current)
    {
        currentHealth = current;
        Display();
    }
    void Display()
    {
        healthBar.fillAmount = currentHealth / maxHealth;
        healthText.text = currentHealth.ToString();
    }
    public void Die()
    {
        Vector3 deathPos = transform.position;
        if (spawnPoint != null)
            deathPos = spawnPoint.position;

        GameObject go = Instantiate(deathEffect, deathPos, Quaternion.identity);
        Destroy(go, 3f);
        Destroy(gameObject);
    }
}
