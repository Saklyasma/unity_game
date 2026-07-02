using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[DefaultExecutionOrder(1000)]
public class BackgroundManager : MonoBehaviour
{
    public Image terrainImage;
    public CountryBackgroundData bgData;
    public Sprite defaultBackground;

    void Start()
    {
        StartCoroutine(ApplyBackgroundDelayed());
    }

    IEnumerator ApplyBackgroundDelayed()
    {
        yield return null;
        ApplyBackground();
    }

    public void ApplyBackground()
    {
        if (bgData == null)
            bgData = Resources.Load<CountryBackgroundData>("CountryBackgroundData");

        if (bgData == null)
        {
            Debug.LogError("[BG] CountryBackgroundData not found in Resources!");
            ApplyFallback();
            return;
        }

        string country = PlayerPrefs.GetString("OpponentCountry", "").Trim();

        if (string.IsNullOrEmpty(country))
        {
            Debug.LogWarning("[BG] No country in PlayerPrefs — using default background.");
            ApplyFallback();
            return;
        }

        Debug.Log($"[BG] Looking for background: '{country}'");

        Sprite bg = bgData.GetBackground(country);

        if (bg != null)
        {
            terrainImage.sprite = bg;
            Debug.Log($"[BG] Applied background for: '{country}'");
            Debug.Log($"[BG] Applied background for: '{bg.name}'");
        }
        else
        {
            Debug.LogWarning($"[BG] No background found for '{country}' — using default.");
            ApplyFallback();
        }
    }

    void ApplyFallback()
    {
        if (terrainImage != null && defaultBackground != null)
            terrainImage.sprite = defaultBackground;
    }
}