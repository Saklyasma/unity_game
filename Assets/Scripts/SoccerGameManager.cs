using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SoccerGameManager : MonoBehaviour
{
    public static SoccerGameManager Instance { get; private set; }
    public enum GoalOwner { Player, Bot }

    // ── Win condition ──────────────────────────────────────────────────────
    private const int WIN_SCORE = 5;   // ← change here to adjust match length

    public bool IsPlaying { get; private set; } = true;
    public int PlayerScore { get; private set; }
    public int BotScore { get; private set; }

    // ── Persistent super-shoot flags ───────────────────────────────────────
    public bool PlayerSuperShootEarned { get; private set; }
    public bool BotSuperShootEarned { get; private set; }

    [Header("== UI ==")]
    public TextMeshProUGUI scoreText;

    [Header("== OPPONENT ==")]
    public TextMeshProUGUI opponentNameText;
    public Image opponentFlagImage;

    [Header("== BALL ==")]
    public GameObject ball;

    [Header("== GOAL DETECTORS ==")]
    public GoalDetector goalDetector1;   // GoalZone1 — bot  scores here
    public GoalDetector goalDetector2;   // GoalZone2 — player scores here

    [Header("== PLAYERS ==")]
    public BotAIController botAI;
    public Rigidbody2D playerRb;
    [SerializeField] private PlayerUIController playerUIController;

    [Header("== MATCH END POPUP ==")]
    [SerializeField] private MatchEndPopup matchEndPopup;   // drag MatchEndPopup GO here

    private BallController _ballController;
    private Vector2 _playerSpawnPos;
    private GoalOwner _lastGoalOwner;

    private enum QuizSource { Goal, Bubble }
    private QuizSource _quizSource = QuizSource.Goal;

    // ── Unity lifecycle ────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (ball != null)
            _ballController = ball.GetComponent<BallController>();

        if (_ballController == null)
            Debug.LogError("[GameManager] BallController not found on Ball!");

        if (playerUIController == null)
        {
            playerUIController = FindObjectOfType<PlayerUIController>();
            if (playerUIController == null)
                Debug.LogWarning("[GameManager] PlayerUIController not found.");
        }

        // Auto-find MatchEndPopup if not assigned in Inspector
        if (matchEndPopup == null)
            matchEndPopup = FindObjectOfType<MatchEndPopup>();

        if (playerRb != null)
            _playerSpawnPos = playerRb.position;

        AutoFindScoreText();
        AutoFindOpponentUI();
        UpdateScoreUI();
        DisplayOpponentInfo();
    }

    // ── Public API ─────────────────────────────────────────────────────────

    public void OnGoalScored(GoalOwner scorer)
    {
        if (!IsPlaying) return;
        IsPlaying = false;

        _quizSource = QuizSource.Goal;
        _lastGoalOwner = scorer;

        Debug.Log($"[GameManager] OnGoalScored — scorer={scorer}");
        FreezeGame();
        ShowQuiz();
    }

    /// <summary>Own goal: no quiz, no score change — reset positions silently.</summary>
    public void OnOwnGoal()
    {
        if (!IsPlaying) return;
        IsPlaying = false;

        Debug.Log("[GameManager] Own goal — silent reset, no score change.");
        ResetRound();
    }

    public void OnBubbleQuizTriggered()
    {
        if (!IsPlaying) return;
        IsPlaying = false;

        _quizSource = QuizSource.Bubble;
        Debug.Log("[GameManager] OnBubbleQuizTriggered");
        FreezeGame();
        ShowQuiz();
    }

    public void OnQuizResult(bool answeredCorrectly)
    {
        Debug.Log($"[GameManager] OnQuizResult — correct={answeredCorrectly}  " +
                  $"source={_quizSource}  owner={_lastGoalOwner}");

        if (_quizSource == QuizSource.Bubble)
            ResumeBubbleQuiz(answeredCorrectly);
        else
            ResolveGoalQuiz(answeredCorrectly);
    }

    /// <summary>
    /// Called by BotAIController and PlayerUIController at the exact moment
    /// the super kick force is applied — NOT at grant time.
    /// </summary>
    public void OnSuperShootFired()
    {
        Debug.Log("[GameManager] Super shoot fired → ball super mode ON.");
        _ballController?.TryActivateSuperMode();
    }

    // ── Persistent super-shoot API ─────────────────────────────────────────

    public void EarnPlayerSuperShoot()
    {
        PlayerSuperShootEarned = true;
        Debug.Log("[GameManager] PlayerSuperShootEarned = true (persists until NewMatch)");
    }

    public void EarnBotSuperShoot()
    {
        BotSuperShootEarned = true;
        Debug.Log("[GameManager] BotSuperShootEarned = true (persists until NewMatch)");
    }

    /// <summary>Call at true game-over or scene reload to wipe all earned states.</summary>
    public void NewMatch()
    {
        PlayerSuperShootEarned = false;
        BotSuperShootEarned = false;
        PlayerScore = 0;
        BotScore = 0;
        UpdateScoreUI();
        Debug.Log("[GameManager] NewMatch — all super shoot flags cleared.");
    }

    // ── Freeze / Unfreeze ──────────────────────────────────────────────────

    private void FreezeGame()
    {
        _ballController?.SetKinematic(true);
        Time.timeScale = 0f;
    }

    private void UnfreezeGame()
    {
        _ballController?.SetKinematic(false);
        Time.timeScale = 1f;
    }

    private void ShowQuiz()
    {
        if (QuizManager.Instance == null)
        {
            Debug.LogError("[GameManager] QuizManager.Instance is null!");
            OnQuizResult(false);
            return;
        }
        QuizManager.Instance.ShowQuiz();
    }

    // ── Scoring matrix ─────────────────────────────────────────────────────

    private void ResolveGoalQuiz(bool correct)
    {
        int playerDelta = 0, botDelta = 0;

        switch (_lastGoalOwner)
        {
            case GoalOwner.Player:
                if (correct)
                {
                    playerDelta = 1;
                    Debug.Log("[GameManager] Player scored + correct → Player +1");
                }
                else
                    Debug.Log("[GameManager] Player scored + wrong → no point");
                break;

            case GoalOwner.Bot:
                if (!correct)
                {
                    botDelta = 1;
                    Debug.Log("[GameManager] Bot scored + wrong → Bot +1");
                }
                else
                    Debug.Log("[GameManager] Bot scored + correct → blocked, no point");
                break;
        }

        ApplyScoreAndReset(playerDelta, botDelta);
    }

    // ── Bubble quiz ────────────────────────────────────────────────────────

    private void ResumeBubbleQuiz(bool correct)
    {
        UnfreezeGame();
        IsPlaying = true;

        if (correct && playerUIController != null)
        {
            EarnPlayerSuperShoot();
            playerUIController.GrantSuperShoot();
            Debug.Log("[GameManager] Bubble correct → GrantSuperShoot() granted.");
        }
        else
            Debug.Log("[GameManager] Bubble wrong → resume, no reward");
    }

    // ── Score & round reset ────────────────────────────────────────────────

    private void ApplyScoreAndReset(int playerDelta, int botDelta)
    {
        PlayerScore += playerDelta;
        BotScore += botDelta;

        Debug.Log($"[GameManager] Scores → Player:{PlayerScore}  Bot:{BotScore}");
        UpdateScoreUI();

        // ── Check win condition BEFORE resetting the round ─────────────────
        if (PlayerScore >= WIN_SCORE)
        {
            EndMatch(playerWon: true);
            return;   // Do NOT reset the round — match is over
        }

        if (BotScore >= WIN_SCORE)
        {
            EndMatch(playerWon: false);
            return;
        }

        ResetRound();
    }

    /// <summary>
    /// Called when either side reaches WIN_SCORE.
    /// Freezes the game, notifies ProgressionManager on win, shows popup.
    /// </summary>
    private void EndMatch(bool playerWon)
    {
        IsPlaying = false;
        FreezeGame();

        Debug.Log($"[GameManager] Match ended — playerWon={playerWon}");

        // ── Unlock next country/region on player win ──────────────────────
        if (playerWon && ProgressionManager.Instance != null)
        {
            int wonIndex = PlayerPrefs.GetInt("OpponentIndex", -1);
            if (wonIndex >= 0)
            {
                ProgressionManager.Instance.OnMatchWin(wonIndex);
                Debug.Log($"[GameManager] Reported win for country index {wonIndex}.");
            }
            else
            {
                Debug.LogWarning("[GameManager] OpponentIndex not found in PlayerPrefs.");
            }
        }

        if (matchEndPopup != null)
            matchEndPopup.ShowPopup(playerWon);
        else
            Debug.LogError("[GameManager] matchEndPopup is null! Assign it in the Inspector.");
    }

    private void ResetRound()
    {
        _ballController?.ResetBall();
        ResetPlayers();

        goalDetector1?.ResetTrigger();
        goalDetector2?.ResetTrigger();

        RestoreEarnedSuperShoots();

        UnfreezeGame();
        _quizSource = QuizSource.Goal;
        IsPlaying = true;
        Debug.Log("[GameManager] Round reset — play resumed.");
    }

    private void RestoreEarnedSuperShoots()
    {
        if (PlayerSuperShootEarned && playerUIController != null)
        {
            playerUIController.RestoreSuperShoot();
            Debug.Log("[GameManager] Restored player super shoot after reset.");
        }

        if (BotSuperShootEarned && botAI != null)
        {
            botAI.RestoreSuperShoot();
            Debug.Log("[GameManager] Restored bot super shoot after reset.");
        }
    }

    private void ResetPlayers()
    {
        botAI?.ResetToSpawn();

        if (playerUIController != null)
            playerUIController.ResetPlayer();
        else if (playerRb != null)
        {
            playerRb.position = _playerSpawnPos;
            playerRb.velocity = Vector2.zero;
            playerRb.angularVelocity = 0f;
        }
    }

    // ── UI ─────────────────────────────────────────────────────────────────

    private void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = $"{PlayerScore} - {BotScore}";
        else
            Debug.LogWarning("[GameManager] scoreText not assigned.");
    }

    private void DisplayOpponentInfo()
    {
        CountryFlag selected = CountryDataHolder.Instance?.GetSelected();

        if (selected != null)
        {
            if (opponentNameText != null) opponentNameText.text = selected.countryName;
            if (opponentFlagImage != null) opponentFlagImage.sprite = selected.flag;
            return;
        }

        string fallback = PlayerPrefs.GetString("OpponentCountry", "Opponent");
        if (opponentNameText != null) opponentNameText.text = fallback;
        Debug.LogWarning($"[GameManager] CountryDataHolder missing — fallback: {fallback}");
    }

    private void AutoFindOpponentUI()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;

        if (opponentNameText == null)
            foreach (var t in canvas.GetComponentsInChildren<TextMeshProUGUI>(true))
                if (t.gameObject.name.ToLower().Contains("opponent"))
                { opponentNameText = t; break; }

        if (opponentFlagImage == null)
            foreach (var img in canvas.GetComponentsInChildren<Image>(true))
                if (img.gameObject.name.ToLower().Contains("opponent"))
                { opponentFlagImage = img; break; }
    }

    private void AutoFindScoreText()
    {
        if (scoreText != null) return;

        GameObject obj = GameObject.Find("Score Text");
        if (obj != null) scoreText = obj.GetComponent<TextMeshProUGUI>();

        if (scoreText == null)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
                foreach (var t in canvas.GetComponentsInChildren<TextMeshProUGUI>(true))
                    if (t.gameObject.name.ToLower().Contains("score"))
                    { scoreText = t; break; }
        }

        if (scoreText == null) Debug.LogError("[GameManager] ScoreText not found!");
    }
}