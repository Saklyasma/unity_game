using UnityEngine;

// Training-only goal wall. Same ball-layer collision detection idea as
// GoalDetector, but reports straight to TrainingArenaController instead of
// SoccerGameManager - no quiz, no Time.timeScale freeze, instant reset.
// Ported from the WorldCup repo's ML-Agents pipeline (see TrainingArenaController).
public class TrainingGoalTrigger : MonoBehaviour
{
    public TrainingArenaController arena;
    public TrainingArenaController.Side side;
    public string ballLayerName = "Ball";

    private int _ballLayer = -1;

    void Start()
    {
        _ballLayer = LayerMask.NameToLayer(ballLayerName);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.layer != _ballLayer) return;
        if (arena != null) arena.OnGoal(side);
    }
}
