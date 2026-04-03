using UnityEngine;

// GameManager는 게임의 전반적인 상태를 관리하는 클래스입니다. 점수 관리, 게임 오버 처리, UI 업데이트 등의 기능을 담당합니다. 좀비가 사망할 때 점수를 추가하고, 플레이어가 사망하면 게임 오버 UI를 활성화하는 등의 역할을 수행합니다.
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
