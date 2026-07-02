using UnityEngine;
using System.Collections.Generic;

public class CountryDataHolder : MonoBehaviour
{
    public static CountryDataHolder Instance { get; private set; }

    public List<CountryFlag> Countries { get; private set; } = new();
    public int SelectedIndex { get; private set; } = -1;

    private Dictionary<string, GameObject> botPrefabMap = new Dictionary<string, GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetCountries(List<CountryFlag> countries)
    {
        Countries = countries;
    }

    public void SetSelectedIndex(int index)
    {
        SelectedIndex = index;
    }

    public CountryFlag GetSelected()
    {
        if (SelectedIndex < 0 || SelectedIndex >= Countries.Count) return null;
        return Countries[SelectedIndex];
    }

    public void RegisterBotPrefab(string countryName, GameObject prefab)
    {
        botPrefabMap[countryName] = prefab;
    }

    public GameObject GetSelectedBotPrefab()
    {
        if (Countries == null || SelectedIndex < 0 || SelectedIndex >= Countries.Count)
            return null;

        string name = Countries[SelectedIndex].countryName;
        return botPrefabMap.TryGetValue(name, out var prefab) ? prefab : null;
    }
}