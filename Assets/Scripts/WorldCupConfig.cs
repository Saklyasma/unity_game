using UnityEngine;
using System.Collections.Generic;

public static class WorldCupConfig
{
    private static ConfigData _data;
    private static bool _loaded;

    [System.Serializable]
    public class ConfigData
    {
        public CountryEntry[] countries;
        public RegionEntry[] regions;
    }

    [System.Serializable]
    public class CountryEntry
    {
        public int index;
        public string displayName;
        public int goalsToWin;
        public bool startUnlocked;
        public string region;
    }

    [System.Serializable]
    public class RegionEntry
    {
        public string name;
        public int[] countryIndexes;
    }

    private static void Ensure()
    {
        if (_loaded) return;
        TextAsset json = Resources.Load<TextAsset>("WorldCupConfig");
        if (json != null)
        {
            _data = JsonUtility.FromJson<ConfigData>(json.text);
            Debug.Log($"[WorldCupConfig] Loaded {_data?.countries?.Length ?? 0} countries, {_data?.regions?.Length ?? 0} regions");
        }
        else
        {
            Debug.LogError("[WorldCupConfig] WorldCupConfig.json not found in Resources!");
            _data = new ConfigData();
        }
        _loaded = true;
    }

    public static int CountryCount()
    {
        Ensure();
        return _data?.countries?.Length ?? 0;
    }

    public static int GetGoalsToWin(int countryIndex)
    {
        Ensure();
        if (_data?.countries != null)
            foreach (var c in _data.countries)
                if (c.index == countryIndex)
                    return c.goalsToWin;
        return 1;
    }

    public static bool IsStartUnlocked(int countryIndex)
    {
        Ensure();
        if (_data?.countries != null)
            foreach (var c in _data.countries)
                if (c.index == countryIndex)
                    return c.startUnlocked;
        return false;
    }

    public static string GetDisplayName(int countryIndex)
    {
        Ensure();
        if (_data?.countries != null)
            foreach (var c in _data.countries)
                if (c.index == countryIndex)
                    return c.displayName;
        return $"Country_{countryIndex}";
    }

    public static RegionEntry[] GetRegions()
    {
        Ensure();
        return _data?.regions;
    }

    public static CountryEntry[] GetAllCountries()
    {
        Ensure();
        return _data?.countries;
    }
}
