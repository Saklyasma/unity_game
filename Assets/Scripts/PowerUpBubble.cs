// ═══════════════════════════════════════════════════════════════════
// PowerUpBubble.cs
//
// CHANGES vs previous version:
//   • Player proximity check added to FixedUpdate (same pattern as bot).
//     This makes player collection reliable regardless of layer/mask config.
//   • OnTriggerEnter2D kept as secondary path (belt-and-suspenders).
//   • Null-guard on _rb before MovePosition to avoid NRE on first frame.
//   • Early-exit in FixedUpdate if no controllers are assigned.
// ═══════════════════════════════════════════════════════════════════
using UnityEngine;

public class PowerUpBubble : MonoBehaviour
{
    [Header("== FLOATING ==")]
    public float floatAmplitude = 0.3f;
    public float floatSpeed = 2f;
    public float travelSpeed = 1.5f;

    [Header("== COLLECTION ==")]
    [Tooltip("Distance at which either player is considered to have collected the bubble")]
    public float collectRadius = 1.2f;

    [Header("== REFERENCES ==")]
    [SerializeField] private PlayerUIController playerController;
    public BotAIController botController;

    [Header("== LAYER MASKS (optional — proximity check is primary) ==")]
    [SerializeField] private LayerMask playerLayerMask;

    private Vector3 _startPos;
    private Vector3 _endPos;
    private float _journeyLength;
    private float _distanceCovered;
    private bool _moving;
    private Rigidbody2D _rb;
    private float _sqrCollect;

    // Cached player transform so we don't call GetComponent every frame
    private Transform _playerTransform;

    /// <summary>True while the bubble is active and moving across the field.</summary>
    public bool IsActive => _moving && gameObject.activeSelf;

    // ── Unity lifecycle ──────────────────────────────────────────────────

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _startPos = transform.position;
        _endPos = new Vector3(-_startPos.x, _startPos.y, _startPos.z);
        _journeyLength = Vector3.Distance(_startPos, _endPos);
        _sqrCollect = collectRadius * collectRadius;

        // Cache the player transform once — avoids GetComponent in hot path
        if (playerController != null)
            _playerTransform = playerController.transform;

        gameObject.SetActive(false);
    }

    void FixedUpdate()
    {
        if (!_moving) return;

        // ── Move bubble ──────────────────────────────────────────────────
        _distanceCovered += travelSpeed * Time.fixedDeltaTime;
        float t = Mathf.Clamp01(_distanceCovered / _journeyLength);
        float sinY = Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        Vector3 pos = Vector3.Lerp(_startPos, _endPos, t);
        pos.y += sinY;
        _rb.MovePosition(pos);

        Vector2 pos2D = pos; // reuse for distance checks below

        // ── Player proximity check (PRIMARY collection path) ─────────────
        // Mirrors the bot check: fast sqrMagnitude, no physics layer dependency.
        if (_playerTransform != null)
        {
            Vector2 toPlayer = (Vector2)_playerTransform.position - pos2D;
            if (toPlayer.sqrMagnitude <= _sqrCollect)
            {
                Debug.Log("[PowerUpBubble] ✅ Player collected bubble (proximity).");
                ResetBubble();
                GrantPlayerSuperShoot(null); // null → use cached playerController
                return;
            }
        }

        // ── Bot proximity check ──────────────────────────────────────────
        if (botController != null)
        {
            Vector2 toBot = (Vector2)botController.transform.position - pos2D;
            if (toBot.sqrMagnitude <= _sqrCollect)
            {
                Debug.Log("[PowerUpBubble] ✅ Bot collected bubble → superShootReady = true.");
                ResetBubble();
                botController.superShootReady = true;
                botController.ForceDecide();
                return;
            }
        }

        // ── End of path ──────────────────────────────────────────────────
        if (t >= 1f)
        {
            Debug.Log("[PowerUpBubble] End of path → Reset.");
            ResetBubble();
        }
    }

    // ── Trigger — PLAYER secondary path ─────────────────────────────────
    // Kept as a fallback in case the player moves very fast and skips past
    // the proximity radius between FixedUpdate ticks (tunnelling edge case).

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!_moving) return;

        // If a playerLayerMask is configured, respect it; otherwise accept any hit
        if (playerLayerMask.value != 0 && ((1 << other.gameObject.layer) & playerLayerMask) == 0)
            return;

        // Don't fire if the bot somehow has a trigger (belt-and-suspenders)
        if (botController != null && other.gameObject == botController.gameObject) return;

        Debug.Log("[PowerUpBubble] ✅ Player collected bubble (trigger).");
        ResetBubble();
        GrantPlayerSuperShoot(other);
    }

    // ── Public ───────────────────────────────────────────────────────────

    /// <summary>Reactivates the bubble from its spawn position.</summary>
    public void Respawn()
    {
        _distanceCovered = 0f;
        if (_rb != null) _rb.MovePosition(_startPos);
        gameObject.SetActive(true);
        _moving = true;
        Debug.Log("[PowerUpBubble] Bubble respawned.");
    }

    // ── Private ──────────────────────────────────────────────────────────

    /// <summary>
    /// Central grant logic for the player.
    /// Pass the triggering Collider2D when called from OnTriggerEnter2D,
    /// or null when called from the proximity path (uses cached controller).
    /// </summary>
    private void GrantPlayerSuperShoot(Collider2D other)
    {
        // Priority 1: GameManager quiz flow
        if (SoccerGameManager.Instance != null)
        {
            SoccerGameManager.Instance.OnBubbleQuizTriggered();
            return;
        }

        // Priority 2: direct grant (debug scenes / no GameManager)
        PlayerUIController p = playerController;

        if (p == null && other != null)
            p = other.GetComponent<PlayerUIController>();

        if (p != null)
        {
            p.GrantSuperShoot();
            Debug.Log("[PowerUpBubble] Player GrantSuperShoot() via fallback.");
        }
        else
        {
            Debug.LogWarning("[PowerUpBubble] Could not find PlayerUIController to grant super shoot!");
        }
    }

    private void ResetBubble()
    {
        _moving = false;
        _distanceCovered = 0f;
        if (_rb != null) _rb.MovePosition(_startPos);
        gameObject.SetActive(false);
    }
}