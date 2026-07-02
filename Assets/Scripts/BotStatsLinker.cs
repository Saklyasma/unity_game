using UnityEngine;
using System.Collections.Generic;

public class BotStatsLinker : MonoBehaviour
{
    [System.Serializable]
    public class CountryStatEntry
    {
        public string countryName;
        public BotStatsData stats;
    }

    [Header("Assign all 10 country stats here")]
    public List<CountryStatEntry> entries;

    public BotAIController botAI;

    void Start()
    {
        if (CountryDataHolder.Instance == null)
        {
            Debug.LogWarning("[BotStatsLinker] CountryDataHolder not found!");
            return;
        }

        CountryFlag selected = CountryDataHolder.Instance.GetSelected();
        if (selected == null)
        {
            Debug.LogWarning("[BotStatsLinker] No country selected!");
            return;
        }

        foreach (var entry in entries)
        {
            if (entry.countryName == selected.countryName)
            {
                botAI.ApplyStats(entry.stats);
                return;
            }
        }

        Debug.LogWarning($"[BotStatsLinker] No stats found for '{selected.countryName}'");
    }
}