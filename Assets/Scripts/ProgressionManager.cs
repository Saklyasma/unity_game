using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class ProgressionManager : MonoBehaviour
{
    public static ProgressionManager Instance { get; private set; }

    private List<RegionData> regions = new List<RegionData>();

    private const string SaveKey_Completed = "completed_";
    private const string SaveKey_Unlocked = "unlocked_";

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadRegionsFromConfig();
        InitUnlocks();
    }

    private void LoadRegionsFromConfig()
    {
        regions.Clear();
        var configRegions = WorldCupConfig.GetRegions();
        if (configRegions != null)
        {
            foreach (var r in configRegions)
            {
                if (r.countryIndexes != null && r.countryIndexes.Length > 0)
                    regions.Add(new RegionData(r.name, new List<int>(r.countryIndexes)));
            }
        }
        Debug.Log($"[ProgressionManager] Loaded {regions.Count} regions from config");
    }

    /// <summary>Call this after the player wins a match.</summary>
    public void OnMatchWin(int countryIndex)
    {
        MarkCompleted(countryIndex);

        int regionIdx = GetRegionIndex(countryIndex);
        if (regionIdx < 0) return;

        RegionData region = regions[regionIdx];
        int posInRegion = region.countryIndexes.IndexOf(countryIndex);
        bool hasNextInRegion = posInRegion < region.countryIndexes.Count - 1;

        if (hasNextInRegion)
        {
            int next = region.countryIndexes[posInRegion + 1];
            UnlockCountry(next);
        }
        else if (IsRegionComplete(region))
        {
            if (regionIdx + 1 < regions.Count)
            {
                int firstOfNext = regions[regionIdx + 1].countryIndexes[0];
                UnlockCountry(firstOfNext);
            }
            else
            {
                Debug.Log("[ProgressionManager] All regions complete!");
            }
        }
    }

    /// <summary>Returns the first unlocked but not yet completed country index, or -1 if all done.</summary>
    public int GetNextPlayableCountry()
    {
        int total = WorldCupConfig.CountryCount();
        for (int i = 0; i < total; i++)
            if (IsUnlocked(i) && !IsCompleted(i))
                return i;
        return -1;
    }

    public bool IsUnlocked(int countryIndex)
        => PlayerPrefs.GetInt(SaveKey_Unlocked + countryIndex, 0) == 1;

    public bool IsCompleted(int countryIndex)
        => PlayerPrefs.GetInt(SaveKey_Completed + countryIndex, 0) == 1;

    // ── Helpers ───────────────────────────────────────────────────────

    private void InitUnlocks()
    {
        int total = WorldCupConfig.CountryCount();
        for (int i = 0; i < total; i++)
        {
            if (WorldCupConfig.IsStartUnlocked(i) && !IsUnlocked(i))
                UnlockCountry(i);
        }
    }

    private void UnlockCountry(int idx)
    {
        PlayerPrefs.SetInt(SaveKey_Unlocked + idx, 1);
        PlayerPrefs.Save();
        Debug.Log($"[ProgressionManager] Country {idx} unlocked!");
    }

    private void MarkCompleted(int idx)
    {
        PlayerPrefs.SetInt(SaveKey_Completed + idx, 1);
        PlayerPrefs.Save();
    }

    private bool IsRegionComplete(RegionData region)
    {
        foreach (int idx in region.countryIndexes)
            if (!IsCompleted(idx)) return false;
        return true;
    }

    private int GetRegionIndex(int countryIndex)
    {
        for (int i = 0; i < regions.Count; i++)
            if (regions[i].countryIndexes.Contains(countryIndex))
                return i;
        return -1;
    }

    public void ResetProgress()
    {
        int total = WorldCupConfig.CountryCount();
        for (int i = 0; i < total; i++)
        {
            PlayerPrefs.DeleteKey(SaveKey_Unlocked + i);
            PlayerPrefs.DeleteKey(SaveKey_Completed + i);
        }
        PlayerPrefs.Save();
        InitUnlocks();
    }
}
