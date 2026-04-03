using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public Text scoreText;
    public Text waveText;

    public GameObject gameOverUI;
    [SerializeField] private PressESC pressESC;



    private void Awake()
    {
        if (gameOverUI == null)
            gameOverUI = FindInactiveChildByName("EndUI");

        if (pressESC == null)
            pressESC = FindInactiveComponentInScene<PressESC>();
    }

    public void OnEnable()
    {
        SetScoreText(0);
        SetWaveInfo(0);
        SetActiveGameOverUI(false);

        if (pressESC != null)
            pressESC.HideMenu();

        Time.timeScale = 1f;

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!TryTogglePauseMenu())
                StartCoroutine(TogglePauseMenuNextFrame());
        }


    }

    private bool TryTogglePauseMenu()
    {
        if (pressESC == null)
            pressESC = FindInactiveComponentInScene<PressESC>();

        if (pressESC == null)
            return false;

        pressESC.ToggleMenu();
        return true;
    }

    private IEnumerator TogglePauseMenuNextFrame()
    {
        yield return null;
        TryTogglePauseMenu();
    }

    public void SetScoreText(int score)
    {
        scoreText.text = $"Score: {score}";
    }
    public void SetWaveInfo(int wave)
    {
        waveText.text = $"Wave: {wave}";
    }

    public void SetActiveGameOverUI(bool active)
    {
        if (gameOverUI == null)
            gameOverUI = FindInactiveChildByName("EndUI");

        if (gameOverUI == null)
        {
            Debug.LogError("EndUI not found. Assign the EndUI root object to UIManager.", this);
            return;
        }

        gameOverUI.SetActive(active);
    }

    private GameObject FindInactiveChildByName(string objectName)
    {
        Transform[] transforms = GetComponentsInChildren<Transform>(true);
        foreach (Transform child in transforms)
        {
            if (child.name == objectName)
                return child.gameObject;
        }

        return null;
    }

    private T FindInactiveComponentInScene<T>() where T : Component
    {
        T[] all = Resources.FindObjectsOfTypeAll<T>();
        foreach (T item in all)
        {
            if (item == null || item.gameObject == null)
                continue;

            if (!item.gameObject.scene.IsValid())
                continue;

            return item;
        }

        return null;
    }

    public void OnclickRestart()
    {
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.buildIndex);
    }
}
