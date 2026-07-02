using UnityEngine;

/// <summary>
/// GoalDetector
/// ────────────
/// Attach to: GoalZone1/Square  (isBotGoal = false → bot scores here)
///            GoalZone2/Square(1) (isBotGoal = true  → player scores here)
///
/// Requires: Is Trigger = OFF on the BoxCollider2D (uses OnCollisionEnter2D)
///           Ball GameObject must be on the Layer named "Ball"
/// </summary>
public class GoalDetector : MonoBehaviour
{
    [Header("== SETTINGS ==")]
    [Tooltip("false = GoalZone1 (player's net, bot scores) | true = GoalZone2 (bot's net, player scores)")]
    public bool isBotGoal;

    [Header("== LAYER ==")]
    public string ballLayerName = "Ball";

    private int _ballLayer = -1;
    private bool _triggered = false;

    private void Start()
    {
        _ballLayer = LayerMask.NameToLayer(ballLayerName);
        if (_ballLayer == -1)
            Debug.LogError($"[GoalDetector] Layer '{ballLayerName}' not found! Make sure the Ball is on a layer called 'Ball'.");
    }

    /// <summary>Called by GameManager at the start of each round.</summary>
    public void ResetTrigger() => _triggered = false;

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (_triggered) return;
        if (col.gameObject.layer != _ballLayer) return;
        if (SoccerGameManager.Instance == null) return;
        if (!SoccerGameManager.Instance.IsPlaying) return;

        _triggered = true;

        SoccerGameManager.GoalOwner scorer = isBotGoal
            ? SoccerGameManager.GoalOwner.Player
            : SoccerGameManager.GoalOwner.Bot;

        Debug.Log($"[GoalDetector] Goal! isBotGoal={isBotGoal} → scorer={scorer}");
        SoccerGameManager.Instance.OnGoalScored(scorer);
    }
}