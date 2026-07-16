using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Attach this to the MatchEndPopup GameObject in the Canvas.
/// Wire up all references in the Inspector.
/// ShowPopup() is called by SoccerGameManager when a player reaches WIN_SCORE.
/// </summary>
public class MatchEndPopup : MonoBehaviour
{
    public static MatchEndPopup Instance { get; private set; }

    [Header("== Panel ==")]
    [SerializeField] private GameObject popupPanel;

    [Header("== Result Text ==")]
    [SerializeField] private TextMeshProUGUI resultText;   // "You are Winner!" / "You are Loser!"

    [Header("== Buttons ==")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button nextMatchButton;
    [SerializeField] private Button returnToMapButton;

    [Header("== Scene Names ==")]
    [SerializeField] private string matchSceneName = "Dlc_2_Level[AR] 0";   // same scene restart
    [SerializeField] private string nextSceneName = "Dlc_2_Level[AR] 0";   // next opponent scene
    [SerializeField] private string mapSceneName = "Dlc_1_Map[AR]";     // world-map scene

    // ── Unity lifecycle ────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // Safety: hide at startup regardless of Editor state
        if (popupPanel != null) popupPanel.SetActive(false);
    }

    private void Start()
    {
        // Wire buttons (safe even if already wired via Inspector)
        if (restartButton != null) restartButton.onClick.AddListener(OnRestart);
        if (nextMatchButton != null) nextMatchButton.onClick.AddListener(OnNextMatch);
        if (returnToMapButton != null) returnToMapButton.onClick.AddListener(OnReturnToMap);
    }

    // ── Public API ─────────────────────────────────────────────────────────

    /// <summary>
    /// Call this from SoccerGameManager when the match ends.
    /// playerWon = true  → "You are Winner!"
    /// playerWon = false → "You are Loser!"
    /// </summary>
    public void ShowPopup(bool playerWon)
    {
        if (resultText != null)
            resultText.text = playerWon ? "You are Winner!" : "You are Loser!";

        if (popupPanel != null) popupPanel.SetActive(true);

        // ── Show/hide buttons based on win/loss ─────────────────────────
        bool hasNext = ProgressionManager.Instance != null &&
                       ProgressionManager.Instance.GetNextPlayableCountry() >= 0;
        if (nextMatchButton != null)
            nextMatchButton.gameObject.SetActive(playerWon && hasNext);
        if (restartButton != null)
            restartButton.gameObject.SetActive(!playerWon);

        // Game is already frozen by SoccerGameManager.FreezeGame()
        // We freeze again here as a safety net in case ShowPopup is called standalone
        Time.timeScale = 0f;

        Debug.Log($"[MatchEndPopup] Shown — playerWon={playerWon}, hasNext={hasNext}");
    }

    // ── Button handlers ────────────────────────────────────────────────────

    private void OnRestart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(matchSceneName);
    }

    private void OnNextMatch()
    {
        Time.timeScale = 1f;

        // ── Advance to next playable country BEFORE loading ────────────
        if (ProgressionManager.Instance != null)
        {
            int nextIdx = ProgressionManager.Instance.GetNextPlayableCountry();
            if (nextIdx >= 0)
            {
                string countryName = $"Country_{nextIdx}";
                if (CountryDataHolder.Instance != null &&
                    CountryDataHolder.Instance.Countries != null &&
                    nextIdx < CountryDataHolder.Instance.Countries.Count)
                {
                    countryName = CountryDataHolder.Instance.Countries[nextIdx].countryName;
                }
                PlayerPrefs.SetInt("OpponentIndex", nextIdx);
                PlayerPrefs.SetString("OpponentCountry", countryName);
                PlayerPrefs.Save();
                Debug.Log($"[MatchEndPopup] Advanced to next country: index={nextIdx}, name={countryName}");
            }
        }

        SceneManager.LoadScene(nextSceneName);
    }

    private void OnReturnToMap()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mapSceneName);
    }
}