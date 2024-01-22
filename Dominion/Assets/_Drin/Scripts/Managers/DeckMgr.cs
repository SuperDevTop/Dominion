using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckMgr : MonoBehaviour
{
    public GameObject syncedTimer;
    public static DeckMgr instance;
    public List<CharacterData> deck;
    public float maxElixir;
    public static float elixir;
    public float elixirPerSecond;
    public float elixirRateMultiplier = 1;
    private void Awake() {
        instance = this;
        elixir = 1;
        elixirRateMultiplier = 1;
        if (MulMgr.IsTest())
            elixirRateMultiplier = 100;
    }
    public static float MaxElixir()
    {
        return instance.maxElixir;
    }
    void Start()
    {
        ShuffleDeck();
        StartCoroutine(GenerateElixir());

        if (MulMgr.IsMaster())
            MulMgr.Spawn(syncedTimer, Vector3.zero);

        EventBus.Subscribe<EndedEvent>(EndHandler);
    }
    private void EndHandler(EndedEvent e)
    {
        StopAllCoroutines();
    }
    void ShuffleDeck()
    {
        for (int i = 0; i < deck.Count; i++)
        {
            CharacterData temp = deck[i];
            int randomIndex = Random.Range(i, deck.Count);
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
        UIMgr.UpdateCards(deck);
    }
    public static void SetEMul(float amnt)
    {
        if (instance.elixirRateMultiplier == 1 && amnt == 2)
            UIMgr.DoubleElixir();
        instance.elixirRateMultiplier = amnt;
    }
    public static void Use(CharacterData data)
    {
        if (elixir < data.cost) return;
        instance.deck.Remove(data);
        instance.deck.Add(data);
        elixir -= data.cost;        
        UIMgr.UpdateCards(instance.deck);
    }
    IEnumerator GenerateElixir() // @TODO: unsafe, unvalidated elixir production
    {
        yield return new WaitForSecondsRealtime(SyncedTimer.introTime);
        while (true)
        {
            elixir = Mathf.Clamp(elixir + elixirPerSecond * elixirRateMultiplier * Time.smoothDeltaTime, 0, maxElixir);
            UIMgr.DisplayElixir(elixir, maxElixir);
            yield return null;
        }
    }
}
