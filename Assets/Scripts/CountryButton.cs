using UnityEngine;
using UnityEngine.UI;

public class CountryButton : MonoBehaviour
{
    [Header("Country Settings")]
    public string countryName; // set this in Inspector for each country button

    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnCountrySelected);
    }

    void OnCountrySelected()
    {
        PlayerPrefs.SetString("SelectedCountry", countryName);
        PlayerPrefs.Save();
        Debug.Log($"Selected country: {countryName}");
    }
}