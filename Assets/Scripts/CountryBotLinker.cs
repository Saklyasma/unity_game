using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class CountryBotEntry
{
    public string countryName;   // must match exactly what you typed in CountryFlag
    public GameObject botPrefab;
}

public class CountryBotLinker : MonoBehaviour
{
    [Header("Bot Prefab Assignments")]
    public List<CountryBotEntry> botEntries;

    void Start()
    {
        // Make sure the holder exists
        if (CountryDataHolder.Instance == null)
            new GameObject("CountryDataHolder").AddComponent<CountryDataHolder>();

        // Wait for the handler to set the countries, then register prefabs
        // (CountryFlagButtonHandler runs Start() first since it creates the holder)
        RegisterPrefabs();
    }

    void RegisterPrefabs()
    {
        foreach (var entry in botEntries)
        {
            CountryDataHolder.Instance.RegisterBotPrefab(entry.countryName, entry.botPrefab);
        }
    }
}