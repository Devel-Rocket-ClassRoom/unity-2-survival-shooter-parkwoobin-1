using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ZombieSpawner : MonoBehaviour
{
    public GameManager gameManager;
    public UIManager uiManager;
    public ZombieBase[] Prefab;
    public ZombieData[] zombieDatas;
    public Transform[] spawnPoints;

    private List<ZombieBase> zombies = new List<ZombieBase>();

    private int wave;

    private void Awake()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        uiManager = FindFirstObjectByType<UIManager>();
    }

    private void Update()
    {
        if (zombies.Count == 0)
        {
            SpawnWave();
        }
    }

    private void SpawnWave()
    {
        wave++;

        int count = Mathf.RoundToInt(wave * 1.5f);
        for (int i = 0; i < count; i++)
        {
            CreateZombie();
        }

        if (uiManager != null)
            uiManager.SetWaveInfo(wave);

    }

    private void CreateZombie()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("ZombieSpawner: spawnPoints not assigned!");
            return;
        }

        if (Prefab == null || Prefab.Length == 0)
        {
            Debug.LogError("ZombieSpawner: Prefab not assigned!");
            return;
        }

        if (zombieDatas == null || zombieDatas.Length == 0)
        {
            Debug.LogError("ZombieSpawner: zombieDatas not assigned!");
            return;
        }

        if (Prefab.Length != zombieDatas.Length)
        {
            Debug.LogWarning($"ZombieSpawner: Prefab({Prefab.Length}) and zombieDatas({zombieDatas.Length}) length mismatch. Matching by index where possible.");
        }

        var point = spawnPoints[Random.Range(0, spawnPoints.Length)];
        int zombieTypeIndex = Random.Range(0, Prefab.Length);
        var zombie = Instantiate(Prefab[zombieTypeIndex], point.position, point.rotation);

        if (zombieTypeIndex < zombieDatas.Length && zombieDatas[zombieTypeIndex] != null)
        {
            zombie.Setup(zombieDatas[zombieTypeIndex]);
        }
        else
        {
            Debug.LogWarning($"ZombieSpawner: Missing ZombieData for prefab index {zombieTypeIndex}. Using prefab default data.");
        }
        zombies.Add(zombie);

        zombie.gameObject.SetActive(true);

        zombie.OnDead.AddListener(() => zombies.Remove(zombie));

        if (gameManager != null)
            zombie.OnDead.AddListener(() => gameManager.AddScore(100));

        if (uiManager != null)
            zombie.OnDead.AddListener(() => uiManager.SetWaveInfo(wave));

        zombie.OnDead.AddListener(() => Destroy(zombie.gameObject, 5f));
    }
}
