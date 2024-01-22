using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GameMgr : MonoBehaviour
{
    public float snap = 2f;
    public Color blueClr, redClr;
    public AudioSource towerFire;
    public MeshRenderer[] blueBounds, redBounds;
    // General prefabs
    public GameObject bulletPrefab, charHealthPrefab, deathEffectChar, towerDeath;

    private static GameMgr instance;
    private void Awake() => instance = this;
    public List<ClashObject> objs = new List<ClashObject>();
    public List<ClashObject> towers = new List<ClashObject>();
    public static float GetSnap()
    {
        return instance.snap;
    }
    public static Color GetBlue()
    {
        return instance.blueClr;
    }
    public static Color GetRed()
    {
        return instance.redClr;
    }
    public static void TowerFire()
    {
        instance.towerFire.Play();

    }
    public static GameObject towerDeathPrefab()
    {
        return instance.towerDeath;
    }
    public static GameObject deathEffectPrefab()
    {
        return instance.deathEffectChar;
    }
    public static GameObject healthPrefab()
    {
        return instance.charHealthPrefab;
    }

    public static GameObject bullet()
    {
        return instance.bulletPrefab;
    }
    void Start()
    {
        objs = new List<ClashObject>();
        towers = new List<ClashObject>();
        ClashObject[] cos = FindObjectsOfType<ClashObject>();

        objs.InsertRange(0, cos);
        towers.InsertRange(0, cos);
    }
    public static MeshRenderer[] GetBlueBounds()
    {
        return instance.blueBounds; 
    }
    public static MeshRenderer[] GetRedBounds()
    {
        return instance.redBounds; 
    }


    public static void Add(ClashObject co, int pl)
    {
        co.pl = pl;
        instance.objs.Add(co);
    }
    public static void Remove(ClashObject co)
    {
        instance.towers.Remove(co);
        instance.objs.Remove(co);
    }
    public static ClashObject[] GetCharacterTarget(ClashObject from)
    {
        ClashObject[] objs = GetObjectsWithin(from);
        if (objs == null || objs.Length == 0) objs = GetTowers(from);
        return objs;
    }
    public static ClashObject[] GetObjectsWithin(ClashObject from)
    {
        List<ClashObject> withinRadius = new List<ClashObject>();
        foreach (ClashObject co in instance.objs)
        {
            if (co == null) continue;
            if (co.pl != from.pl && Vector3.Distance(from.positionTrack.position, co.positionTrack.position) <= from.rangeRadius)
                withinRadius.Add(co);
        }
        withinRadius.Sort((co1, co2) => Vector3.Distance(from.positionTrack.position, co1.positionTrack.position).CompareTo(Vector3.Distance(from.positionTrack.position, co2.positionTrack.position)));
        return withinRadius.ToArray();
    }
    public static ClashObject[] GetTowers(ClashObject from)
    {
        List<ClashObject> towers = new List<ClashObject>();
        foreach (ClashObject co in instance.towers)
        {
            if (co == null || co.pl == from.pl) continue;
            towers.Add(co);
        }

        if (towers.Count == 3)
            towers.Sort((co1, co2) => Mathf.Abs(co1.positionTrack.position.x - from.positionTrack.position.x).CompareTo(
                Mathf.Abs(co2.positionTrack.position.x - from.positionTrack.position.x)));
        else
            towers.Sort((co1, co2) => Vector3.Distance(from.positionTrack.position, co1.positionTrack.position)
                .CompareTo(Vector3.Distance(from.positionTrack.position, co2.positionTrack.position)));
        return towers.ToArray();
    }
}
