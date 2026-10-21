using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject[] groundObstacles;
    [SerializeField] private GameObject[] airObstacles;
    [SerializeField] private GameObject coin;

    [Header("Timing (seconds between spawns)")]
    [SerializeField] private float minInterval = 1.3f;
    [SerializeField] private float maxInterval = 2.4f;

    [Header("Position")]
    [SerializeField] private float spawnX = 9f;
    [SerializeField] private float[] coinHeights = { -2.2f, -0.8f };
    [SerializeField] private float[] airHeights = { -0.5f, -2.5f };

    [Header("Chances (relative weights)")]
    [SerializeField] private float groundWeight = 55f;
    [SerializeField] private float airWeight = 20f;
    [SerializeField] private float coinWeight = 25f;

    [SerializeField] private GameObject shieldPickup;
    [SerializeField] private float shieldWeight = 6f;
    private PlayerHealth playerHealth;

    private float timer;

    private void Start()
    {
        playerHealth = FindFirstObjectByType<PlayerHealth>();
        timer = 1.5f;   // short grace period at the start
    }

    private void Update()
    {
        if (GameSpeed.Current <= 0f) return;   // game over: stop spawning

        timer -= Time.deltaTime;
        if (timer > 0f) return;

        SpawnRandom();
        // Faster world, shorter wait, so the distance between obstacles stays similar.
        float speedFactor = Mathf.Max(6f / GameSpeed.Current, 0.7f);
        timer = Random.Range(minInterval, maxInterval) * speedFactor;
    }

    private void SpawnRandom()
    {
        // No shield pickups while one is already active.
        float shieldChance = (shieldPickup != null && playerHealth != null && !playerHealth.HasShield)
            ? shieldWeight : 0f;

        float roll = Random.value * (groundWeight + airWeight + coinWeight + shieldChance);

        if (roll < groundWeight)
            Spawn(RandomOf(groundObstacles), null);
        else if (roll < groundWeight + airWeight)
            Spawn(RandomOf(airObstacles), null);
        else if (roll < groundWeight + airWeight + coinWeight)
            Spawn(coin, coinHeights[Random.Range(0, coinHeights.Length)]);
        else
            Spawn(shieldPickup, coinHeights[Random.Range(0, coinHeights.Length)]);
    }

    private GameObject RandomOf(GameObject[] list)
    {
        return list[Random.Range(0, list.Length)];
    }

    private void Spawn(GameObject prefab, float? y)
    {
        if (prefab == null) return;
        float spawnY = y ?? prefab.transform.position.y;
        Instantiate(prefab, new Vector3(spawnX, spawnY, 0f), Quaternion.identity);
    }
}