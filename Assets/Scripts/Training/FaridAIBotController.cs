using UnityEngine;
using Unity.Sentis;

// Standalone Sentis inference controller for the FaridAI model - trained
// separately with Stable-Baselines3 against a custom Python Gym environment
// (MyPlatformGameEnv), NOT via Unity ML-Agents. It therefore can't plug into
// BehaviorParameters/Agent/DecisionRequester like BotRLAgent does (those
// expect ML-Agents' own ONNX tensor-naming protocol) - this runs inference
// directly through Sentis instead: builds the 10-float observation vector
// FaridAI was trained on, runs a forward pass, argmaxes the 8 action
// logits, and maps the result to the same move/jump/kick vocabulary as
// BotAIController/BotRLAgent.
//
// Observation order (must match MyPlatformGameEnv exactly):
//   0 player_x, 1 player_vx, 2 ball_x, 3 ball_y, 4 ball_vx,
//   5 dist_player_ball, 6 dist_ball_goal, 7 goal_x, 8 has_ball, 9 time_left
//
// Actions (Discrete(8)):
//   0 idle, 1 move left, 2 move right, 3 jump, 4 shoot,
//   5 move left + jump, 6 move right + jump, 7 jump + shoot
[RequireComponent(typeof(Rigidbody2D))]
public class FaridAIBotController : MonoBehaviour
{
    [Header("Model")]
    public ModelAsset modelAsset;

    [Header("References")]
    public Transform ball;
    public Transform targetGoal;
    public LayerMask groundLayer;
    public float groundRayLength = 0.6f;

    [Header("Movement")]
    public float moveSpeed = 5.5f;
    public float jumpForce = 9f;
    public int maxJumps = 2;

    [Header("Kick")]
    public float kickForce = 14f;
    public float kickRange = 2f;
    public float kickCooldownTime = 0.45f;

    [Header("Decision")]
    [Tooltip("Seconds between inference calls - no need to run the network every physics tick")]
    public float decisionInterval = 0.1f;

    private Model _runtimeModel;
    private IWorker _worker;
    private Rigidbody2D _rb;
    private Rigidbody2D _ballRb;
    private int _jumpsLeft;
    private bool _isGrounded;
    private float _kickCooldown;
    private float _decisionTimer;
    private int _currentAction;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        if (ball != null) _ballRb = ball.GetComponent<Rigidbody2D>();
    }

    void OnEnable()
    {
        if (modelAsset == null)
        {
            Debug.LogError("[FaridAIBotController] No ModelAsset assigned - staying idle.");
            enabled = false;
            return;
        }

        _runtimeModel = ModelLoader.Load(modelAsset);
        _worker = WorkerFactory.CreateWorker(BackendType.CPU, _runtimeModel);
        _jumpsLeft = maxJumps;
        _decisionTimer = 0f;
    }

    void OnDisable()
    {
        _worker?.Dispose();
        _worker = null;
    }

    void FixedUpdate()
    {
        if (_kickCooldown > 0f) _kickCooldown -= Time.fixedDeltaTime;

        Vector2 origin = _rb.position;
        const float hw = 0.3f;
        _isGrounded =
            Physics2D.Raycast(origin, Vector2.down, groundRayLength, groundLayer)
         || Physics2D.Raycast(origin + new Vector2(-hw, 0f), Vector2.down, groundRayLength, groundLayer)
         || Physics2D.Raycast(origin + new Vector2(hw, 0f), Vector2.down, groundRayLength, groundLayer);
        if (_isGrounded) _jumpsLeft = maxJumps;

        _decisionTimer -= Time.fixedDeltaTime;
        if (_decisionTimer <= 0f)
        {
            _decisionTimer = decisionInterval;
            _currentAction = Infer();
        }

        ApplyAction(_currentAction);
    }

    private int Infer()
    {
        float[] obs = BuildObservation();
        using TensorFloat input = new TensorFloat(new TensorShape(1, obs.Length), obs);

        _worker.Execute(input);
        TensorFloat output = _worker.PeekOutput("action_logits") as TensorFloat;
        output.MakeReadable();
        float[] logits = output.ToReadOnlyArray();

        int best = 0;
        for (int i = 1; i < logits.Length; i++)
            if (logits[i] > logits[best]) best = i;
        return best;
    }

    // time_left is fixed at 1 (== "plenty of time left") - the real match has
    // no per-episode countdown to feed here; this feature only mattered for
    // the training curriculum, so a constant stand-in is the closest honest
    // choice rather than inventing a fake countdown.
    private float[] BuildObservation()
    {
        Vector2 pos = _rb.position;
        Vector2 ballPos = ball != null ? (Vector2)ball.position : pos;
        float ballVx = _ballRb != null ? _ballRb.velocity.x : 0f;
        float goalX = targetGoal != null ? targetGoal.position.x : pos.x;

        float distPlayerBall = Vector2.Distance(pos, ballPos);
        float distBallGoal = Mathf.Abs(goalX - ballPos.x);
        float hasBall = distPlayerBall <= kickRange ? 1f : 0f;

        return new float[]
        {
            pos.x, _rb.velocity.x, ballPos.x, ballPos.y, ballVx,
            distPlayerBall, distBallGoal, goalX, hasBall, 1f,
        };
    }

    private void ApplyAction(int action)
    {
        bool moveLeft = action == 1 || action == 5;
        bool moveRight = action == 2 || action == 6;
        bool wantsJump = action == 3 || action == 5 || action == 6 || action == 7;
        bool wantsShoot = action == 4 || action == 7;

        float targetVx = moveLeft ? -moveSpeed : (moveRight ? moveSpeed : 0f);
        _rb.velocity = new Vector2(targetVx, _rb.velocity.y);

        if (wantsJump && _isGrounded && _jumpsLeft > 0)
        {
            _rb.velocity = new Vector2(_rb.velocity.x, jumpForce);
            _jumpsLeft--;
        }

        if (wantsShoot && _kickCooldown <= 0f && _ballRb != null && ball != null)
        {
            float sqrDist = ((Vector2)ball.position - _rb.position).sqrMagnitude;
            if (sqrDist <= kickRange * kickRange)
            {
                float dir = targetGoal != null ? Mathf.Sign(targetGoal.position.x - _rb.position.x) : 1f;
                _ballRb.AddForce(new Vector2(dir * kickForce, kickForce * 0.25f), ForceMode2D.Impulse);
                _kickCooldown = kickCooldownTime;
            }
        }
    }
}
