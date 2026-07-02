using UnityEngine;

/// <summary>
/// Zone difens fixe 9dam el player starting position.
/// El bot ki youslilha → yarja3 lel botGoalZone w ma ydhrabch.
/// Zéro Update() — static, zéro overhead.
/// </summary>
public class PlayerGoalZone : MonoBehaviour
{
    [Header("Zone Settings")]
    [Tooltip("Radius eli el BotAIController yista3mlou — lazem yitkawwen sama mte3 playerGoalZoneRadius fil bot")]
    public float zoneRadius = 3f;

    // ── Gizmos only — no Update, no logic ────────────────────
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.15f);
        Gizmos.DrawSphere(transform.position, zoneRadius);

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.7f);
        Gizmos.DrawWireSphere(transform.position, zoneRadius);

#if UNITY_EDITOR
        UnityEditor.Handles.color = new Color(1f, 0.5f, 0f, 1f);
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * (zoneRadius + 0.3f),
            "⚠️ PlayerGoalZone"
        );
#endif
    }
}