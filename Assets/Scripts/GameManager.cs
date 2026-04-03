using UnityEngine;

public class GameManager : MonoBehaviour
{
    public UIManager uiManager;
    public ZombieSpawner spawner;
    private int score = 0;
    public bool IsGameOver { get; private set; }

    private void Awake()
    {
        uiManager = FindFirstObjectByType<UIManager>();
    }

    public void Start()
    {
        uiManager.SetScoreText(score);
    }

    public void AddScore(int add)
    {
        if (IsGameOver) return;

        score += add;
        uiManager.SetScoreText(score);

    }

    public void EndGame()
    {
        if (IsGameOver)
            return;

        IsGameOver = true;
        spawner.enabled = false;    // 종료시 좀비 스포너 비활성화
        uiManager.SetActiveGameOverUI(true);
    }
}
