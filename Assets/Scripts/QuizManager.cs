using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;
using RTLTMPro;

/// <summary>
/// QuizManager
/// ───────────
/// Owns the quiz panel UI only.
/// Has zero knowledge of scores or game state.
///
/// Contract:
///   GameManager calls ShowQuiz()         → quiz appears.
///   Player submits                        → calls GameManager.OnQuizResult(bool).
/// </summary>
public class QuizManager : MonoBehaviour
{
    public static QuizManager Instance { get; private set; }

    // ── Inspector refs ─────────────────────────────────────────────────────

    [Header("== PANEL ==")]
    public GameObject quizPanel;

    [Header("== UI ==")]
    public TextMeshProUGUI questionText;
    public Button[] answerButtons;
    public TextMeshProUGUI[] answerTexts;
    public Button applyButton;
    [Tooltip("Optional replay button used to play the current question voice.")]
    public Button playVoiceButton;
    [Tooltip("Optional helper/status label under the question or button area.")]
    public TextMeshProUGUI statusText;
    [Tooltip("Optional root CanvasGroup used to fade/disable the modal card.")]
    public CanvasGroup modalCanvasGroup;
    [Tooltip("Optional root transform of the modal card to animate. Falls back to quizPanel transform.")]
    public RectTransform modalRoot;
    [Tooltip("Optional backdrop CanvasGroup to fade in/out with the modal.")]
    public CanvasGroup backdropCanvasGroup;

    [Header("== COLORS ==")]
    public Color normalColor = Color.white;
    public Color selectedColor = new Color(1f, 0.92f, 0.45f, 1f);
    public Color correctColor = new Color(0.39f, 0.83f, 0.49f, 1f);
    public Color wrongColor = new Color(0.94f, 0.38f, 0.38f, 1f);
    [Tooltip("Subtle dim color for disabled answer slots or idle secondary elements.")]
    public Color disabledColor = new Color(0.9f, 0.9f, 0.9f, 0.72f);

    [Header("== LABELS ==")]
    [SerializeField] private string selectAnswerPrompt = "Select one answer";
    [SerializeField] private string submitPrompt = "Tap Confirm to lock your answer";
    [SerializeField] private string correctFeedback = "Correct!";
    [SerializeField] private string wrongFeedback = "Wrong answer";

    [Header("== AUDIO ==")]
    [Tooltip("Optional dedicated audio source for quiz voice playback. If missing, one is created automatically.")]
    public AudioSource questionVoiceSource;
    [SerializeField] private bool autoPlayQuestionVoice = true;
    [SerializeField] private float replayButtonCooldown = 0.15f;
    [SerializeField] private string playVoicePrompt = "Tap the speaker to hear the question";
    [SerializeField] private string noVoicePrompt = "No voice available for this question";

    [Header("== DATA SOURCE ==")]
    [SerializeField] private string quizDataResourcePath = "QuizData";
    [SerializeField] private string fallbackLanguage = "ar";

    [Header("== ANIMATION ==")]
    [SerializeField] private float openDuration = 0.2f;
    [SerializeField] private float closeDuration = 0.16f;
    [SerializeField] private float openStartScale = 0.9f;
    [SerializeField] private float closeEndScale = 0.96f;
    [SerializeField] private float selectedScale = 1.03f;
    [SerializeField] private float idleScale = 1f;

    // ── Question definition ────────────────────────────────────────────────

    [System.Serializable]
    public struct QuizQuestion
    {
        public string question;
        public string[] answers;
        public int correctIndex;
        public AudioClip voiceClip;
    }

    [Serializable]
    private class QuizDatabase
    {
        public CountryQuizData[] countries;
    }

    [Serializable]
    private class CountryQuizData
    {
        public string countryId;
        public string countryName;
        public LanguageQuizSet languages;
    }

    [Serializable]
    private class LanguageQuizSet
    {
        public QuizLanguageBlock ar;
        public QuizLanguageBlock en;
        public QuizLanguageBlock fr;
    }

    [Serializable]
    private class QuizLanguageBlock
    {
        public QuizEntry[] questions;
    }

    [Serializable]
    private class QuizEntry
    {
        public int id;
        public string question;
        public string[] answers;
        public int correctIndex;
        public string audioResource;
    }

    // ── Data ───────────────────────────────────────────────────────────────

    private QuizQuestion[] _questions;
    private List<int> _pool = new List<int>();

    // ── Round state ────────────────────────────────────────────────────────

    private int _correctIndex = -1;
    private int _selectedIndex = -1;
    private bool _awaitingSubmission = false;
    private Coroutine _panelAnimationCoroutine;
    private Coroutine _buttonPulseCoroutine;
    private Vector3 _modalBaseScale = Vector3.one;
    private Vector3[] _buttonBaseScales;
    private QuizQuestion _currentQuestion;
    private float _nextReplayAllowedTime;

    // ── Unity lifecycle ────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        LoadQuestionsFromJson();
        if (quizPanel != null) quizPanel.SetActive(false);

        if (modalRoot == null && quizPanel != null)
            modalRoot = quizPanel.transform as RectTransform;
        if (modalRoot != null)
            _modalBaseScale = modalRoot.localScale;

        CacheButtonScales();
        EnsureQuestionVoiceSource();
        SetStatus(selectAnswerPrompt, normalColor);
    }

    // ── Public API ─────────────────────────────────────────────────────────

    /// <summary>
    /// Opens the quiz panel with a randomly-selected, non-repeating question.
    /// Called by GameManager on goal or bubble pickup.
    /// </summary>
    public void ShowQuiz()
    {
        if (_questions == null || _questions.Length == 0)
        {
            Debug.LogError("[QuizManager] No questions defined!");
            SoccerGameManager.Instance?.OnQuizResult(true); // avoid deadlock
            return;
        }

        _selectedIndex = -1;
        _awaitingSubmission = true;

        CacheButtonScales();
        EnsureUIReferences();

        QuizQuestion q = _questions[PickRandom()];
        _currentQuestion = q;
        _correctIndex = q.correctIndex;

        if (questionText == null)
        {
            Debug.LogError("[QuizManager] questionText is not assigned and could not be auto-found.");
            SoccerGameManager.Instance?.OnQuizResult(true);
            return;
        }

        SetLocalizedText(questionText, q.question);
        SetStatus(q.voiceClip != null ? playVoicePrompt : selectAnswerPrompt, normalColor);

        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (answerButtons[i] == null) continue;

            bool hasAnswer = i < q.answers.Length;
            answerButtons[i].gameObject.SetActive(hasAnswer);

            if (!hasAnswer)
            {
                if (i < answerTexts.Length && answerTexts[i] != null)
                    SetLocalizedText(answerTexts[i], string.Empty);
                answerButtons[i].onClick.RemoveAllListeners();
                answerButtons[i].interactable = false;
                continue;
            }

            answerButtons[i].image.color = normalColor;
            answerButtons[i].onClick.RemoveAllListeners();
            SetButtonScale(i, idleScale);
            SetAnswerVisualState(i, selected: false, disabled: false);

            if (i < answerTexts.Length && answerTexts[i] != null)
                SetLocalizedText(answerTexts[i], q.answers[i]);

            int captured = i;
            answerButtons[i].onClick.AddListener(() => SelectAnswer(captured));
            answerButtons[i].interactable = true;
        }

        if (applyButton != null)
        {
            applyButton.onClick.RemoveAllListeners();
            applyButton.onClick.AddListener(Submit);
            applyButton.interactable = false;
        }

        ConfigurePlayVoiceButton();

        if (modalCanvasGroup != null)
        {
            modalCanvasGroup.alpha = 1f;
            modalCanvasGroup.interactable = true;
            modalCanvasGroup.blocksRaycasts = true;
        }

        quizPanel.SetActive(true);
        PlayPanelAnimation(opening: true);

        int firstSelectable = GetFirstSelectableAnswerIndex();
        if (firstSelectable >= 0)
            EventSystem.current?.SetSelectedGameObject(answerButtons[firstSelectable].gameObject);
    }

    // ── Private ────────────────────────────────────────────────────────────

    private void SelectAnswer(int index)
    {
        if (!_awaitingSubmission) return;

        _selectedIndex = index;
        if (applyButton != null) applyButton.interactable = true;
        SetStatus(submitPrompt, selectedColor);

        for (int i = 0; i < answerButtons.Length; i++)
        {
            bool isSelected = (i == index);
            if (answerButtons[i] == null || !answerButtons[i].interactable) continue;
            answerButtons[i].image.color = isSelected ? selectedColor : normalColor;
            SetButtonScale(i, isSelected ? selectedScale : idleScale);
            SetAnswerVisualState(i, selected: isSelected, disabled: false);
        }

        if (applyButton != null)
            EventSystem.current?.SetSelectedGameObject(applyButton.gameObject);
    }

    private void Submit()
    {
        if (!_awaitingSubmission) return;
        if (_selectedIndex == -1)
        {
            Debug.Log("[QuizManager] No answer selected.");
            SetStatus(selectAnswerPrompt, wrongColor);
            return;
        }

        _awaitingSubmission = false;
        StopQuestionVoice();

        foreach (Button b in answerButtons)
            if (b != null) b.interactable = false;
        if (applyButton != null) applyButton.interactable = false;

        bool correct = (_selectedIndex == _correctIndex);
        SetStatus(correct ? correctFeedback : wrongFeedback, correct ? correctColor : wrongColor);

        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (answerButtons[i] == null) continue;

            bool isCorrect = i == _correctIndex;
            bool isSelected = i == _selectedIndex;

            if (isCorrect) answerButtons[i].image.color = correctColor;
            else if (isSelected) answerButtons[i].image.color = wrongColor;
            else if (answerButtons[i].gameObject.activeInHierarchy) answerButtons[i].image.color = disabledColor;

            SetButtonScale(i, isCorrect ? selectedScale : idleScale);
            SetAnswerVisualState(i, selected: isCorrect || isSelected, disabled: !isCorrect && !isSelected);
        }

        if (_buttonPulseCoroutine != null)
            StopCoroutine(_buttonPulseCoroutine);
        _buttonPulseCoroutine = StartCoroutine(PulseHighlightedAnswer(_correctIndex));

        StartCoroutine(CloseAfterDelay(1.2f, correct));
    }

    private IEnumerator CloseAfterDelay(float delay, bool correct)
    {
        // WaitForSecondsRealtime because Time.timeScale == 0 during quiz
        yield return new WaitForSecondsRealtime(delay);

        foreach (Button b in answerButtons)
            if (b != null) b.interactable = true;
        if (applyButton != null) applyButton.interactable = true;

        if (_buttonPulseCoroutine != null)
        {
            StopCoroutine(_buttonPulseCoroutine);
            _buttonPulseCoroutine = null;
        }

        StopQuestionVoice();

        if (quizPanel != null && quizPanel.activeSelf)
        {
            yield return PlayPanelAnimationAndWait(opening: false);
            quizPanel.SetActive(false);
        }

        SetStatus(selectAnswerPrompt, normalColor);

        SoccerGameManager.Instance?.OnQuizResult(correct);
    }

    private void PlayPanelAnimation(bool opening)
    {
        if (_panelAnimationCoroutine != null)
            StopCoroutine(_panelAnimationCoroutine);
        _panelAnimationCoroutine = StartCoroutine(AnimatePanel(opening));
    }

    private IEnumerator PlayPanelAnimationAndWait(bool opening)
    {
        if (_panelAnimationCoroutine != null)
            StopCoroutine(_panelAnimationCoroutine);
        yield return AnimatePanel(opening);
        _panelAnimationCoroutine = null;
    }

    private IEnumerator AnimatePanel(bool opening)
    {
        float duration = Mathf.Max(0.01f, opening ? openDuration : closeDuration);
        float elapsed = 0f;

        Vector3 fromScale = opening ? _modalBaseScale * openStartScale : _modalBaseScale;
        Vector3 toScale = opening ? _modalBaseScale : _modalBaseScale * closeEndScale;

        float fromAlpha = opening ? 0f : 1f;
        float toAlpha = opening ? 1f : 0f;

        if (modalRoot != null && opening)
            modalRoot.localScale = fromScale;
        if (backdropCanvasGroup != null)
            backdropCanvasGroup.alpha = fromAlpha;
        if (modalCanvasGroup != null)
            modalCanvasGroup.alpha = opening ? 0f : 1f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = 1f - Mathf.Pow(1f - t, 3f); // easeOutCubic

            if (modalRoot != null)
                modalRoot.localScale = Vector3.LerpUnclamped(fromScale, toScale, eased);
            if (backdropCanvasGroup != null)
                backdropCanvasGroup.alpha = Mathf.LerpUnclamped(fromAlpha, toAlpha, eased);
            if (modalCanvasGroup != null)
                modalCanvasGroup.alpha = Mathf.LerpUnclamped(opening ? 0f : 1f, opening ? 1f : 0f, eased);

            yield return null;
        }

        if (modalRoot != null)
            modalRoot.localScale = toScale;
        if (backdropCanvasGroup != null)
            backdropCanvasGroup.alpha = toAlpha;
        if (modalCanvasGroup != null)
            modalCanvasGroup.alpha = opening ? 1f : 0f;
    }

    private void Update()
    {
        if (!_awaitingSubmission || !quizPanel || !quizPanel.activeInHierarchy)
            return;

        if (_selectedIndex >= 0 && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space)))
            Submit();

        if (playVoiceButton != null && playVoiceButton.interactable && Input.GetKeyDown(KeyCode.V))
            PlayCurrentQuestionVoice();
    }

    private void OnDisable()
    {
        StopQuestionVoice();
    }

    private void EnsureUIReferences()
    {
        if (quizPanel == null) return;

        if (questionText == null)
            questionText = quizPanel.GetComponentInChildren<TextMeshProUGUI>(true);
    }

    private void CacheButtonScales()
    {
        if (answerButtons == null) return;

        if (_buttonBaseScales == null || _buttonBaseScales.Length != answerButtons.Length)
            _buttonBaseScales = new Vector3[answerButtons.Length];

        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (answerButtons[i] == null) continue;
            _buttonBaseScales[i] = answerButtons[i].transform.localScale;
        }
    }

    private void SetButtonScale(int index, float multiplier)
    {
        if (_buttonBaseScales == null || index < 0 || index >= _buttonBaseScales.Length) return;
        if (answerButtons[index] == null) return;
        answerButtons[index].transform.localScale = _buttonBaseScales[index] * multiplier;
    }

    private void SetAnswerVisualState(int index, bool selected, bool disabled)
    {
        if (index < 0 || index >= answerButtons.Length || answerButtons[index] == null)
            return;

        var label = index < answerTexts.Length ? answerTexts[index] : null;
        if (label != null)
        {
            label.fontStyle = selected ? FontStyles.Bold : FontStyles.Normal;
            label.alpha = disabled ? 0.75f : 1f;
        }
    }

    private void SetStatus(string message, Color color)
    {
        if (statusText == null) return;
        SetLocalizedText(statusText, message);
        statusText.color = color;
    }

    private void SetLocalizedText(TextMeshProUGUI target, string value)
    {
        if (target == null) return;

        if (target is RTLTextMeshPro rtlText)
            rtlText.text = value;
        else
            target.text = value;
    }

    private void EnsureQuestionVoiceSource()
    {
        if (questionVoiceSource != null)
        {
            questionVoiceSource.playOnAwake = false;
            questionVoiceSource.loop = false;
            questionVoiceSource.ignoreListenerPause = true;
            return;
        }

        questionVoiceSource = GetComponent<AudioSource>();
        if (questionVoiceSource == null)
            questionVoiceSource = gameObject.AddComponent<AudioSource>();

        questionVoiceSource.playOnAwake = false;
        questionVoiceSource.loop = false;
        questionVoiceSource.ignoreListenerPause = true;
    }

    private void ConfigurePlayVoiceButton()
    {
        if (playVoiceButton == null) return;

        playVoiceButton.onClick.RemoveAllListeners();
        playVoiceButton.onClick.AddListener(PlayCurrentQuestionVoice);
        playVoiceButton.interactable = _currentQuestion.voiceClip != null;
    }

    private void PlayCurrentQuestionVoice()
    {
        if (_currentQuestion.voiceClip == null)
        {
            SetStatus(noVoicePrompt, disabledColor);
            return;
        }

        if (Time.unscaledTime < _nextReplayAllowedTime)
            return;

        EnsureQuestionVoiceSource();
        _nextReplayAllowedTime = Time.unscaledTime + replayButtonCooldown;

        questionVoiceSource.Stop();
        questionVoiceSource.clip = _currentQuestion.voiceClip;
        questionVoiceSource.Play();
        SetStatus(playVoicePrompt, normalColor);
    }

    private void StopQuestionVoice()
    {
        if (questionVoiceSource != null && questionVoiceSource.isPlaying)
            questionVoiceSource.Stop();
    }

    private int GetFirstSelectableAnswerIndex()
    {
        for (int i = 0; i < answerButtons.Length; i++)
            if (answerButtons[i] != null && answerButtons[i].interactable)
                return i;
        return -1;
    }

    private IEnumerator PulseHighlightedAnswer(int index)
    {
        if (index < 0 || index >= answerButtons.Length || answerButtons[index] == null)
            yield break;

        float duration = 0.7f;
        float elapsed = 0f;
        Vector3 from = _buttonBaseScales[index] * selectedScale;
        Vector3 to = _buttonBaseScales[index] * (selectedScale + 0.04f);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float wave = (Mathf.Sin(elapsed * 14f) + 1f) * 0.5f;
            answerButtons[index].transform.localScale = Vector3.LerpUnclamped(from, to, wave);
            yield return null;
        }

        SetButtonScale(index, selectedScale);
    }

    // ── Question pool (no-repeat shuffle) ──────────────────────────────────

    private void LoadQuestionsFromJson()
    {
        TextAsset jsonAsset = Resources.Load<TextAsset>(quizDataResourcePath);
        if (jsonAsset == null)
        {
            Debug.LogWarning($"[QuizManager] Quiz JSON not found at Resources/{quizDataResourcePath}.json");
            _questions = Array.Empty<QuizQuestion>();
            return;
        }

        QuizDatabase database;
        try
        {
            database = JsonUtility.FromJson<QuizDatabase>(jsonAsset.text);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[QuizManager] Failed to parse quiz JSON: {ex.Message}");
            _questions = Array.Empty<QuizQuestion>();
            return;
        }

        if (database == null || database.countries == null || database.countries.Length == 0)
        {
            Debug.LogWarning("[QuizManager] Quiz JSON loaded but contains no countries.");
            _questions = Array.Empty<QuizQuestion>();
            return;
        }

        string selectedCountry = PlayerPrefs.GetString("OpponentCountry", string.Empty);
        string selectedLanguage = ResolveCurrentLanguage();

        CountryQuizData countryData = FindCountry(database.countries, selectedCountry)
            ?? database.countries[0];

        QuizLanguageBlock languageBlock = GetLanguageBlock(countryData, selectedLanguage)
            ?? GetLanguageBlock(countryData, fallbackLanguage);

        if (languageBlock == null || languageBlock.questions == null || languageBlock.questions.Length == 0)
        {
            Debug.LogWarning($"[QuizManager] No quiz questions found for country '{countryData.countryName}' and language '{selectedLanguage}'.");
            _questions = Array.Empty<QuizQuestion>();
            return;
        }

        List<QuizQuestion> loadedQuestions = new List<QuizQuestion>(languageBlock.questions.Length);
        foreach (QuizEntry entry in languageBlock.questions)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.question) || entry.answers == null || entry.answers.Length == 0)
                continue;

            if (entry.correctIndex < 0 || entry.correctIndex >= entry.answers.Length)
                continue;

            loadedQuestions.Add(new QuizQuestion
            {
                question = entry.question,
                answers = entry.answers,
                correctIndex = entry.correctIndex,
                voiceClip = LoadVoiceClip(entry.audioResource)
            });
        }

        _questions = loadedQuestions.ToArray();
        RefillPool();
    }

    private string ResolveCurrentLanguage()
    {
        string language = PlayerPrefs.GetString("Language", fallbackLanguage);
        if (string.IsNullOrWhiteSpace(language))
            language = fallbackLanguage;
        return language.Trim().ToLowerInvariant();
    }

    private CountryQuizData FindCountry(CountryQuizData[] countries, string selectedCountry)
    {
        if (countries == null || countries.Length == 0)
            return null;

        if (string.IsNullOrWhiteSpace(selectedCountry))
            return null;

        for (int i = 0; i < countries.Length; i++)
        {
            CountryQuizData country = countries[i];
            if (country == null) continue;

            if (string.Equals(country.countryName, selectedCountry, StringComparison.OrdinalIgnoreCase)
                || string.Equals(country.countryId, selectedCountry, StringComparison.OrdinalIgnoreCase))
                return country;
        }

        return null;
    }

    private QuizLanguageBlock GetLanguageBlock(CountryQuizData countryData, string language)
    {
        if (countryData == null || countryData.languages == null)
            return null;

        switch ((language ?? string.Empty).ToLowerInvariant())
        {
            case "ar": return countryData.languages.ar;
            case "fr": return countryData.languages.fr;
            case "en": return countryData.languages.en;
            default: return null;
        }
    }

    private AudioClip LoadVoiceClip(string audioResource)
    {
        if (string.IsNullOrWhiteSpace(audioResource))
            return null;

        return Resources.Load<AudioClip>(audioResource);
    }

    private void RefillPool()
    {
        _pool.Clear();
        for (int i = 0; i < _questions.Length; i++) _pool.Add(i);
    }

    private int PickRandom()
    {
        if (_pool.Count == 0) RefillPool();
        int slot = UnityEngine.Random.Range(0, _pool.Count);
        int index = _pool[slot];
        _pool.RemoveAt(slot);
        return index;
    }
}
