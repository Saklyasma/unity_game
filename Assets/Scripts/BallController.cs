// ═══════════════════════════════════════════════════════════════════
// BallController.cs
//
// TRAIL CHANGES:
//   • Trail emits ONLY during super mode (ActivateSuperMode → ResetBall/timeout).
//   • DisableSuperEffects() now called in Awake so trail never emits at
//     scene start or after any reset — not just after Start().
//   • Trail color is set to superTrailColor on activation and restored
//     to its original color on deactivation / reset.
//   • superTrailColor is inspector-exposed so you can tweak it without
//     touching code.
//   • All previous logic (punch animation, LastToucher, etc.) unchanged.
// ═══════════════════════════════════════════════════════════════════
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class BallController : MonoBehaviour
{
    // ── Who last touched the ball ──────────────────────────────────────────
    public enum Toucher { None, Player, Bot }
    public Toucher LastToucher { get; private set; } = Toucher.None;

    [Header("== LAST TOUCH LAYERS ==")]
    [Tooltip("Layer name of the Player character")]
    public string playerLayerName = "Player";
    [Tooltip("Layer name of the Bot character")]
    public string botLayerName = "Bot";

    [Header("== SUPER MODE ==")]
    public float superModeDuration = 5f;
    public float superModeScale = 1.4f;

    [Tooltip("How far past superModeScale the ball punches on activation (e.g. 1.8)")]
    public float superPunchScale = 1.8f;
    [Tooltip("Seconds for the punch-in and punch-out phases combined")]
    public float superPunchDuration = 0.35f;

    [Tooltip("Particle burst on super activation — optional")]
    public ParticleSystem superModeEffect;

    [Tooltip("Assign your TrailRenderer here — it will ONLY emit during Super mode")]
    public TrailRenderer superTrail;

    [Tooltip("Color the trail glows during Super mode")]
    public Color superTrailColor = new Color(1f, 0.55f, 0f); // orange

    public bool SuperModeActive { get; private set; }

    private Rigidbody2D _rb;
    private Vector3 _startPosition;
    private Vector3 _startScale;
    private Coroutine _superModeCoroutine;

    private int _playerLayer = -1;
    private int _botLayer = -1;
    private Color _trailBaseColor;          // original trail color saved at Awake

    // ── Unity lifecycle ────────────────────────────────────────────────────

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _startPosition = transform.position;
        _startScale = transform.localScale;

        _playerLayer = LayerMask.NameToLayer(playerLayerName);
        _botLayer = LayerMask.NameToLayer(botLayerName);

        if (_playerLayer == -1)
            Debug.LogWarning($"[BallController] Layer '{playerLayerName}' not found.");
        if (_botLayer == -1)
            Debug.LogWarning($"[BallController] Layer '{botLayerName}' not found.");

        // Save the trail's original color and make sure it starts silent
        if (superTrail != null)
            _trailBaseColor = superTrail.colorGradient.colorKeys.Length > 0
                ? superTrail.colorGradient.colorKeys[0].color
                : Color.white;

        // Guarantee trail is off from the very first frame — before Start()
        DisableSuperEffects();
    }

    private void Start()
    {
        // Belt-and-suspenders: also called in Awake, safe to call twice
        DisableSuperEffects();
    }

    // ── Last-touch tracking ────────────────────────────────────────────────

    private void OnCollisionEnter2D(Collision2D col)
    {
        int layer = col.gameObject.layer;
        if (layer == _playerLayer) LastToucher = Toucher.Player;
        else if (layer == _botLayer) LastToucher = Toucher.Bot;
        // walls / ground / other → don't change LastToucher
    }

    // ── Public API ─────────────────────────────────────────────────────────

    public void ResetBall()
    {
        if (_superModeCoroutine != null)
        {
            StopCoroutine(_superModeCoroutine);
            _superModeCoroutine = null;
        }

        SuperModeActive = false;
        LastToucher = Toucher.None;
        _rb.isKinematic = false;
        _rb.velocity = Vector2.zero;
        _rb.angularVelocity = 0f;
        transform.position = _startPosition;
        transform.localScale = _startScale;

        DisableSuperEffects();  // ← trail cleared here too
        Debug.Log("[BallController] Ball reset.");
    }

    public void SetKinematic(bool kinematic)
    {
        _rb.velocity = Vector2.zero;
        _rb.angularVelocity = 0f;
        _rb.isKinematic = kinematic;
    }

    // ── Super mode ─────────────────────────────────────────────────────────

    /// <summary>
    /// Starts (or restarts) super mode with punch-scale intro + trail on.
    /// Trail turns off automatically when super mode ends or ball resets.
    /// </summary>
    public void ActivateSuperMode()
    {
        if (_superModeCoroutine != null)
            StopCoroutine(_superModeCoroutine);

        _superModeCoroutine = StartCoroutine(SuperModeRoutine());
        Debug.Log("[BallController] Super mode activated.");
    }

    /// <summary>Activates super mode only if not already active.</summary>
    public void TryActivateSuperMode()
    {
        if (SuperModeActive) return;
        ActivateSuperMode();
    }

    // ── Private ────────────────────────────────────────────────────────────

    private IEnumerator SuperModeRoutine()
    {
        SuperModeActive = true;

        // ── Turn trail ON only now ───────────────────────────────────────
        EnableSuperEffects();

        // ── Punch-in: startScale → punchScale ───────────────────────────
        float half = superPunchDuration * 0.5f;
        float elapsed = 0f;

        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            float s = Mathf.Lerp(1f, superPunchScale / superModeScale, elapsed / half);
            transform.localScale = _startScale * superModeScale * s;
            yield return null;
        }

        // ── Punch-out: punchScale → superModeScale ───────────────────────
        elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            float s = Mathf.Lerp(superPunchScale / superModeScale, 1f, elapsed / half);
            transform.localScale = _startScale * superModeScale * s;
            yield return null;
        }

        // Snap to exact target scale
        transform.localScale = _startScale * superModeScale;

        // ── Hold super mode for its full duration ────────────────────────
        Debug.Log($"[BallController] Super mode ON — {superModeDuration}s.");
        yield return new WaitForSeconds(superModeDuration);

        SuperModeActive = false;
        transform.localScale = _startScale;
        _superModeCoroutine = null;

        // ── Turn trail OFF when super mode naturally expires ─────────────
        DisableSuperEffects();
        Debug.Log("[BallController] Super mode OFF.");
    }

    // ── Effects helpers ────────────────────────────────────────────────────

    private void EnableSuperEffects()
    {
        if (superTrail != null)
        {
            superTrail.Clear();
            SetTrailColor(superTrailColor);
            superTrail.emitting = true;
        }

        if (superModeEffect != null && !superModeEffect.isPlaying)
            superModeEffect.Play();
    }

    private void DisableSuperEffects()
    {
        if (superTrail != null)
        {
            superTrail.emitting = false;
            superTrail.Clear();
            SetTrailColor(_trailBaseColor); // restore original color
        }

        if (superModeEffect != null && superModeEffect.isPlaying)
            superModeEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    /// <summary>
    /// Replaces every color key in the trail's gradient with <paramref name="c"/>
    /// while preserving each key's time position and all alpha keys.
    /// </summary>
    private void SetTrailColor(Color c)
    {
        if (superTrail == null) return;

        Gradient grad = superTrail.colorGradient;
        GradientColorKey[] colorKeys = grad.colorKeys;

        for (int i = 0; i < colorKeys.Length; i++)
            colorKeys[i].color = c;

        grad.SetKeys(colorKeys, grad.alphaKeys);
        superTrail.colorGradient = grad;
    }
}