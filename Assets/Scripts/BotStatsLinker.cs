using UnityEngine;
using System.Collections.Generic;

[DefaultExecutionOrder(-50)]
public class BotStatsLinker : MonoBehaviour
{
    [System.Serializable]
    public class CountryStatEntry
    {
        public string countryName;
        public BotStatsData stats;
    }

    [Header("Assign all 10 country stats here")]
    public List<CountryStatEntry> entries;

    [Header("Spawn")]
    public Transform botSpawnPoint;

    [Header("Bot Runtime References")]
    public Transform ball;
    public Transform playerGoal;
    public Transform player;
    public Transform botGoalZone;
    public PowerUpBubble bubbleObject;
    public LayerMask groundLayer;

    public BotAIController botAI;

    void Awake()
    {
        AutoResolveReferences();
        Debug.Log($"[BotStatsLinker] Awake: botAI after resolve={(botAI != null ? botAI.name : "null")}, CountryDataHolder.Instance={(CountryDataHolder.Instance != null ? "exists" : "null")}");

        if (CountryDataHolder.Instance == null)
        {
            Debug.LogWarning("[BotStatsLinker] CountryDataHolder not found!");
            return;
        }

        CountryFlag selected = CountryDataHolder.Instance.GetSelected();
        Debug.Log($"[BotStatsLinker] GetSelected returned: countryName='{(selected != null ? selected.countryName : "null")}', flag={(selected != null && selected.flag != null ? selected.flag.name : "null")}");

        if (selected == null)
        {
            Debug.LogWarning("[BotStatsLinker] No country selected!");
            return;
        }



        BotAIController activeBot = EnsureSelectedBotInstance(selected);
        if (activeBot == null)
        {
            Debug.LogWarning("[BotStatsLinker] Could not create or resolve active bot instance.");
            return;
        }

        botAI = activeBot;

        string selectedKey = CountryDataHolder.NormalizeCountryKey(selected.countryName);
        Debug.Log($"[BotStatsLinker] Looking for stats with key='{selectedKey}' in {entries.Count} entries");

        foreach (var entry in entries)
        {
            if (CountryDataHolder.NormalizeCountryKey(entry.countryName) == selectedKey)
            {
                Debug.Log($"[BotStatsLinker] Applying stats for '{entry.countryName}' (key='{selectedKey}')");
                botAI.ApplyStats(entry.stats);
                return;
            }
        }

        Debug.LogWarning($"[BotStatsLinker] No stats found for '{selected.countryName}'");
    }

    private BotAIController EnsureSelectedBotInstance(CountryFlag selected)
    {
        if (botAI == null)
        {
            Debug.LogError("[BotStatsLinker] No existing botAI in scene — cannot create bot.");
            return null;
        }

        GameObject selectedPrefab = CountryDataHolder.Instance.GetSelectedBotPrefab();
        Debug.Log($"[BotStatsLinker] EnsureSelectedBotInstance: selected='{selected.countryName}', prefab={(selectedPrefab != null ? selectedPrefab.name : "null")}, existingBotAI='{botAI.name}'");

        if (selectedPrefab != null)
        {
            SpriteRenderer prefabSprite = selectedPrefab.GetComponentInChildren<SpriteRenderer>(true);
            SpriteRenderer botSprite = botAI.GetComponentInChildren<SpriteRenderer>(true);
            if (prefabSprite != null && botSprite != null)
            {
                botSprite.sprite = prefabSprite.sprite;
                botAI.name = selectedPrefab.name;
                Debug.Log($"[BotStatsLinker] Applied sprite '{prefabSprite.sprite?.name}' from '{selectedPrefab.name}' to bot '{botAI.name}'");
            }
            else
            {
                Debug.Log($"[BotStatsLinker] Could not copy sprite: prefabSprite={(prefabSprite != null ? "ok" : "null")}, botSprite={(botSprite != null ? "ok" : "null")}");
            }
        }
        else
        {
            Debug.Log($"[BotStatsLinker] No prefab found — keeping bot visual as-is");
        }

        return botAI;
    }

    private void AutoResolveReferences()
    {
        SoccerGameManager gameManager = SoccerGameManager.Instance != null
            ? SoccerGameManager.Instance
            : FindObjectOfType<SoccerGameManager>();

        if (gameManager != null)
        {
            if (ball == null && gameManager.ball != null)
                ball = gameManager.ball.transform;

            if (botAI == null)
                botAI = gameManager.botAI;
        }

        if (bubbleObject == null)
            bubbleObject = FindObjectOfType<PowerUpBubble>();

        if (player == null)
        {
            PlayerUIController playerController = FindObjectOfType<PlayerUIController>();
            if (playerController != null)
                player = playerController.transform;
        }

        if ((playerGoal == null || botGoalZone == null) && gameManager != null)
        {
            if (playerGoal == null && gameManager.goalDetector1 != null)
                playerGoal = gameManager.goalDetector1.transform;

            if (botGoalZone == null && gameManager.goalDetector2 != null)
                botGoalZone = gameManager.goalDetector2.transform;
        }

        if (groundLayer.value == 0 && botAI != null)
            groundLayer = botAI.groundLayer;

        if (botSpawnPoint == null && botAI != null)
            botSpawnPoint = botAI.transform;
    }

    private Vector3 ResolveSpawnPosition(GameObject selectedPrefab)
    {
        if (botSpawnPoint != null)
            return botSpawnPoint.position;

        if (botAI != null)
            return botAI.transform.position;

        return selectedPrefab.transform.position;
    }

    private Quaternion ResolveSpawnRotation(GameObject selectedPrefab)
    {
        if (botSpawnPoint != null)
            return botSpawnPoint.rotation;

        if (botAI != null)
            return botAI.transform.rotation;

        return selectedPrefab.transform.rotation;
    }

    private Transform ResolveSpawnParent()
    {
        if (botSpawnPoint != null)
            return botSpawnPoint.parent;

        if (botAI != null)
            return botAI.transform.parent;

        return null;
    }

    private void AssignRuntimeReferences(BotAIController target, BotAIController fallbackSource)
    {
        target.ball = ball != null ? ball : fallbackSource != null ? fallbackSource.ball : null;
        target.playerGoal = playerGoal != null ? playerGoal : fallbackSource != null ? fallbackSource.playerGoal : null;
        target.player = player != null ? player : fallbackSource != null ? fallbackSource.player : null;
        target.botGoalZone = botGoalZone != null ? botGoalZone : fallbackSource != null ? fallbackSource.botGoalZone : null;
        target.bubbleObject = bubbleObject != null ? bubbleObject : fallbackSource != null ? fallbackSource.bubbleObject : null;
        target.groundLayer = groundLayer.value != 0 ? groundLayer : fallbackSource != null ? fallbackSource.groundLayer : default;
    }

    private void UpdateSceneReferences(BotAIController newBot, BotAIController oldBot)
    {
        SoccerGameManager gameManager = SoccerGameManager.Instance != null
            ? SoccerGameManager.Instance
            : FindObjectOfType<SoccerGameManager>();

        if (gameManager != null)
            gameManager.botAI = newBot;

        PowerUpBubble bubble = newBot.bubbleObject != null
            ? newBot.bubbleObject
            : FindObjectOfType<PowerUpBubble>();

        if (bubble != null && (oldBot == null || bubble.botController == oldBot))
            bubble.botController = newBot;
    }
}
