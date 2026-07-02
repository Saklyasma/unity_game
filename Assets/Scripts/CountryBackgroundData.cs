using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CountryBackgroundData", menuName = "WorldCup/Country Background Data")]
public class CountryBackgroundData : ScriptableObject
{
    [System.Serializable]
    public class CountryBG
    {
        public string countryName;
        public Sprite backgroundSprite;
    }

    public CountryBG[] countries;

    private Dictionary<string, Sprite> _cache;

    public Sprite GetBackground(string countryName)
    {
        if (_cache == null)
            BuildCache();

        if (string.IsNullOrWhiteSpace(countryName))
            return null;

        string key = countryName.Trim();
        if (_cache.TryGetValue(key, out var sprite))
            return sprite;

        string keyLower = key.ToLowerInvariant();
        foreach (var kv in _cache)
            if (kv.Key.ToLowerInvariant() == keyLower)
                return kv.Value;

        return null;
    }

    void BuildCache()
    {
        _cache = new Dictionary<string, Sprite>(countries.Length);
        foreach (var entry in countries)
            if (entry != null && !string.IsNullOrEmpty(entry.countryName))
                _cache[entry.countryName.Trim()] = entry.backgroundSprite;
    }

    void OnValidate() => _cache = null;
}