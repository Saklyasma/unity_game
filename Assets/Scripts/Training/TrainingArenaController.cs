using UnityEngine;

// Training-only arena manager. Deliberately independent from SoccerGameManager/
// GoalDetector so the real game's quiz/freeze flow is never touched here -
// a goal just resets ball + bots instantly, no Time.timeScale=0 pause.
//
// Ported from the WorldCup repo's ML-Agents pipeline. Not used by the real
// game integration (BotRLAgent.arena stays null there - SoccerGameManager/
// GoalDetector already handle reset/freeze on goals); this class exists so a
// dedicated training scene can be built later the same way WorldCup's was.
public class TrainingArenaController : MonoBehaviour
{
    public enum Side { Left, Right }

    [Header("References")]
    public Rigidbody2D ball;
    public Rigidbody2D leftBot;
    public Rigidbody2D rightBot;

    [Tooltip("Set when the right-side bot is RL-controlled. Left null while both sides are still scripted (Day 1).")]
    public BotRLAgent rightAgent;

    private Vector2 _ballSpawn;
    private Vector2 _leftSpawn;
    private Vector2 _rightSpawn;

    void Awake()
    {
        if (ball != null) _ballSpawn = ball.position;
        if (leftBot != null) _leftSpawn = leftBot.position;
        if (rightBot != null) _rightSpawn = rightBot.position;
    }

    // "side" is which goal wall the ball hit - Left wall means the right-side
    // attacker scored, and vice versa.
    public void OnGoal(Side wallHit)
    {
        Debug.Log($"[TrainingArena] {wallHit} goal wall hit.");

        if (rightAgent != null)
        {
            // EndEpisode() inside these calls triggers OnEpisodeBegin(),
            // which itself calls ResetArena() - don't reset twice here.
            if (wallHit == Side.Left) rightAgent.OnGoalScoredFor();
            else rightAgent.OnGoalScoredAgainst();
        }
        else
        {
            ResetArena();
        }
    }

    public void ResetArena()
    {
        if (ball != null)
        {
            ball.position = _ballSpawn;
            ball.velocity = Vector2.zero;
            ball.angularVelocity = 0f;
        }

        ResetCharacter(leftBot, _leftSpawn);
        ResetCharacter(rightBot, _rightSpawn);
    }

    private void ResetCharacter(Rigidbody2D rb, Vector2 spawn)
    {
        if (rb == null) return;

        var botAI = rb.GetComponent<BotAIController>();
        if (botAI != null)
        {
            botAI.ResetToSpawn();
            return;
        }

        var playerUI = rb.GetComponent<PlayerUIController>();
        if (playerUI != null)
        {
            playerUI.ResetPlayer();
            return;
        }

        rb.position = spawn;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }
}
