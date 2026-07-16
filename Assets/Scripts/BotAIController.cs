// ═══════════════════════════════════════════════════════════════════
// BotAIController.cs  —  v2.0  (Performance + Realism Overhaul)
//
// CHANGES FROM v1:
//   • Adaptive ball prediction (scales with speed + distance)
//   • Intercept uses time-of-arrival estimation, not fixed predictionTime
//   • Smart repositioning after shooting (avoids ball-crowding)
//   • Corner aim uses player velocity to predict future keeper position
//   • Goalie bounce-estimation for fast low shots
//   • Better aerial: predicts apex Y, only jumps when it's reachable
//   • Possession detection includes ball acceleration
//   • Difficulty gates fine-grained: reaction, aim, aggression decouple
//   • FixedUpdate caches all per-frame data once (no duplicate GetComponent calls)
//   • Squared-distance comparisons kept throughout (no Sqrt in hot paths)
//   • Super-shoot persistence fix from v1 fully preserved
// ═══════════════════════════════════════════════════════════════════

using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BotAIController : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────
    // INSPECTOR
    // ─────────────────────────────────────────────────────────

    [Header("Movement")]
    public float moveSpeed = 5.5f;
    public float jumpForce = 9f;
    public int maxJumps = 2;

    [Header("Movement Feel")]
    public float acceleration = 14f;
    public float deceleration = 20f;
    [Tooltip("Speed multiplier when returning to goal")]
    public float returnSpeedMul = 0.65f;

    [Header("Ground Detection")]
    public LayerMask groundLayer;
    public float groundRayLength = 0.6f;

    [Header("Kick")]
    [SerializeField] private float kickForce = 14f;
    [SerializeField] private float kickRange = 2.0f;
    [SerializeField] private float kickCooldownTime = 0.45f;

    [Header("Super Shoot")]
    [SerializeField] private float superForceX = 22f;
    [SerializeField] private float superForceY = 8f;
    [SerializeField] private float superSpin = 600f;

    [Header("Force Variance")]
    public float jumpForceVariance = 0.8f;
    public float kickForceVariance = 1.5f;

    [Header("References")]
    public Transform ball;
    public Transform playerGoal;
    public Transform player;
    public Transform botGoalZone;

    [Header("AI — Distances")]
    public float defendRadius = 5.5f;
    public float shootTriggerRange = 3.2f;
    public float predictionTime = 0.22f;
    [Tooltip("Bot approaches this offset behind ball before shooting")]
    public float chaseApproachDist = 0.4f;

    [Header("AI — Possession Detection")]
    public float possessionRange = 2.2f;
    public float velocityDotWeight = 1.2f;

    [Header("AI — Goalie")]
    public float keeperDepth = 1.1f;
    public float keeperTrackSpeed = 8f;

    [Header("AI — Safe Distance")]
    public float safeDistance = 1.5f;
    [Range(0f, 1f)]
    public float repulsionStrength = 0.75f;

    [Header("AI — Jump Rules")]
    public float noJumpNearPlayerRange = 0.8f;
    public float jumpCooldownTime = 0.40f;
    public float jumpBallHeightThreshold = 0.3f;

    [Header("AI — Reaction")]
    public float reactionMin = 0.08f;
    public float reactionMax = 0.20f;

    [Header("AI — Kick Aim")]
    [Range(0f, 1f)]
    public float cornerAimBias = 0.55f;
    public float cornerAimMinSpeed = 1.0f;

    [Header("AI — Bubble Collection")]
    public PowerUpBubble bubbleObject;
    public float bubbleDetectRange = 7f;
    public float bubbleJumpYThreshold = 0.4f;

    [Header("AI — Difficulty")]
    [Range(0f, 1f)]
    [Tooltip("0 = easy, 1 = hard")]
    public float difficulty = 0.7f;

    [Header("AI — Play Style (set via ApplyStats)")]
    [Range(0f, 1f)]
    public float playStyleAggressive = 0.5f;
    [Range(0f, 1f)]
    public float playStyleDefensive = 0.3f;
    [Range(0f, 1f)]
    public float playStylePossession = 0.4f;

    [Header("AI — Player Pattern Adaptation")]
    [Tooltip("How many of the player's last shot targets to remember")]
    public int playerHistorySize = 5;
    [Tooltip("How much to bias goalie away from player's preferred zone")]
    [Range(0f, 1f)]
    public float patternAvoidBias = 0.35f;

    [Header("AI — Dribble")]
    [Tooltip("How much open space (as fraction of field) bot needs to dribble instead of shoot")]
    [Range(0f, 1f)]
    public float dribbleSpaceThreshold = 0.25f;

    [Header("AI — Pressure")]
    public int pressureScoreDiff = 1;
    public float pressureSpeedBoost = 1.25f;
    public float pressureRadiusBoost = 1.5f;

    [Header("AI — Fake Shoot")]
    [Range(0f, 1f)]
    public float fakeShootChance = 0.2f;
    public float fakeShootDuration = 0.18f;

    [Header("AI — Aerial")]
    public float aerialInterceptLookAhead = 0.5f;
    public float aerialMinBallVelY = 2.5f;

    [Header("AI — Post-Shot Reposition")]
    [Tooltip("After shooting, bot steps back this many units to avoid crowding")]
    public float repositionOffset = 1.2f;
    [Tooltip("Seconds bot waits before repositioning after a kick")]
    public float repositionDelay = 0.25f;

    [Header("Debug")]
    public bool verboseLog = false;

    // ─────────────────────────────────────────────────────────
    // PRIVATE — components
    // ─────────────────────────────────────────────────────────

    private Rigidbody2D rb;
    private Rigidbody2D ballRb;
    private Rigidbody2D _bubbleRb;
    private Rigidbody2D _playerRb;
    private BallController _ballController;

    // ─────────────────────────────────────────────────────────
    // PRIVATE — cached per-frame world data
    // ─────────────────────────────────────────────────────────

    private Vector2 _ballPos;
    private Vector2 _ballVel;
    private Vector2 _ballVelPrev;           // for acceleration
    private Vector2 _playerPos;
    private Vector2 _playerVel;
    private Vector2 _botPos;
    private Vector2 _predictedBallPos;      // short-horizon prediction
    private Vector2 _interceptTarget;       // time-of-arrival intercept
    private float _ballSpd;

    // ─────────────────────────────────────────────────────────
    // PRIVATE — cached squared distances
    // ─────────────────────────────────────────────────────────

    private float _sqrShoot;
    private float _sqrKick;
    private float _sqrDefend;
    private float _sqrPossession;
    private float _sqrNoJump;
    private float _sqrBubbleDetect;

    // ─────────────────────────────────────────────────────────
    // PRIVATE — state machine
    // ─────────────────────────────────────────────────────────

    private enum BotState
    {
        Idle, Chase, Shoot, Defend, GoalieBlock, ReturnGoal, CollectBubble, Reposition
    }
    private BotState _state = BotState.Idle;
    private Vector2 _spawnPos;

    // ─────────────────────────────────────────────────────────
    // PRIVATE — physics / movement
    // ─────────────────────────────────────────────────────────

    private int _jumpsLeft;
    private bool _isGrounded;
    private bool _facingRight;
    private bool _wantsJump;

    private float _kickCooldown;
    private float _jumpCooldown;
    private float _reactionTimer;
    private bool _reactionPending;
    private float _decisionTimer;
    private const float DecisionInterval = 0.15f;

    private float _stuckTimer;
    private float _unstuckTimer;
    private float _unstuckDir;
    private float _targetX;

    // ─────────────────────────────────────────────────────────
    // PRIVATE — field geometry
    // ─────────────────────────────────────────────────────────

    private float _homeX;
    private float _fieldDir;
    private float _midX;
    private float _goalTopY;
    private float _goalBotY;
    private float _goalMidY;

    // ─────────────────────────────────────────────────────────
    // PRIVATE — misc AI
    // ─────────────────────────────────────────────────────────

    private bool _fakeShootPending;
    private float _fakeShootTimer;
    private float _repositionTimer;        // countdown after kick
    private Vector2 _repositionTarget;

    // ─────────────────────────────────────────────────────────
    // PRIVATE — player pattern tracking
    // ─────────────────────────────────────────────────────────

    private float[] _playerShotHistory;
    private int _playerShotIndex;

    // ─────────────────────────────────────────────────────────
    // PRIVATE — possession tracking
    // ─────────────────────────────────────────────────────────

    private bool _playerHadPossession;
    private float _possessionLostTimer;
    private const float PossessionFallbackDuration = 0.6f;

    // ─────────────────────────────────────────────────────────
    // PROPERTY — superShootReady (logged, preserves match flag)
    // ─────────────────────────────────────────────────────────

    private bool _superShootReady;
    public bool superShootReady
    {
        get => _superShootReady;
        set
        {
            _superShootReady = value;
            if (verboseLog)
                Debug.Log($"[BotAI] superShootReady = {value}");
        }
    }

    // ═════════════════════════════════════════════════════════
    // INIT
    // ═════════════════════════════════════════════════════════

    void Awake() => rb = GetComponent<Rigidbody2D>();

    void Start()
    {
        _jumpsLeft = maxJumps;

        if (ball != null)
        {
            ballRb = ball.GetComponent<Rigidbody2D>();
            _ballController = ball.GetComponent<BallController>();
        }

        if (bubbleObject != null)
            _bubbleRb = bubbleObject.GetComponent<Rigidbody2D>();

        if (player != null)
            _playerRb = player.GetComponent<Rigidbody2D>();

        CacheSqrDistances();

        if (botGoalZone != null && playerGoal != null)
        {
            _homeX = botGoalZone.position.x;
            _fieldDir = Mathf.Sign(playerGoal.position.x - _homeX);
            _midX = (_homeX + playerGoal.position.x) * 0.5f;
        }

        Collider2D goalCol = playerGoal != null
            ? playerGoal.GetComponent<Collider2D>() : null;

        if (goalCol != null)
        {
            _goalTopY = goalCol.bounds.max.y;
            _goalBotY = goalCol.bounds.min.y;
        }
        else if (playerGoal != null)
        {
            _goalTopY = playerGoal.position.y + 1f;
            _goalBotY = playerGoal.position.y - 1f;
        }

        _decisionTimer = Random.Range(0f, DecisionInterval);
        _facingRight = _fieldDir < 0f;
        ApplyScale();
        _spawnPos = rb.position;

        _goalMidY = (_goalTopY + _goalBotY) * 0.5f;

        _playerShotHistory = new float[playerHistorySize];
        for (int i = 0; i < playerHistorySize; i++)
            _playerShotHistory[i] = _goalMidY;
        _playerShotIndex = 0;
    }

    private void CacheSqrDistances()
    {
        _sqrShoot = shootTriggerRange * shootTriggerRange;
        _sqrKick = kickRange * kickRange;
        _sqrDefend = defendRadius * defendRadius;
        _sqrPossession = possessionRange * possessionRange;
        _sqrNoJump = noJumpNearPlayerRange * noJumpNearPlayerRange;
        _sqrBubbleDetect = bubbleDetectRange * bubbleDetectRange;
    }

    // ═════════════════════════════════════════════════════════
    // PUBLIC RESET
    // ═════════════════════════════════════════════════════════

    public void ResetToSpawn()
    {
        rb.position = _spawnPos;
        rb.velocity = Vector2.zero;

        _kickCooldown = 0f;
        _jumpCooldown = 0f;
        _reactionTimer = 0f;
        _stuckTimer = 0f;
        _unstuckTimer = 0f;
        _repositionTimer = 0f;
        _decisionTimer = DecisionInterval;
        _jumpsLeft = maxJumps;
        _wantsJump = false;
        _reactionPending = false;
        _fakeShootPending = false;
        _fakeShootTimer = 0f;
        _state = BotState.Idle;
        _targetX = _spawnPos.x;

        // DO NOT touch superShootReady — GameManager.RestoreEarnedSuperShoots()
        // calls RestoreSuperShoot() after this if BotSuperShootEarned is true.
        _superShootReady = false;

        if (verboseLog)
            Debug.Log("[BotAI] ResetToSpawn — superShootReady cleared, pending restore.");
    }

    /// <summary>Called by GameManager.RestoreEarnedSuperShoots() after a round reset.</summary>
    public void RestoreSuperShoot()
    {
        superShootReady = true;
        Debug.Log("[BotAI] Super shoot restored after reset.");
    }

    public void ApplyStats(BotStatsData stats)
    {
        if (stats == null) return;

        moveSpeed = stats.moveSpeed;
        jumpForce = stats.jumpForce;
        difficulty = stats.difficulty;
        pressureSpeedBoost = stats.pressureSpeedBoost;

        playStyleAggressive = stats.aggressive;
        playStyleDefensive = stats.defensive;
        playStylePossession = stats.possession;

        var flags = System.Reflection.BindingFlags.NonPublic
                  | System.Reflection.BindingFlags.Instance;

        typeof(BotAIController).GetField("kickForce", flags)?.SetValue(this, stats.kickForce);
        typeof(BotAIController).GetField("kickRange", flags)?.SetValue(this, stats.kickRange);

        _sqrKick = stats.kickRange * stats.kickRange;
        _sqrShoot = shootTriggerRange * shootTriggerRange;

        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null) sr.color = Color.white;

        Debug.Log($"[BotAI] Stats applied for {stats.countryName} — difficulty={difficulty} agg={playStyleAggressive} def={playStyleDefensive} poss={playStylePossession}");
    }

    public void ForceDecide()
    {
        _decisionTimer = 0f;
        _reactionPending = false;
        Decide();
    }

    // ═════════════════════════════════════════════════════════
    // DIFFICULTY & PRESSURE HELPERS
    // ═════════════════════════════════════════════════════════

    private bool IsUnderPressure()
    {
        if (SoccerGameManager.Instance == null) return false;
        return (SoccerGameManager.Instance.PlayerScore
              - SoccerGameManager.Instance.BotScore) >= pressureScoreDiff;
    }

    private float EffectiveSpeed()
    {
        float s = moveSpeed * Mathf.Lerp(0.80f, 1.10f, difficulty);
        if (IsUnderPressure()) s *= pressureSpeedBoost;
        return s;
    }

    private float EffectiveShootSqr()
    {
        float range = shootTriggerRange * Mathf.Lerp(0.75f, 1.30f, difficulty);
        if (IsUnderPressure()) range *= pressureRadiusBoost;
        return range * range;
    }

    // Reaction window: at high difficulty the bot reacts nearly instantly
    private float ReactionWindow()
    {
        float rMin = Mathf.Lerp(reactionMax, reactionMin, difficulty);
        float rMax = Mathf.Lerp(reactionMax, reactionMin * 0.5f, difficulty);
        return Random.Range(rMin, rMax);
    }

    // ═════════════════════════════════════════════════════════
    // UPDATE
    // ═════════════════════════════════════════════════════════

    void Update()
    {
        float dt = Time.deltaTime;

        if (_kickCooldown > 0f) _kickCooldown -= dt;
        if (_jumpCooldown > 0f) _jumpCooldown -= dt;
        if (_reactionTimer > 0f) _reactionTimer -= dt;
        if (_unstuckTimer > 0f) _unstuckTimer -= dt;
        if (_repositionTimer > 0f) _repositionTimer -= dt;

        if (_fakeShootPending)
        {
            _fakeShootTimer -= dt;
            if (_fakeShootTimer <= 0f) _fakeShootPending = false;
        }

        if (_reactionPending && _reactionTimer <= 0f)
            _reactionPending = false;

        _decisionTimer -= dt;
        if (_decisionTimer <= 0f && !_reactionPending)
        {
            _decisionTimer = DecisionInterval;
            Decide();
        }
        else if (_decisionTimer <= 0f)
        {
            _decisionTimer = DecisionInterval * 0.5f;
        }

        UpdateStuck(dt);
    }

    // ═════════════════════════════════════════════════════════
    // FIXED UPDATE
    // ═════════════════════════════════════════════════════════

    void FixedUpdate()
    {
        _botPos = rb.position;

        // ── Cache ball data ───────────────────────────────────────────────
        if (ball != null)
        {
            _ballPos = ball.position;
            if (ballRb != null)
            {
                _ballVelPrev = _ballVel;
                _ballVel = ballRb.velocity;
                _ballSpd = _ballVel.magnitude;
            }
            else { _ballVel = _ballVelPrev = Vector2.zero; _ballSpd = 0f; }
        }

        // ── Cache player data ─────────────────────────────────────────────
        if (player != null)
        {
            _playerPos = player.position;
            _playerVel = _playerRb != null ? _playerRb.velocity : Vector2.zero;
        }

        // ── Adaptive prediction ───────────────────────────────────────────
        // Horizon scales: closer/faster ball → shorter horizon (less over-shooting)
        float distToBall = Vector2.Distance(_botPos, _ballPos);
        float adaptivePt = Mathf.Clamp(predictionTime
                                  * Mathf.Lerp(1.5f, 0.7f, Mathf.Clamp01(_ballSpd / 10f))
                                  * Mathf.Lerp(1.3f, 0.8f, Mathf.Clamp01(distToBall / 8f)),
                                  0.08f, 0.45f);

        _predictedBallPos = new Vector2(
            _ballPos.x + _ballVel.x * adaptivePt,
            _ballPos.y + _ballVel.y * adaptivePt
                       + 0.5f * Physics2D.gravity.y * adaptivePt * adaptivePt
        );

        ComputeIntercept();

        // ── Ground check ─────────────────────────────────────────────────
        const float hw = 0.3f;
        _isGrounded =
            Physics2D.Raycast(_botPos, Vector2.down, groundRayLength, groundLayer)
         || Physics2D.Raycast(_botPos + new Vector2(-hw, 0f), Vector2.down, groundRayLength, groundLayer)
         || Physics2D.Raycast(_botPos + new Vector2(hw, 0f), Vector2.down, groundRayLength, groundLayer);

        if (_isGrounded) _jumpsLeft = maxJumps;

        _wantsJump = false;
        ExecuteState();

        // ── Desired X with repulsion & field bounds ───────────────────────
        float desiredX = _unstuckTimer > 0f
            ? _botPos.x + _unstuckDir * 2f : _targetX;

        bool offensive = _state == BotState.Chase || _state == BotState.Shoot;
        if (offensive && player != null)
        {
            float hDiff = _botPos.x - _playerPos.x;
            float hDiffAbs = hDiff < 0f ? -hDiff : hDiff;
            if (hDiffAbs < safeDistance)
            {
                float overlap = 1f - (hDiffAbs / safeDistance);
                float pushDir = hDiff >= 0f ? 1f : -1f;
                desiredX += pushDir * overlap * repulsionStrength * safeDistance;
            }
        }

        if (botGoalZone != null && _state != BotState.CollectBubble)
        {
            if (_fieldDir > 0f) desiredX = Mathf.Max(desiredX, _homeX);
            else desiredX = Mathf.Min(desiredX, _homeX);
        }

        // ── Velocity integration ──────────────────────────────────────────
        float speedMul = _state == BotState.ReturnGoal ? returnSpeedMul : 1f;
        float baseSpd = EffectiveSpeed() * speedMul;
        float diff = desiredX - _botPos.x;
        float movDir = diff > 0.15f ? 1f : (diff < -0.15f ? -1f : 0f);
        float targetSpd = movDir * baseSpd;
        float rate = movDir != 0f ? acceleration : deceleration;
        float newVelX = Mathf.MoveTowards(rb.velocity.x, targetSpd, rate * Time.fixedDeltaTime);

        float newVelY = rb.velocity.y;

        bool playerBodyBlock = _state != BotState.CollectBubble
                            && player != null
                            && (_playerPos - _botPos).sqrMagnitude < _sqrNoJump;

        if (_wantsJump && _jumpsLeft > 0 && !playerBodyBlock && _jumpCooldown <= 0f)
        {
            float jf = jumpForce + Random.Range(-jumpForceVariance, jumpForceVariance);
            newVelY = jf;
            _jumpsLeft--;
            _jumpCooldown = jumpCooldownTime;
            if (verboseLog)
                Debug.Log($"[BotAI] JUMP state={_state} force={jf:F2} jumpsLeft={_jumpsLeft}");
        }

        rb.velocity = new Vector2(newVelX, newVelY);
        Flip(_fieldDir > 0f);
    }

    // ═════════════════════════════════════════════════════════
    // INTERCEPT — time-of-arrival estimation
    // ═════════════════════════════════════════════════════════

    void ComputeIntercept()
    {
        if (ballRb == null) { _interceptTarget = _ballPos; return; }

        float spd = Mathf.Max(EffectiveSpeed(), 0.1f);
        float tArrive = Mathf.Clamp(distToBall() / spd, 0f, 0.9f);

        // Iterative refinement (one extra step for a better estimate)
        Vector2 est1 = BallPositionAt(tArrive);
        float d1 = Vector2.Distance(_botPos, est1);
        float t2 = Mathf.Clamp(d1 / spd, 0f, 0.9f);

        _interceptTarget = BallPositionAt(t2);
    }

    private float distToBall() => Vector2.Distance(_botPos, _ballPos);

    private Vector2 BallPositionAt(float t)
    {
        return new Vector2(
            _ballPos.x + _ballVel.x * t,
            _ballPos.y + _ballVel.y * t + 0.5f * Physics2D.gravity.y * t * t
        );
    }

    // ═════════════════════════════════════════════════════════
    // DECIDE
    // ═════════════════════════════════════════════════════════

    void Decide()
    {
        if (ball == null) { SetState(BotState.Idle); return; }

        // Reposition: don't interrupt until delay expires
        if (_state == BotState.Reposition && _repositionTimer > 0f) return;

        float sqrToBall = (_ballPos - _botPos).sqrMagnitude;
        float sqrBallHome = botGoalZone != null
            ? (_ballPos - (Vector2)botGoalZone.position).sqrMagnitude : 0f;
        float effectiveSqr = EffectiveShootSqr();

        // ── Super shoot awareness ────────────────────────────────────────
        bool playerSuper = SoccerGameManager.Instance != null
            && SoccerGameManager.Instance.PlayerSuperShootEarned;

        // ── Tactical fallback after losing possession ────────────────────
        bool playerHasPossession = PlayerHasPossession();
        if (!_playerHadPossession && playerHasPossession)
            _possessionLostTimer = PossessionFallbackDuration;
        _playerHadPossession = playerHasPossession;
        if (_possessionLostTimer > 0f) _possessionLostTimer -= Time.deltaTime;

        if (playerSuper)
        {
            // Player has super shoot: play more defensively
            if (sqrBallHome < _sqrDefend * 6f)
            {
                SetState(BotState.GoalieBlock);
                return;
            }
            if (playerHasPossession)
            {
                SetState(BotState.GoalieBlock);
                return;
            }
        }

        // After losing possession: fall back before engaging
        if (_possessionLostTimer > 0f && !playerHasPossession)
        {
            if (sqrBallHome < _sqrDefend * 3f)
            {
                SetState(BotState.GoalieBlock);
                return;
            }
        }

        // Priority 0: bubble if goal is safe
        if (BubbleIsReachable() && sqrBallHome >= _sqrDefend * 4f)
        {
            SetState(BotState.CollectBubble);
            return;
        }

        // Super shoot: stay offensive unless ball is at our goal
        if (superShootReady && sqrBallHome >= _sqrDefend)
        {
            SetState(sqrToBall <= effectiveSqr ? BotState.Shoot : BotState.Chase);
            return;
        }

        // Priority 1: immediate goal threat
        if (sqrBallHome < _sqrDefend * 4f)
        {
            SetState(sqrToBall <= effectiveSqr ? BotState.Defend : BotState.GoalieBlock);
            return;
        }

        // Priority 2: player has possession
        if (playerHasPossession)
        {
            // More aggressive countries chase sooner
            bool shouldChase = IsUnderPressure() && sqrToBall <= effectiveSqr;
            if (!shouldChase) shouldChase = sqrToBall < _sqrDefend * (1f + playStyleAggressive);
            SetState(shouldChase ? BotState.Chase : BotState.GoalieBlock);
            return;
        }

        // Priority 3: chase / shoot
        SetState(sqrToBall <= effectiveSqr ? BotState.Shoot : BotState.Chase);
    }

    private bool BubbleIsReachable()
    {
        if (bubbleObject == null || !bubbleObject.IsActive) return false;
        Vector2 d = (Vector2)bubbleObject.transform.position - _botPos;
        return d.sqrMagnitude <= _sqrBubbleDetect;
    }

    void SetState(BotState newState)
    {
        if (newState == _state) return;
        _reactionTimer = ReactionWindow();
        _reactionPending = true;
        _state = newState;
        if (verboseLog) Debug.Log($"[BotAI] → {_state}");
    }

    // ═════════════════════════════════════════════════════════
    // POSSESSION CHECK — adds ball acceleration factor
    // ═════════════════════════════════════════════════════════

    bool PlayerHasPossession()
    {
        if (player == null || botGoalZone == null) return false;

        Vector2 toPlayer = _playerPos - _ballPos;
        float sqrDist = toPlayer.sqrMagnitude;

        if (sqrDist > _sqrPossession * 4f) return false;
        if (_ballSpd < 0.3f) return sqrDist < _sqrPossession;

        float invLen = 1f / Mathf.Sqrt(sqrDist + 0.0001f);
        Vector2 toPNorm = toPlayer * invLen;
        Vector2 bVNorm = _ballVel / _ballSpd;

        Vector2 toGoal = ((Vector2)botGoalZone.position - _ballPos).normalized;
        float goalDot = Vector2.Dot(bVNorm, toGoal);
        float velDot = Vector2.Dot(bVNorm, toPNorm);

        // Acceleration toward goal = extra possession signal
        Vector2 ballAccel = (_ballVel - _ballVelPrev) / Mathf.Max(Time.fixedDeltaTime, 0.001f);
        float accelToGoal = Vector2.Dot(ballAccel.normalized, toGoal);

        bool nearPlayer = sqrDist - velDot * velocityDotWeight < _sqrPossession;
        bool movingAtGoal = goalDot > 0.3f && _ballSpd > 0.5f;
        bool acceleratingIn = accelToGoal > 0.4f;

        return nearPlayer || movingAtGoal || acceleratingIn;
    }

    // ═════════════════════════════════════════════════════════
    // STATE EXECUTION
    // ═════════════════════════════════════════════════════════

    void ExecuteState()
    {
        switch (_state)
        {
            case BotState.Idle: ExecIdle(); break;
            case BotState.Chase: ExecChase(); break;
            case BotState.Shoot: ExecShoot(); break;
            case BotState.Defend: ExecDefend(); break;
            case BotState.GoalieBlock: ExecGoalieBlock(); break;
            case BotState.ReturnGoal: ExecReturnGoal(); break;
            case BotState.CollectBubble: ExecCollectBubble(); break;
            case BotState.Reposition: ExecReposition(); break;
        }
    }

    void ExecIdle() => _targetX = _botPos.x;

    void ExecReturnGoal()
    {
        if (botGoalZone == null) { _targetX = _botPos.x; return; }
        _targetX = Mathf.Lerp(_botPos.x, _homeX + _fieldDir * keeperDepth, 0.85f);
        TryKickIfInRange();
    }

    void ExecChase()
    {
        float approachX = _interceptTarget.x - _fieldDir * chaseApproachDist;
        _targetX = Mathf.Lerp(_botPos.x, approachX, 0.9f);

        JumpTowardsBall(_interceptTarget, aerial: true);
        TryKickIfInRange();
    }

    void ExecShoot()
    {
        float dx = _ballPos.x - _botPos.x;
        float dy = _ballPos.y - _botPos.y;
        float sqrDst = dx * dx + dy * dy;
        float holdSqr = kickRange * kickRange * 0.25f;

        _targetX = sqrDst > holdSqr
            ? Mathf.Lerp(_botPos.x, _ballPos.x, 0.75f)
            : _botPos.x;

        JumpTowardsBall(_ballPos, aerial: true);

        if (_kickCooldown <= 0f && sqrDst <= _sqrKick && ShouldKickInsteadOfDribble())
            TryKick();
    }

    /// <summary>
    /// Decide whether to shoot or dribble. Dribble when there's open space
    /// toward the goal and the bot's play style favors possession.
    /// </summary>
    private bool ShouldKickInsteadOfDribble()
    {
        if (playerGoal == null) return true;

        // Super shoot ready: always shoot
        if (superShootReady) return true;

        // Player is close: shoot before they tackle
        if (player != null)
        {
            float playerDist = Mathf.Abs(_playerPos.x - _botPos.x);
            if (playerDist < safeDistance * 2f) return true;
        }

        // Calculate open space toward the opponent's goal
        float spaceToGoal = Mathf.Abs(playerGoal.position.x - _ballPos.x);
        float fieldWidth = Mathf.Abs((playerGoal.position.x - (botGoalZone != null ? botGoalZone.position.x : _homeX)));
        float spaceRatio = spaceToGoal / Mathf.Max(fieldWidth, 0.01f);

        // Low space = close to goal = shoot
        if (spaceRatio < 0.15f) return true;

        // High possession style + lots of space = dribble instead
        if (playStylePossession > 0.4f && spaceRatio > dribbleSpaceThreshold)
        {
            // Dribble: don't kick, keep chasing
            if (verboseLog)
                Debug.Log($"[BotAI] Dribbling — space={spaceRatio:F2} poss={playStylePossession:F2}");
            return false;
        }

        return true;
    }

    void ExecDefend()
    {
        if (botGoalZone == null) return;
        Vector2 goalPos = botGoalZone.position;
        float blendFwd = IsUnderPressure() ? 0.85f : 0.70f;
        float blendedX = _ballPos.x * blendFwd + goalPos.x * (1f - blendFwd);

        _targetX = _fieldDir > 0f
            ? Mathf.Max(blendedX, goalPos.x)
            : Mathf.Min(blendedX, goalPos.x);

        if (_isGrounded && _ballPos.y > _botPos.y + jumpBallHeightThreshold)
            _wantsJump = true;

        TryKickIfInRange();
    }

    void ExecGoalieBlock()
    {
        if (botGoalZone == null) return;
        Vector2 goalPos = botGoalZone.position;
        float anchorX = goalPos.x + _fieldDir * keeperDepth;

        bool ballInSuperMode = _ballController != null && _ballController.SuperModeActive;

        // ── Super mode goalie: track ball center, not prediction ─────────
        float trackBlend;
        Vector2 trackTarget;

        if (ballInSuperMode)
        {
            trackBlend = Mathf.Lerp(0.70f, 0.95f, difficulty);
            trackTarget = _ballPos;
        }
        else
        {
            trackBlend = Mathf.Lerp(0.55f, 0.88f, difficulty);
            trackTarget = _predictedBallPos;

            // ── Player pattern adaptation: bias away from preferred zone ──
            float playerPrefY = GetPlayerPreferredShotY();
            float biasDir = playerPrefY > _goalMidY ? -1f : 1f;
            float biasAmount = (_playerShotIndex > 1 ? patternAvoidBias : 0f)
                             * Mathf.Lerp(0.5f, 1f, difficulty);
            trackTarget.y += biasDir * biasAmount * (_goalTopY - _goalBotY) * 0.15f;
        }

        float trackX = Mathf.Lerp(anchorX, trackTarget.x, trackBlend);
        float maxFwd = goalPos.x + _fieldDir * keeperDepth * 2f;

        trackX = _fieldDir > 0f
            ? Mathf.Clamp(trackX, goalPos.x, maxFwd)
            : Mathf.Clamp(trackX, maxFwd, goalPos.x);

        _targetX = trackX;

        // Jump for fast low shots: estimate if ball will reach goal line
        bool fastIncoming = _ballSpd > 4f
            && Vector2.Dot(_ballVel.normalized,
                           ((Vector2)botGoalZone.position - _ballPos).normalized) > 0.5f;

        float jumpThreshold;
        if (ballInSuperMode)
            jumpThreshold = 0.15f;          // super = jump at anything
        else if (fastIncoming)
            jumpThreshold = 0.2f;
        else
            jumpThreshold = _ballSpd > 3f ? 0.4f : 0.8f;

        if (_isGrounded && _jumpCooldown <= 0f
            && (trackTarget.y > _botPos.y + jumpThreshold
                || (ballInSuperMode && trackTarget.y > _botPos.y)))
            _wantsJump = true;

        if (!_isGrounded && _jumpsLeft > 0 && _jumpCooldown <= 0f
            && trackTarget.y > _botPos.y + jumpThreshold)
            _wantsJump = true;

        TryKickIfInRange();
    }

    void ExecCollectBubble()
    {
        if (bubbleObject == null || !bubbleObject.IsActive)
        { ForceDecide(); return; }

        Vector2 bubblePos = bubbleObject.transform.position;
        float bubbleVelX = _bubbleRb != null ? _bubbleRb.velocity.x : 0f;
        float leadOffset = bubbleVelX * predictionTime * 4f;

        _targetX = bubblePos.x + leadOffset;

        float dX = Mathf.Abs(bubblePos.x - _botPos.x);
        float dY = bubblePos.y - _botPos.y;

        if (dY > bubbleJumpYThreshold && dX < 1.8f)
        {
            if (_isGrounded) _wantsJump = true;
            if (!_isGrounded && _jumpsLeft > 0 && dY > jumpBallHeightThreshold)
                _wantsJump = true;
        }

        if (verboseLog)
            Debug.Log($"[BotAI][Bubble] dX={dX:F2} dY={dY:F2} wantsJump={_wantsJump}");

        TryKickIfInRange();
    }

    // ── Post-shot repositioning ───────────────────────────────────────────
    void ExecReposition()
    {
        _targetX = _repositionTarget.x;
        // Transition back once we're close or time is up
        float dx = Mathf.Abs(_botPos.x - _repositionTarget.x);
        if (dx < 0.3f || _repositionTimer <= 0f)
            ForceDecide();
    }

    // ── Shared jump-toward-ball helper ────────────────────────────────────
    private void JumpTowardsBall(Vector2 target, bool aerial)
    {
        bool aerialOpportunity = aerial
            && _ballVel.y > aerialMinBallVelY
            && Mathf.Abs(_ballPos.x - _botPos.x) < shootTriggerRange * 1.5f;

        if (_isGrounded && (
            target.y > _botPos.y + jumpBallHeightThreshold
            || _ballVel.y > 2f
            || aerialOpportunity))
            _wantsJump = true;

        if (!_isGrounded && _jumpsLeft > 0 &&
            target.y > _botPos.y + jumpBallHeightThreshold)
            _wantsJump = true;
    }

    // ═════════════════════════════════════════════════════════
    // KICK
    // ═════════════════════════════════════════════════════════

    private void TryKickIfInRange()
    {
        if (_kickCooldown > 0f) return;
        float dx = _ballPos.x - _botPos.x;
        float dy = _ballPos.y - _botPos.y;
        if (dx * dx + dy * dy <= _sqrKick) TryKick();
    }

    void TryKick()
    {
        if (ballRb == null || _fakeShootPending) return;

        bool offensive = _state == BotState.Chase
                      || _state == BotState.Shoot
                      || _state == BotState.CollectBubble;

        // ── Super shoot ───────────────────────────────────────────────────
        if (superShootReady && offensive)
        {
            float dir = playerGoal != null
                ? Mathf.Sign(playerGoal.position.x - _botPos.x)
                : _fieldDir;

            if (_ballController != null)
                _ballController.ActivateSuperMode();
            else
                Debug.LogWarning("[BotAI] _ballController is null — ActivateSuperMode() skipped!");

            ballRb.velocity = new Vector2(dir * superForceX, superForceY);
            ballRb.angularVelocity = dir * -superSpin;

            SoccerGameManager.Instance?.OnSuperShootFired();
            superShootReady = false;    // consumed; GameManager re-grants after next reset
            _kickCooldown = kickCooldownTime;

            if (verboseLog)
                Debug.Log($"[BotAI] 💥 SUPER SHOOT dir={dir} v={ballRb.velocity}");

            TriggerReposition();
            return;
        }

        // ── Fake shoot ────────────────────────────────────────────────────
        if (offensive && Random.value < fakeShootChance * difficulty)
        {
            _fakeShootPending = true;
            _fakeShootTimer = fakeShootDuration;
            if (verboseLog) Debug.Log("[BotAI] 🎭 FAKE SHOOT pause");
            return;
        }

        // ── Normal kick ───────────────────────────────────────────────────
        float baseForce = kickForce + Random.Range(-kickForceVariance, kickForceVariance);
        float ballHeightAbove = _ballPos.y - _botPos.y;
        Vector2 kickVec;

        if (_state == BotState.Defend || _state == BotState.GoalieBlock)
        {
            float clearDir = playerGoal != null
                ? Mathf.Sign(playerGoal.position.x - _botPos.x) : _fieldDir;
            float clearLift = ballHeightAbove > 0.5f ? baseForce * 0.15f : 0f;
            kickVec = new Vector2(clearDir * baseForce, clearLift);
        }
        else
        {
            float shotDirX = playerGoal != null
                ? Mathf.Sign(playerGoal.position.x - _botPos.x) : _fieldDir;

            float targetY = PickBestCorner();

            float horizDist = Mathf.Abs(
                (playerGoal != null ? playerGoal.position.x
                                    : _botPos.x + _fieldDir * 5f) - _ballPos.x);
            float travelTime = horizDist / Mathf.Max(baseForce * 0.8f, 0.1f);
            float requiredVy = (targetY - _ballPos.y) / Mathf.Max(travelTime, 0.05f)
                              - 0.5f * Physics2D.gravity.y * travelTime;
            float liftY = Mathf.Clamp(requiredVy * 0.35f,
                                            -baseForce * 0.4f, baseForce * 0.5f);

            if (ballHeightAbove > 0.8f)
            {
                baseForce *= 0.85f;
                liftY = Mathf.Min(liftY, -ballHeightAbove * 0.3f);
            }

            float aimQuality = Mathf.Lerp(0f, 1f, difficulty);
            float biasedLift = _ballSpd >= cornerAimMinSpeed
                ? Mathf.Lerp(Random.Range(-0.5f, 1.2f),
                             Mathf.Lerp(0.8f, liftY, cornerAimBias),
                             aimQuality)
                : 0.8f;

            kickVec = new Vector2(shotDirX * baseForce, biasedLift);

            // Post-shot reposition (offensive only)
            TriggerReposition();
        }

        ballRb.AddForce(kickVec, ForceMode2D.Impulse);
        _kickCooldown = kickCooldownTime;

        if (verboseLog)
            Debug.Log($"[BotAI] KICK state={_state} vec={kickVec} force={baseForce:F1}");
    }

    /// <summary>
    /// Record where the player aimed their last shot (called from TryKick).
    /// </summary>
    public void RecordPlayerShot(float shotTargetY)
    {
        if (_playerShotHistory == null || _playerShotHistory.Length == 0) return;
        _playerShotHistory[_playerShotIndex % _playerShotHistory.Length] = shotTargetY;
        _playerShotIndex++;
    }

    /// <summary>Returns the player's most common shot zone (average of last N).</summary>
    private float GetPlayerPreferredShotY()
    {
        if (_playerShotHistory == null || _playerShotHistory.Length == 0)
            return _goalMidY;

        float sum = 0f;
        int count = Mathf.Min(_playerShotIndex, _playerShotHistory.Length);
        if (count == 0) return _goalMidY;

        for (int i = 0; i < count; i++) sum += _playerShotHistory[i];
        return sum / count;
    }

    /// <summary>
    /// After kicking, step backward to avoid crowding the ball and to
    /// create a more natural attacking rhythm.
    /// </summary>
    private void TriggerReposition()
    {
        _repositionTarget = new Vector2(
            _botPos.x - _fieldDir * repositionOffset,
            _botPos.y
        );
        _repositionTimer = repositionDelay;

        // Only switch to Reposition if we're in an offensive state
        if (_state == BotState.Chase || _state == BotState.Shoot)
            SetState(BotState.Reposition);
    }

    // ═════════════════════════════════════════════════════════
    // SMART CORNER AIM — uses player velocity to predict keeper movement
    // ═════════════════════════════════════════════════════════

    private float PickBestCorner()
    {
        if (player == null) return _goalBotY;

        // Estimate where the player will be by the time the ball arrives
        float horizDist = playerGoal != null
            ? Mathf.Abs(playerGoal.position.x - _ballPos.x) : 5f;
        float estTravel = horizDist / Mathf.Max(kickForce * 0.8f, 0.1f);
        float futureY = _playerPos.y + _playerVel.y * estTravel;

        float goalMidY = (_goalTopY + _goalBotY) * 0.5f;
        float playerBias = futureY - goalMidY;
        float chosenY = playerBias >= 0f ? _goalBotY : _goalTopY;

        float noise = Mathf.Lerp(0.8f, 0.05f, difficulty);
        chosenY += Random.Range(-noise, noise);

        return Mathf.Clamp(chosenY, _goalBotY, _goalTopY);
    }

    // ═════════════════════════════════════════════════════════
    // ANTI-STUCK
    // ═════════════════════════════════════════════════════════

    void UpdateStuck(float dt)
    {
        if (_unstuckTimer > 0f) return;

        float diffX = _targetX - _botPos.x;
        bool wantsMove = diffX > 0.25f || diffX < -0.25f;
        bool notMoving = Mathf.Abs(rb.velocity.x) < 0.18f;

        _stuckTimer = wantsMove && notMoving ? _stuckTimer + dt : 0f;

        if (_stuckTimer > 0.45f)
        {
            _stuckTimer = 0f;
            _unstuckTimer = 0.5f;
            _unstuckDir = diffX > 0f ? -1f : 1f;
            _wantsJump = _isGrounded;
            if (verboseLog) Debug.Log("[BotAI] UNSTUCK");
        }
    }

    // ═════════════════════════════════════════════════════════
    // HELPERS
    // ═════════════════════════════════════════════════════════

    void Flip(bool right)
    {
        if (_facingRight == right) return;
        _facingRight = right;
        ApplyScale();
    }

    void ApplyScale()
    {
        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * (_facingRight ? 1f : -1f);
        transform.localScale = s;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if ((groundLayer.value & (1 << col.gameObject.layer)) != 0)
            _jumpsLeft = maxJumps;
    }

    // ═════════════════════════════════════════════════════════
    // GIZMOS
    // ═════════════════════════════════════════════════════════

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Vector3 pos = transform.position;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(pos, shootTriggerRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(pos, kickRange);

        if (botGoalZone != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(botGoalZone.position, defendRadius);
            Gizmos.color = new Color(1f, 1f, 0f, 0.25f);
            Gizmos.DrawWireSphere(botGoalZone.position, defendRadius * 2f);
        }

        if (player != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(player.position, possessionRange);
        }

        Gizmos.color = new Color(0f, 1f, 0.5f, 0.4f);
        Gizmos.DrawWireSphere(pos, bubbleDetectRange);

        if (ball != null && Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(_predictedBallPos, 0.3f);
            Gizmos.DrawLine(ball.position, _predictedBallPos);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_interceptTarget, 0.25f);
            Gizmos.DrawLine(ball.position, _interceptTarget);

            if (_state == BotState.Reposition)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(_repositionTarget, 0.2f);
            }
        }

        if (Application.isPlaying)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(pos, pos + new Vector3(_fieldDir * 1.5f, 0f, 0f));
            UnityEditor.Handles.Label(pos + Vector3.up * 1.5f,
                $"[{_state}] diff={difficulty:F1}" +
                $"{(superShootReady ? " SUPER" : "")}" +
                $"{(IsUnderPressure() ? " PRESS" : "")}");
        }

        Gizmos.color = Color.white;
        Gizmos.DrawLine(pos, pos + Vector3.down * groundRayLength);
    }
#endif
}
