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

    [Header("Visuals")]
    public Color jerseyColor = Color.white;
}