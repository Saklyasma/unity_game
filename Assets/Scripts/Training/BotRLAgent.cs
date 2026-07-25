using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

// RL counterpart to BotAIController: same movement/kick vocabulary (move X,
// jump, kick), but the decision of when to use them is learned instead of
// scripted. Heuristic() below is a simple hand-written policy (chase the
// ball, jump when it's overhead, kick in range) - with Behavior Type set to
// "Heuristic Only" this already plays competently with zero training, and
// it doubles as the source policy for demonstration recording (see
// DemonstrationRecorder on this GameObject) if imitation-learning
// pretraining is added later so PPO doesn't start from random actions.
//
// Ported from the WorldCup repo's proven ML-Agents pipeline (trained against
// real Unity physics, curriculum difficulty easy->hard, see
// ml-training/README.md there). Known gap carried over: no notion of this
// project's super-shoot bubble power-up or per-country BotStatsData tuning -
// training never covered either, so RL mode plays "neutral" regardless of
// opponent country.
[RequireComponent(typeof(Rigidbody2D))]
public class BotRLAgent : Agent
{
    [Header("References")]
    public Transform ball;
    public Transform player;
    public Transform targetGoal;
    public Transform ownGoal;
    public TrainingArenaController arena;

    [Header("Movement")]
    public float moveSpeed = 5.5f;
    public float acceleration = 14f;
    public float deceleration = 20f;
    public float jumpForce = 9f;
    public int maxJumps = 2;
    public LayerMask groundLayer;
    public float groundRayLength = 0.6f;

    [Header("Kick")]
    public float kickForce = 14f;
    public float kickRange = 2f;
    public float kickCooldownTime = 0.45f;

    [Header("Reward shaping")]
    public float approachRewardScale = 0.01f;
    public float ballTouchReward = 0.5f;
    public float goalReward = 1f;
    public float concedeReward = -1f;
    public float stepPenalty = -0.0005f;

    private Rigidbody2D rb;
    private Rigidbody2D ballRb;
    private int jumpsLeft;
    private bool isGrounded;
    private float kickCooldown;
    private float prevDistToBall;
    private Vector2 spawnPos;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody2D>();
        if (ball != null) ballRb = ball.GetComponent<Rigidbody2D>();
        spawnPos = rb.position;
    }

    public override void OnEpisodeBegin()
    {
        if (arena != null)
            arena.ResetArena();
        else
        {
            rb.position = spawnPos;
            rb.velocity = Vector2.zero;
        }

        jumpsLeft = maxJumps;
        kickCooldown = 0f;
        prevDistToBall = ball != null ? Vector2.Distance(rb.position, ball.position) : 0f;

        ApplyCurriculumDifficulty();
    }

    // Reads the "opponent_difficulty" environment parameter (set by
    // mlagents-learn's curriculum config, see ml-training/config/bot_training.yaml
    // in the WorldCup repo this was ported from) and applies it to the
    // scripted opponent each episode. Falls back to whatever difficulty is
    // already set when there's no trainer/curriculum (manual Play testing, or
    // the real game where "player" has no BotAIController) - harmless no-op
    // in both cases.
    private void ApplyCurriculumDifficulty()
    {
        if (player == null) return;
        var opponentAI = player.GetComponent<BotAIController>();
        if (opponentAI == null) return;

        opponentAI.difficulty = Academy.Instance.EnvironmentParameters
            .GetWithDefault("opponent_difficulty", opponentAI.difficulty);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector2 pos = rb.position;

        sensor.AddObservation(rb.velocity / moveSpeed);
        sensor.AddObservation(isGrounded ? 1f : 0f);
        sensor.AddObservation(jumpsLeft / (float)maxJumps);

        AddRelative(sensor, ball, pos);
        sensor.AddObservation(ballRb != null ? ballRb.velocity / 10f : Vector2.zero);
        AddRelative(sensor, player, pos);
        AddRelative(sensor, targetGoal, pos);
        AddRelative(sensor, ownGoal, pos);
    }

    private static void AddRelative(VectorSensor sensor, Transform t, Vector2 fromPos)
    {
        sensor.AddObservation(t != null ? ((Vector2)t.position - fromPos) / 20f : Vector2.zero);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        bool wantsJump = actions.DiscreteActions[0] == 1;
        bool wantsKick = actions.DiscreteActions[1] == 1;

        float targetSpd = moveX * moveSpeed;
        float rate = Mathf.Abs(moveX) > 0.05f ? acceleration : deceleration;
        float newVelX = Mathf.MoveTowards(rb.velocity.x, targetSpd, rate * Time.fixedDeltaTime);
        float newVelY = rb.velocity.y;

        if (wantsJump && isGrounded && jumpsLeft > 0)
        {
            newVelY = jumpForce;
            jumpsLeft--;
        }

        rb.velocity = new Vector2(newVelX, newVelY);

        if (wantsKick && kickCooldown <= 0f && ball != null)
        {
            float sqrDist = ((Vector2)ball.position - rb.position).sqrMagnitude;
            if (sqrDist <= kickRange * kickRange) TryKick();
        }

        AddReward(stepPenalty);

        if (ball != null)
        {
            // Clamped so a hard opponent kick sending the ball flying away
            // (or back) doesn't dump a huge, agent-uncontrolled swing into
            // the reward - only rewards the agent's own gradual approach.
            float dist = Vector2.Distance(rb.position, ball.position);
            float delta = Mathf.Clamp(prevDistToBall - dist, -0.5f, 0.5f);
            AddReward(delta * approachRewardScale);
            prevDistToBall = dist;
        }
    }

    private void TryKick()
    {
        if (ballRb == null) return;

        float dir = targetGoal != null ? Mathf.Sign(targetGoal.position.x - rb.position.x) : 1f;
        ballRb.AddForce(new Vector2(dir * kickForce, kickForce * 0.25f), ForceMode2D.Impulse);
        kickCooldown = kickCooldownTime;
        AddReward(ballTouchReward);
    }

    public void OnGoalScoredFor()
    {
        AddReward(goalReward);
        EndEpisode();
    }

    public void OnGoalScoredAgainst()
    {
        AddReward(concedeReward);
        EndEpisode();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var cont = actionsOut.ContinuousActions;
        var disc = actionsOut.DiscreteActions;

        if (ball == null)
        {
            cont[0] = 0f;
            disc[0] = 0;
            disc[1] = 0;
            return;
        }

        Vector2 pos = rb.position;
        Vector2 ballPos = ball.position;
        float dx = ballPos.x - pos.x;

        cont[0] = Mathf.Clamp(dx / 2f, -1f, 1f);

        bool ballAbove = ballPos.y > pos.y + 0.3f;
        bool ballNearX = Mathf.Abs(dx) < 3f;
        disc[0] = (isGrounded && ballAbove && ballNearX) ? 1 : 0;

        float sqrDist = (ballPos - pos).sqrMagnitude;
        disc[1] = (sqrDist <= kickRange * kickRange && kickCooldown <= 0f) ? 1 : 0;
    }

    void FixedUpdate()
    {
        if (kickCooldown > 0f) kickCooldown -= Time.fixedDeltaTime;

        Vector2 origin = rb.position;
        const float hw = 0.3f;
        isGrounded =
            Physics2D.Raycast(origin, Vector2.down, groundRayLength, groundLayer)
         || Physics2D.Raycast(origin + new Vector2(-hw, 0f), Vector2.down, groundRayLength, groundLayer)
         || Physics2D.Raycast(origin + new Vector2(hw, 0f), Vector2.down, groundRayLength, groundLayer);

        if (isGrounded) jumpsLeft = maxJumps;
    }
}
