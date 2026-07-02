// ═══════════════════════════════════════════════════════════════════
// PlayerUIController.cs
//
// ANIMATION CHANGES:
//   • GrantSuperShoot() / RestoreSuperShoot() now trigger a repeating
//     pulse coroutine on the shoot button so the player knows it's ready.
//   • superShootReady setter cancels the pulse when power is consumed/reset.
//   • All previous logic (expiry, persistence, reset) unchanged.
// ═══════════════════════════════════════════════════════════════════
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class PlayerUIController : MonoBehaviour
{
    // ── Inspector ────────────────────────────────────────────────────

    [Header("Movement Settings")]
    public float moveSpeed = 7f;
    public float jumpForce = 14f;
    public int maxJumps = 2;

    [Header("Ground Detection")]
    public LayerMask groundLayer;

    [Header("UI Buttons")]
    [SerializeField] private Button jumpButton;
    [SerializeField] private Button moveRightButton;
    [SerializeField] private Button moveLeftButton;
    [SerializeField] private Button shootButton;

    [Header("Kick Detection")]
    [SerializeField] private Transform kickPoint;
    [SerializeField] private float kickRange = 1.5f;
    [SerializeField] private LayerMask ballLayer;

    [Header("Normal Shoot")]
    [SerializeField] private float normalForceX = 6f;
    [SerializeField] private float normalForceY = 2f;

    [Header("Super Shoot")]
    [SerializeField] private float superForceX = 28f;
    [SerializeField] private float superForceY = 10f;
    [SerializeField] private float superSpin = 1500f;

    [Header("Super Shoot Expiry")]
    [Tooltip("Seconds before an uncollected super shoot lapses automatically (0 = never)")]
    [SerializeField] private float superShootExpiry = 10f;

    [Header("Super-Ready Visual")]
    [SerializeField] private Color normalButtonColor = Color.white;
    [SerializeField] private Color superReadyButtonColor = new Color(1f, 0.55f, 0f);

    [Header("Super-Ready Button Animation")]
    [Tooltip("How much the button scales up at the peak of each pulse (1.0 = no pulse)")]
    [SerializeField] private float pulsePeakScale = 1.18f;
    [Tooltip("Duration of one full pulse cycle (seconds)")]
    [SerializeField] private float pulseDuration = 0.55f;

    // ── Private state ────────────────────────────────────────────────

    private Rigidbody2D rb;
    private int jumpsLeft;
    private bool isGrounded;
    private float moveDirection;
    private float kickCooldown;
    private bool facingRight = true;
    private Vector2 startPos;

    private Coroutine _superShootExpiryCoroutine;
    private Coroutine _superPulseCoroutine;          // NEW — button pulse
    private Vector3 _shootButtonBaseScale;          // NEW — original button scale

    // ── superShootReady property ─────────────────────────────────────

    private bool _superShootReady;
    public bool superShootReady
    {
        get => _superShootReady;
        set
        {
            _superShootReady = value;

            if (!value)
            {
                // Cancel expiry timer
                if (_superShootExpiryCoroutine != null)
                {
                    StopCoroutine(_superShootExpiryCoroutine);
                    _superShootExpiryCoroutine = null;
                }

                // Cancel pulse and snap button back to original scale
                StopPulse();
            }

            RefreshShootButtonColor();
        }
    }

    // ── Unity lifecycle ──────────────────────────────────────────────

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        jumpsLeft = maxJumps;
        startPos = transform.position;

        // Cache the shoot button's natural scale before any animation touches it
        if (shootButton != null)
            _shootButtonBaseScale = shootButton.transform.localScale;

        RegisterButtonEvents();
        RefreshShootButtonColor();
    }

    void OnDestroy()
    {
        UnregisterButtonEvents();
        StopPulse();
    }

    // ── Button wiring ────────────────────────────────────────────────

    private void RegisterButtonEvents()
    {
        jumpButton?.onClick.AddListener(Jump);
        shootButton?.onClick.AddListener(Shoot);
        if (moveRightButton != null) AddPointerEvents(moveRightButton, MoveRightDown, StopMove);
        if (moveLeftButton != null) AddPointerEvents(moveLeftButton, MoveLeftDown, StopMove);
    }

    private void UnregisterButtonEvents()
    {
        jumpButton?.onClick.RemoveListener(Jump);
        shootButton?.onClick.RemoveListener(Shoot);
        moveRightButton?.onClick.RemoveAllListeners();
        moveLeftButton?.onClick.RemoveAllListeners();
    }

    private void AddPointerEvents(Button btn,
        UnityEngine.Events.UnityAction onDown,
        UnityEngine.Events.UnityAction onUp)
    {
        var trigger = btn.gameObject.GetComponent<EventTrigger>()
                      ?? btn.gameObject.AddComponent<EventTrigger>();

        var down = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        down.callback.AddListener(_ => onDown());

        var up = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        up.callback.AddListener(_ => onUp());

        trigger.triggers.Add(down);
        trigger.triggers.Add(up);
    }

    // ── Physics loop ─────────────────────────────────────────────────

    void FixedUpdate()
    {
        rb.velocity = new Vector2(moveDirection * moveSpeed, rb.velocity.y);
        if (kickCooldown > 0f) kickCooldown -= Time.fixedDeltaTime;
    }

    // ── Movement ─────────────────────────────────────────────────────

    public void MoveRightDown() { moveDirection = 1f; Flip(true); }
    public void MoveLeftDown() { moveDirection = -1f; Flip(false); }
    public void StopMove() => moveDirection = 0f;

    private void Flip(bool toRight)
    {
        if (facingRight == toRight) return;
        facingRight = toRight;
        if (kickPoint == null) return;
        Vector3 p = kickPoint.localPosition;
        p.x = toRight ? Mathf.Abs(p.x) : -Mathf.Abs(p.x);
        kickPoint.localPosition = p;
    }

    // ── Jump ─────────────────────────────────────────────────────────

    public void Jump()
    {
        if (jumpsLeft <= 0) return;
        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        jumpsLeft--;
        isGrounded = false;
    }

    // ── Shoot ────────────────────────────────────────────────────────

    public void Shoot()
    {
        if (kickCooldown > 0f || kickPoint == null) return;

        StartCoroutine(KickAnimation());

        Collider2D ballCol = ballLayer.value == 0
            ? Physics2D.OverlapCircle(kickPoint.position, kickRange)
            : Physics2D.OverlapCircle(kickPoint.position, kickRange, ballLayer);

        if (ballCol == null) { kickCooldown = 0.4f; return; }

        Rigidbody2D ballRb = ballCol.GetComponent<Rigidbody2D>();
        if (ballRb == null) { kickCooldown = 0.4f; return; }

        float dir = facingRight ? 1f : -1f;

        if (superShootReady)
        {
            ballCol.GetComponent<BallController>()?.ActivateSuperMode();
            ballRb.velocity = new Vector2(dir * superForceX, superForceY);
            ballRb.angularVelocity = dir * -superSpin;
            SoccerGameManager.Instance?.OnSuperShootFired();
            superShootReady = false;
            Debug.Log($"[Player] 💥 SUPER SHOOT  dir={dir}  v={ballRb.velocity}");
        }
        else
        {
            ballRb.AddForce(new Vector2(dir * normalForceX, normalForceY),
                            ForceMode2D.Impulse);
            Debug.Log($"[Player] 🦶 Normal shoot  dir={dir}");
        }

        kickCooldown = 0.5f;
    }

    // ── Public API for GameManager ───────────────────────────────────

    /// <summary>
    /// Called by GameManager after the player wins the quiz.
    /// Starts the pulse animation + expiry timer.
    /// </summary>
    public void GrantSuperShoot()
    {
        if (_superShootExpiryCoroutine != null)
        {
            StopCoroutine(_superShootExpiryCoroutine);
            _superShootExpiryCoroutine = null;
        }

        superShootReady = true;
        StartPulse();   // ← animate button

        if (superShootExpiry > 0f)
            _superShootExpiryCoroutine = StartCoroutine(SuperShootExpiryRoutine());

        Debug.Log($"[Player] Super shoot granted" +
                  (superShootExpiry > 0f ? $" — expires in {superShootExpiry}s." : " — no expiry."));
    }

    /// <summary>
    /// Called by GameManager.RestoreEarnedSuperShoots() after a round reset.
    /// Re-applies the ready state and restarts the pulse without resetting expiry.
    /// </summary>
    public void RestoreSuperShoot()
    {
        if (_superShootExpiryCoroutine != null)
        {
            StopCoroutine(_superShootExpiryCoroutine);
            _superShootExpiryCoroutine = null;
        }

        _superShootReady = true;        // raw field — skip setter's cancel branch
        RefreshShootButtonColor();
        StartPulse();                   // ← re-animate button after reset

        Debug.Log("[Player] Super shoot restored after reset (no expiry restart).");
    }

    // ── Pulse animation ──────────────────────────────────────────────

    /// <summary>Starts the repeating scale-pulse on the shoot button.</summary>
    private void StartPulse()
    {
        if (shootButton == null) return;
        StopPulse();    // never stack two pulses
        _superPulseCoroutine = StartCoroutine(PulseRoutine());
    }

    /// <summary>Cancels the pulse and snaps the button back to its natural scale.</summary>
    private void StopPulse()
    {
        if (_superPulseCoroutine != null)
        {
            StopCoroutine(_superPulseCoroutine);
            _superPulseCoroutine = null;
        }

        // Snap scale back so the button isn't frozen mid-pulse
        if (shootButton != null)
            shootButton.transform.localScale = _shootButtonBaseScale;
    }

    /// <summary>
    /// Smooth sine-wave pulse: base → peak → base, looping every pulseDuration seconds.
    /// Uses unscaled time so it keeps animating while Time.timeScale == 0
    /// (e.g. during the quiz freeze).
    /// </summary>
    private IEnumerator PulseRoutine()
    {
        float half = pulseDuration * 0.5f;

        while (true)
        {
            // Expand: base → peak
            float elapsed = 0f;
            while (elapsed < half)
            {
                elapsed += Time.unscaledDeltaTime;
                float s = Mathf.Lerp(1f, pulsePeakScale, elapsed / half);
                shootButton.transform.localScale = _shootButtonBaseScale * s;
                yield return null;
            }

            // Contract: peak → base
            elapsed = 0f;
            while (elapsed < half)
            {
                elapsed += Time.unscaledDeltaTime;
                float s = Mathf.Lerp(pulsePeakScale, 1f, elapsed / half);
                shootButton.transform.localScale = _shootButtonBaseScale * s;
                yield return null;
            }
        }
    }

    // ── Private ──────────────────────────────────────────────────────

    private IEnumerator SuperShootExpiryRoutine()
    {
        yield return new WaitForSeconds(superShootExpiry);
        if (_superShootReady)
        {
            superShootReady = false;    // setter calls StopPulse()
            Debug.Log("[Player] Super shoot expired unused.");
        }
        _superShootExpiryCoroutine = null;
    }

    private IEnumerator KickAnimation()
    {
        if (kickPoint == null) yield break;
        float dir = facingRight ? 1f : -1f;
        Vector3 orig = kickPoint.localPosition;
        kickPoint.localPosition += new Vector3(dir * 0.3f, 0f, 0f);
        yield return new WaitForSeconds(0.1f);
        kickPoint.localPosition = orig;
    }

    private void RefreshShootButtonColor()
    {
        if (shootButton == null) return;
        var img = shootButton.GetComponent<Image>();
        if (img != null)
            img.color = _superShootReady ? superReadyButtonColor : normalButtonColor;
    }

    // ── Reset ────────────────────────────────────────────────────────

    public void ResetPlayer()
    {
        transform.position = startPos;
        rb.velocity = Vector2.zero;
        moveDirection = 0f;
        kickCooldown = 0f;
        jumpsLeft = maxJumps;
        facingRight = true;

        if (_superShootExpiryCoroutine != null)
        {
            StopCoroutine(_superShootExpiryCoroutine);
            _superShootExpiryCoroutine = null;
        }

        StopPulse();            // ← also snaps scale back
        _superShootReady = false;
        RefreshShootButtonColor();

        if (kickPoint != null)
        {
            Vector3 p = kickPoint.localPosition;
            p.x = Mathf.Abs(p.x);
            kickPoint.localPosition = p;
        }
    }

    // ── Collision ────────────────────────────────────────────────────

    void OnCollisionEnter2D(Collision2D col)
    {
        if (IsGroundLayer(col.gameObject.layer))
        { isGrounded = true; jumpsLeft = maxJumps; }
    }

    void OnCollisionExit2D(Collision2D col)
    {
        if (IsGroundLayer(col.gameObject.layer)) isGrounded = false;
    }

    private bool IsGroundLayer(int layer) =>
        (groundLayer.value & (1 << layer)) != 0;

    void OnDrawGizmosSelected()
    {
        if (kickPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(kickPoint.position, kickRange);
    }
}