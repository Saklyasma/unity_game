using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.Sentis;

// Drops BotRLAgent onto the real game's Bot GameObject in Dlc_2_Level, wired
// from the SAME references the existing BotAIController already uses (ball,
// player, playerGoal, botGoalZone, groundLayer) - no guessing, no new
// placeholder objects, this is the actual shipped match.
//
// Ported from the WorldCup repo, where this trained BotRL.onnx (PPO, trained
// against real Unity physics with a curriculum opponent) and this script
// were built and verified. BotAIController's public field names and this
// scene's path are identical between the two projects, so the port is a
// straight copy - see ml-training/README.md in WorldCup for the full
// training history behind BotRL.onnx.
//
// Safety: BotRLAgent/BehaviorParameters/DecisionRequester are added but
// DISABLED. BotAIController stays enabled/untouched. Default gameplay is
// byte-for-byte the same as before this ran - use the menu toggles below to
// switch, or the two component checkboxes directly in the Inspector.
//
// Known gap: BotRLAgent has no concept of this project's super-shoot bubble
// power-up or per-country BotStatsData tuning (training never included
// either) - RL mode simply won't use the super shoot and plays identically
// regardless of the opponent country.
public static class RealGameRLIntegration
{
    private const string ScenePath = "Assets/Scenes/Dlc_2_Level[AR] 0.unity";
    private const string TrainedModelPath = "Assets/MLTraining/Models/BotRL.onnx";
    private const string FaridAIModelPath = "Assets/MLTraining/Models/FaridAI.onnx";

    [MenuItem("Tools/ML Bot Training/5 - Integrate RL Agent Into Real Game")]
    public static void Integrate()
    {
        EditorSceneManager.OpenScene(ScenePath);

        var botAI = Object.FindObjectOfType<BotAIController>();
        if (botAI == null)
        {
            Debug.LogError("[RealGameRLIntegration] No BotAIController found in " + ScenePath);
            return;
        }

        var botGO = botAI.gameObject;

        if (botGO.GetComponent<BotRLAgent>() != null)
        {
            Debug.LogWarning("[RealGameRLIntegration] BotRLAgent already present on " + botGO.name + " - skipping, already integrated.");
            return;
        }

        // Added before DecisionRequester - see the component-order gotcha in
        // WorldCup's ml-training/README.md.
        var rlAgent = botGO.AddComponent<BotRLAgent>();
        rlAgent.ball = botAI.ball;
        rlAgent.player = botAI.player;
        rlAgent.targetGoal = botAI.playerGoal;
        rlAgent.ownGoal = botAI.botGoalZone;
        rlAgent.groundLayer = botAI.groundLayer;
        // arena stays null on purpose: SoccerGameManager already resets
        // positions/freezes on goals via GoalDetector, same as it does for
        // BotAIController - BotRLAgent doesn't need its own episode/reset
        // logic here, just the movement/kick inference.

        var behaviorParams = botGO.GetComponent<BehaviorParameters>();
        behaviorParams.BehaviorName = "BotRL";
        behaviorParams.BrainParameters.VectorObservationSize = 14;
        behaviorParams.BrainParameters.NumStackedVectorObservations = 1;
        behaviorParams.BrainParameters.ActionSpec = new ActionSpec(1, new[] { 2, 2 });
        behaviorParams.BehaviorType = BehaviorType.Default;

        var trainedModel = AssetDatabase.LoadAssetAtPath<ModelAsset>(TrainedModelPath);
        if (trainedModel != null)
            behaviorParams.Model = trainedModel;
        else
            Debug.LogWarning("[RealGameRLIntegration] No trained model found at " + TrainedModelPath + " - RL mode will fall back to BotRLAgent.Heuristic() until one is assigned.");

        var decisionRequester = botGO.AddComponent<DecisionRequester>();
        decisionRequester.DecisionPeriod = 5;

        // Disabled by default - BotAIController (already enabled) keeps
        // driving the bot exactly as before. Flip via the menu items below.
        rlAgent.enabled = false;
        decisionRequester.enabled = false;

        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log("[RealGameRLIntegration] BotRLAgent added to '" + botGO.name + "', disabled by default. " +
                  "Use Tools > ML Bot Training > 6/7 to switch modes.");
    }

    [MenuItem("Tools/ML Bot Training/6 - Switch Real Game Bot to Scripted AI")]
    public static void UseScriptedBot() => SetMode(Mode.Scripted);

    [MenuItem("Tools/ML Bot Training/7 - Switch Real Game Bot to Trained RL Agent")]
    public static void UseTrainedAgent() => SetMode(Mode.MLAgentsRL);

    // Drops FaridAIBotController onto the real Bot GameObject the same way
    // Integrate() does for BotRLAgent - disabled by default, same ball/
    // targetGoal/groundLayer wiring taken straight from the existing
    // BotAIController so nothing is guessed. FaridAI was trained with
    // Stable-Baselines3 against a custom Python Gym env (MyPlatformGameEnv),
    // not Unity ML-Agents, so it runs through plain Sentis instead of
    // BehaviorParameters/Agent - see FaridAIBotController.cs.
    [MenuItem("Tools/ML Bot Training/9 - Integrate FaridAI Into Real Game")]
    public static void IntegrateFaridAI()
    {
        EditorSceneManager.OpenScene(ScenePath);

        var botAI = Object.FindObjectOfType<BotAIController>();
        if (botAI == null)
        {
            Debug.LogError("[RealGameRLIntegration] No BotAIController found in " + ScenePath);
            return;
        }

        var botGO = botAI.gameObject;

        if (botGO.GetComponent<FaridAIBotController>() != null)
        {
            Debug.LogWarning("[RealGameRLIntegration] FaridAIBotController already present on " + botGO.name + " - skipping, already integrated.");
            return;
        }

        var faridAI = botGO.AddComponent<FaridAIBotController>();
        faridAI.ball = botAI.ball;
        faridAI.targetGoal = botAI.playerGoal;
        faridAI.groundLayer = botAI.groundLayer;

        var trainedModel = AssetDatabase.LoadAssetAtPath<ModelAsset>(FaridAIModelPath);
        if (trainedModel != null)
            faridAI.modelAsset = trainedModel;
        else
            Debug.LogWarning("[RealGameRLIntegration] No trained model found at " + FaridAIModelPath + ".");

        // Disabled by default - existing controllers keep driving the bot
        // exactly as before. Flip via menu item 10 (or the checkbox).
        faridAI.enabled = false;

        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log("[RealGameRLIntegration] FaridAIBotController added to '" + botGO.name + "', disabled by default. " +
                  "Use Tools > ML Bot Training > 6/7/10 to switch modes.");
    }

    [MenuItem("Tools/ML Bot Training/10 - Switch Real Game Bot to FaridAI")]
    public static void UseFaridAI() => SetMode(Mode.FaridAI);

    private enum Mode { Scripted, MLAgentsRL, FaridAI }

    // Three controllers can potentially sit on the same Bot GameObject
    // (BotAIController, BotRLAgent+DecisionRequester, FaridAIBotController) -
    // exactly one is ever enabled at a time, since they'd otherwise fight
    // over the same Rigidbody2D. Missing controllers (not yet integrated
    // via menu 5 / 9) are simply left alone - selecting a mode whose
    // controller isn't present yet logs an error and changes nothing.
    private static void SetMode(Mode mode)
    {
        EditorSceneManager.OpenScene(ScenePath);

        var botAI = Object.FindObjectOfType<BotAIController>(includeInactive: true);
        if (botAI == null)
        {
            Debug.LogError("[RealGameRLIntegration] No BotAIController found in " + ScenePath);
            return;
        }

        var rlAgent = botAI.GetComponent<BotRLAgent>();
        var decisionRequester = botAI.GetComponent<DecisionRequester>();
        var faridAI = botAI.GetComponent<FaridAIBotController>();

        if (mode == Mode.MLAgentsRL && (rlAgent == null || decisionRequester == null))
        {
            Debug.LogError("[RealGameRLIntegration] BotRLAgent not integrated yet - run 'Tools > ML Bot Training > 5' first.");
            return;
        }
        if (mode == Mode.FaridAI && faridAI == null)
        {
            Debug.LogError("[RealGameRLIntegration] FaridAIBotController not integrated yet - run 'Tools > ML Bot Training > 9' first.");
            return;
        }

        botAI.enabled = mode == Mode.Scripted;
        if (rlAgent != null) rlAgent.enabled = mode == Mode.MLAgentsRL;
        if (decisionRequester != null) decisionRequester.enabled = mode == Mode.MLAgentsRL;
        if (faridAI != null) faridAI.enabled = mode == Mode.FaridAI;

        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log("[RealGameRLIntegration] Bot mode set to " + mode + ".");
    }

    // Verification-only helper: builds the actual game (all scenes from
    // Build Settings, same as a real release build) so integration mistakes
    // show up as compile/build errors instead of only at manual playtest.
    [MenuItem("Tools/ML Bot Training/8 - Build Real Game (verification)")]
    public static void BuildRealGame()
    {
        const string buildPath = "ml-training/builds/realgame_check/WorldCup.exe";
        Directory.CreateDirectory(Path.GetDirectoryName(buildPath));

        var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = buildPath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.Development
        });
        Debug.Log("[RealGameRLIntegration] Build result: " + report.summary.result + " -> " + buildPath);
        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            EditorApplication.Exit(1);
        }
    }
}
