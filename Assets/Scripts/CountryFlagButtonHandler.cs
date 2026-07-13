using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;

public class CountryFlagButtonHandler : MonoBehaviour
{
    [Header("UI References")]
    public List<Button> buttons;
    public GameObject panel;
    public Image flagDisplay;
    public Text countryNameText;
    public Button playButton;

    [Header("Country Data")]
    public List<CountryFlag> countries;

    [Header("Lock Visuals (optional — one per button, same order)")]
    public List<GameObject> lockOverlays;

    private CountryFlag selectedCountry;
    private int selectedIndex = -1;
    private CanvasGroup[] _overlayGroups;

    // ── Unity lifecycle ───────────────────────────────────────────────────

    void Awake()
    {
        // Remove stray Button on the map GO itself
        var selfButton = GetComponent<Button>();
        if (selfButton != null)
        {
            Debug.LogWarning("[MapHandler] Removed stray Button from map GO.");
            Destroy(selfButton);
        }

        // Background image must never block clicks
        var selfImage = GetComponent<Image>();
        if (selfImage != null)
            selfImage.raycastTarget = false;
    }

    void Start()
    {
        if (panel == null) { Debug.LogError("[MapHandler] Panel not assigned!"); return; }

        CacheOverlayCanvasGroups();
        EnsureDataHolder();
        CountryDataHolder.Instance.SetCountries(countries);
        WireButtonListeners();

        if (playButton != null)
            playButton.onClick.AddListener(OnPlayClicked);

        panel.SetActive(false);
        RefreshAllLockStates();
        InitializeDefaultSelection();

        Debug.Log($"[MapHandler] Ready — {buttons.Count} buttons / " +
                  $"{countries.Count} countries / {lockOverlays?.Count ?? 0} overlays");

        if (ProgressionManager.Instance != null)
            for (int i = 0; i < buttons.Count; i++)
                Debug.Log($"[MapHandler] [{i}] '{GetCountryName(i)}' " +
                          $"unlocked={ProgressionManager.Instance.IsUnlocked(i)} " +
                          $"buttonNull={buttons[i] == null} " +
                          $"overlayNull={(_overlayGroups == null || i >= _overlayGroups.Length || _overlayGroups[i] == null)}");
    }

    void OnEnable() => RefreshAllLockStates();

    // ── Setup ─────────────────────────────────────────────────────────────

    void CacheOverlayCanvasGroups()
    {
        if (lockOverlays == null || lockOverlays.Count == 0) return;

        _overlayGroups = new CanvasGroup[lockOverlays.Count];

        for (int i = 0; i < lockOverlays.Count; i++)
        {
            if (lockOverlays[i] == null)
            {
                Debug.LogWarning($"[MapHandler] lockOverlays[{i}] is null!");
                continue;
            }

            _overlayGroups[i] = lockOverlays[i].GetComponent<CanvasGroup>()
                             ?? lockOverlays[i].AddComponent<CanvasGroup>();

            foreach (var img in lockOverlays[i].GetComponentsInChildren<Image>(true))
                img.raycastTarget = false;

            lockOverlays[i].SetActive(true);
            Debug.Log($"[MapHandler] Overlay[{i}] cached: '{lockOverlays[i].name}'");
        }
    }

    void WireButtonListeners()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            if (buttons[i] == null) continue;

            buttons[i].onClick.RemoveAllListeners();
            int idx = i;
            buttons[i].onClick.AddListener(() => OnCountrySelected(idx));

            var img = buttons[i].GetComponent<Image>();
            if (img != null) img.raycastTarget = true;

            buttons[i].gameObject.SetActive(true);
            buttons[i].interactable = true;

            var cg = buttons[i].GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 1f;
                cg.blocksRaycasts = true;
                cg.interactable = true;
                Debug.LogWarning($"[MapHandler] Button[{i}] has CanvasGroup — forced visible.");
            }
        }
    }

    // ── Lock states ───────────────────────────────────────────────────────

    void RefreshAllLockStates()
    {
        if (ProgressionManager.Instance == null) return;

        for (int i = 0; i < buttons.Count; i++)
        {
            if (buttons[i] == null) continue;

            buttons[i].gameObject.SetActive(true);
            buttons[i].interactable = true;

            var btnCg = buttons[i].GetComponent<CanvasGroup>();
            if (btnCg != null) { btnCg.alpha = 1f; btnCg.blocksRaycasts = true; }

            bool unlocked = ProgressionManager.Instance.IsUnlocked(i);

            if (_overlayGroups != null && i < _overlayGroups.Length && _overlayGroups[i] != null)
            {
                _overlayGroups[i].alpha = unlocked ? 0f : 1f;
                _overlayGroups[i].blocksRaycasts = !unlocked;
                _overlayGroups[i].interactable = false;
            }
            else if (lockOverlays != null && i < lockOverlays.Count && lockOverlays[i] != null)
            {
                lockOverlays[i].SetActive(!unlocked);
            }
        }
    }

    void InitializeDefaultSelection()
    {
        if (countries == null || countries.Count == 0)
            return;

        int defaultIndex;
        if (CountryDataHolder.Instance != null && CountryDataHolder.Instance.SelectedIndex >= 0)
        {
            defaultIndex = Mathf.Clamp(CountryDataHolder.Instance.SelectedIndex, 0, countries.Count - 1);
        }
        else
        {
            defaultIndex = PlayerPrefs.HasKey("OpponentIndex")
                ? Mathf.Clamp(PlayerPrefs.GetInt("OpponentIndex"), 0, countries.Count - 1)
                : 0;
        }

        selectedIndex = defaultIndex;
        selectedCountry = countries[defaultIndex];

        if (CountryDataHolder.Instance != null)
            CountryDataHolder.Instance.SetSelectedIndex(defaultIndex);
    }

    // ── Selection ─────────────────────────────────────────────────────────

    void OnCountrySelected(int index)
    {
        Debug.Log($"[MapHandler] Clicked country[{index}] '{GetCountryName(index)}'");

        if (index < 0 || index >= countries.Count) return;

        if (ProgressionManager.Instance != null &&
            !ProgressionManager.Instance.IsUnlocked(index))
        {
            Debug.LogWarning($"[MapHandler] Country[{index}] is locked.");
            return;
        }

        selectedIndex = index;
        selectedCountry = countries[index];

        if (CountryDataHolder.Instance != null)
        {
            CountryDataHolder.Instance.SetSelectedIndex(index);
            Debug.Log($"[MapHandler] OnCountrySelected: synced CountryDataHolder idx={index} ('{selectedCountry.countryName}')");
        }
        else
            Debug.LogError("[MapHandler] OnCountrySelected: CountryDataHolder.Instance is null!");

        panel.SetActive(true);
        flagDisplay.sprite = selectedCountry.flag;
        if (countryNameText != null)
            countryNameText.text = selectedCountry.countryName;
    }

    // ── Play ──────────────────────────────────────────────────────────────

    void OnPlayClicked()
    {
        if (selectedCountry == null)
        {
            Debug.LogError("[MapHandler] Play clicked but selectedCountry is null! User must click a country first.");
            return;
        }

        Debug.Log($"[MapHandler] OnPlayClicked: saving country='{selectedCountry.countryName}', index={selectedIndex}, flag={(selectedCountry.flag != null ? selectedCountry.flag.name : "null")}");

        PlayerPrefs.SetInt("OpponentIndex", selectedIndex);
        PlayerPrefs.SetString("OpponentCountry", selectedCountry.countryName);
        PlayerPrefs.Save();

        Debug.Log($"[MapHandler] Verified PlayerPrefs after save: OpponentCountry='{PlayerPrefs.GetString("OpponentCountry")}', OpponentIndex={PlayerPrefs.GetInt("OpponentIndex", -999)}");

        CountryDataHolder.Instance.SetSelectedIndex(selectedIndex);

        Debug.Log($"[MapHandler] Loading level for: '{selectedCountry.countryName}' (index {selectedIndex})");
        SceneManager.LoadScene("Dlc_2_Level[AR] 0");
    }

    public void ClosePanel() => panel.SetActive(false);

    void EnsureDataHolder()
    {
        if (CountryDataHolder.Instance != null) return;
        new GameObject("CountryDataHolder").AddComponent<CountryDataHolder>();
    }

    string GetCountryName(int i) =>
        countries != null && i < countries.Count && countries[i] != null
            ? countries[i].countryName : "?";
}

[Serializable]
public class CountryFlag
{
    public string countryName;
    public Sprite flag;
}
