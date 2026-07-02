using UnityEngine;

public class PowerUpSpawner : MonoBehaviour
{
    [Header("== SETTINGS ==")]
    public PowerUpBubble bubble;
    public float minTime = 15f; // ✅ 20s
    public float maxTime = 55f; // ✅ 60s

    private float timer = 0f;
    private float nextSpawn = 0f;
    private int totalAppearances;
    private int currentCount = 0;
    private bool finished = false;

    void Start()
    {
        totalAppearances = Random.Range(2, 5); // 1 à 3 fois
        SetNextSpawn();
        Debug.Log("🫧 Total appearances: " + totalAppearances);
    }

    void Update()
    {
        if (finished) return;
        if (bubble.gameObject.activeInHierarchy) return;

        timer += Time.deltaTime;

        if (timer >= nextSpawn)
        {
            if (currentCount >= totalAppearances)
            {
                finished = true;
                Debug.Log("✅ Bubble: fini!");
                return;
            }

            timer = 0f;
            currentCount++;
            SetNextSpawn();
            bubble.Respawn();
            Debug.Log("🫧 Spawn #" + currentCount + "/" + totalAppearances);
        }
    }

    void SetNextSpawn()
    {
        nextSpawn = Random.Range(minTime, maxTime);
        Debug.Log("⏱️ Next spawn in: " + nextSpawn + "s");
    }
}