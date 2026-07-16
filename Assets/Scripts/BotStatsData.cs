using UnityEngine;

[CreateAssetMenu(fileName = "BotStats", menuName = "WorldCup/Bot Stats")]
public class BotStatsData : ScriptableObject
{
    [Header("Identity")]
    public string countryName;

    [Header("Movement")]
    public float moveSpeed = 5.5f;
    public float jumpForce = 9f;

    [Header("Kick")]
    public float kickForce = 14f;
    public float kickRange = 2.0f;

    [Header("AI - Difficulty")]
    [Range(0f, 1f)]
    public float difficulty = 0.7f;

    [Header("AI - Pressure")]
    public float pressureSpeedBoost = 1.25f;

    [Header("Play Style")]
    [Range(0f, 1f)]
    [Tooltip("Higher = chases more aggressively, presses higher up the field")]
    public float aggressive = 0.5f;
    [Range(0f, 1f)]
    [Tooltip("Higher = stays back, prioritizes defense over attack")]
    public float defensive = 0.3f;
    [Range(0f, 1f)]
    [Tooltip("Higher = dribbles toward goal before shooting, keeps possession longer")]
    public float possession = 0.4f;

    [Header("Visuals")]
    public Color jerseyColor = Color.white;
}