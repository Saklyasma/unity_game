using UnityEngine;
using System.Collections.Generic;

public class CountryDataHolder : MonoBehaviour
{
    public static CountryDataHolder Instance { get; private set; }

    public List<CountryFlag> Countries { get; private set; } = new();
    public int SelectedIndex { get; private set; } = -1;

    private readonly Dictionary<string, GameObject> botPrefabMap = new Dictionary<string, GameObject>();

    private static readonly Dictionary<string, string> CountryAliases = new Dictionary<string, string>
    {
        { "argentina", "argentina" },
        { "argentine", "argentina" },
        { "australia", "australia" },
        { "austtralia", "australia" },
        { "brazil", "brazil" },
        { "brezil", "brazil" },
        { "russia", "russia" },
        { "roussia", "russia" },
        { "roussia ", "russia" },
        { "algeria", "algeria" },
        { "germany", "germany" },
        { "japan", "japan" },
        { "niger", "niger" },
        { "france", "france" },
        { "usa", "usa" },
        { "tunisia", "tunisia" },
        { "tunisie", "tunisia" }
    };

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetCountries(List<CountryFlag> countries)
    {
        Countries = countries;
        Debug.Log($"[Holder] SetCountries: count={countries?.Count}, prevIdx={SelectedIndex}");

        if ((SelectedIndex < 0 || SelectedIndex >= Countries.Count) && Countries.Count > 0)
        {
            SelectedIndex = 0;
            Debug.Log($"[Holder] SetCountries clamped SelectedIndex to 0");
        }
    }

    public void SetSelectedIndex(int index)
    {
        string country = (Countries != null && index >= 0 && index < Countries.Count) ? Countries[index].countryName : "?";
        Debug.Log($"[Holder] SetSelectedIndex({index}) -> country={country}, countriesCount={Countries?.Count}");

        if (Countries == null || Countries.Count == 0)
        {
            SelectedIndex = index;
            Debug.Log($"[Holder] SetSelectedIndex: Countries null/empty, stored index={index}");
            return;
        }

        if (index < 0 || index >= Countries.Count)
        {
            SelectedIndex = 0;
            Debug.Log($"[Holder] SetSelectedIndex: index out of range, clamped to 0");
            return;
        }

        SelectedIndex = index;
    }

    public CountryFlag GetSelected()
    {
        string storedCountry = PlayerPrefs.GetString("OpponentCountry", string.Empty);
        Debug.Log($"[Holder] GetSelected: PlayerPrefs OpponentCountry='{storedCountry}', Countries={Countries?.Count}, SelectedIndex={SelectedIndex}");

        if (Countries != null && Countries.Count > 0)
        {
            if (!string.IsNullOrWhiteSpace(storedCountry))
            {
                string storedKey = NormalizeCountryKey(storedCountry);
                int storedIndex = Countries.FindIndex(c => NormalizeCountryKey(c.countryName) == storedKey);
                Debug.Log($"[Holder] GetSelected: storedKey='{storedKey}', foundIndex={storedIndex}");
                if (storedIndex >= 0)
                {
                    SelectedIndex = storedIndex;
                    Debug.Log($"[Holder] GetSelected returning Countries[{storedIndex}]='{Countries[storedIndex].countryName}'");
                    return Countries[storedIndex];
                }
            }

            if (SelectedIndex < 0 || SelectedIndex >= Countries.Count)
                SelectedIndex = 0;

            Debug.Log($"[Holder] GetSelected returning Countries[{SelectedIndex}]='{Countries[SelectedIndex].countryName}' (fallback)");
            return Countries[SelectedIndex];
        }

        if (!string.IsNullOrWhiteSpace(storedCountry))
        {
            Debug.Log($"[Holder] GetSelected: Countries null/empty, returning CountryFlag from PlayerPrefs: '{storedCountry}'");
            return new CountryFlag { countryName = storedCountry };
        }

        Debug.Log($"[Holder] GetSelected returning null");
        return null;
    }

    public void RegisterBotPrefab(string countryName, GameObject prefab)
    {
        string key = NormalizeCountryKey(countryName);
        if (string.IsNullOrEmpty(key) || prefab == null)
            return;

        botPrefabMap[key] = prefab;
    }

    public GameObject GetSelectedBotPrefab()
    {
        string storedCountry = PlayerPrefs.GetString("OpponentCountry", string.Empty);
        Debug.Log($"[Holder] GetSelectedBotPrefab: OpponentCountry='{storedCountry}', botPrefabMap has {botPrefabMap.Count} entries");

        if (string.IsNullOrWhiteSpace(storedCountry))
        {
            CountryFlag selected = GetSelected();
            if (selected == null)
            {
                Debug.Log($"[Holder] GetSelectedBotPrefab: no country in PlayerPrefs AND GetSelected returned null");
                return null;
            }
            storedCountry = selected.countryName;
            Debug.Log($"[Holder] GetSelectedBotPrefab: fallback to GetSelected country='{storedCountry}'");
        }

        string key = NormalizeCountryKey(storedCountry);
        Debug.Log($"[Holder] GetSelectedBotPrefab: looking up key='{key}' in botPrefabMap");

        if (botPrefabMap.TryGetValue(key, out var prefab))
        {
            Debug.Log($"[Holder] GetSelectedBotPrefab: FOUND prefab '{prefab?.name}' for key='{key}'");
            return prefab;
        }

        Debug.Log($"[Holder] GetSelectedBotPrefab: NO prefab found for key='{key}' — keys in map: {string.Join(", ", botPrefabMap.Keys)}");
        return null;
    }

    public static string NormalizeCountryKey(string countryName)
    {
        if (string.IsNullOrWhiteSpace(countryName))
            return string.Empty;

        string key = countryName.Trim().ToLowerInvariant();
        return CountryAliases.TryGetValue(key, out string normalized) ? normalized : key;
    }
}
