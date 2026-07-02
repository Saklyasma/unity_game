using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class ProgressionManager : MonoBehaviour
{
    public static ProgressionManager Instance { get; private set; }

    private readonly List<RegionData> regions = new List<RegionData>
{
    new RegionData("Africa",       new List<int> { 0, 7  }),  // Algeria → Germany
    new RegionData("America",      new List<int> { 1, 3, 9}), // Argentine → Brezil → USA
    new RegionData("Oceania",      new List<int> { 2      }), // Australia
    new RegionData("Europe",       new List<int> { 4, 5  }),  // France → Roussia
    new RegionData("Asia",         new List<int> { 6, 8  }),  // Japan → Niger
};

    private const string SaveKey_Completed = "completed_";
    private const string SaveKey_Unlocked = "unlocked_";

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitFirstUnlock();
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
                Debug.Log("🏆 All regions complete!");
            }
        }
    }

    public bool IsUnlocked(int countryIndex)
        => PlayerPrefs.GetInt(SaveKey_Unlocked + countryIndex, 0) == 1;

    public bool IsCompleted(int countryIndex)
        => PlayerPrefs.GetInt(SaveKey_Completed + countryIndex, 0) == 1;

    // ── Helpers ───────────────────────────────────────────────────────

    private void InitFirstUnlock()
    {
        if (!IsUnlocked(0)) UnlockCountry(0);
    }

    private void UnlockCountry(int idx)
    {
        PlayerPrefs.SetInt(SaveKey_Unlocked + idx, 1);
        PlayerPrefs.Save();
        Debug.Log($"✅ Country {idx} unlocked!");
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
        foreach (var region in regions)
            foreach (int idx in region.countryIndexes)
            {
                PlayerPrefs.DeleteKey(SaveKey_Unlocked + idx);
                PlayerPrefs.DeleteKey(SaveKey_Completed + idx);
            }
        PlayerPrefs.Save();
        InitFirstUnlock();
    }
}