using UnityEngine;
using UnityEngine.UI;

public class PressESC : MonoBehaviour
{
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private AudioSetting audioSetting;
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private Slider BGMSlider;
    [SerializeField] private Slider EffectSlider;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button quitButton;

    private bool isMenuOpen;
    private float bgmBaseVolume = 1f;
    private bool hasCapturedBgmBaseVolume;

    public bool IsMenuOpen => isMenuOpen;

    private void Awake()
    {
        ResolveMenuPanel();
        ResolveSliders();
        ResolveBgmSource();

        if (audioSetting == null)
            Debug.LogError("AudioSetting is not assigned on PressESC.", this);
        else
            audioSetting.RegisterAsCurrent();

        if (BGMSlider != null)
        {
            BGMSlider.value = audioSetting != null ? audioSetting.BgmVolume : 1f;
            BGMSlider.onValueChanged.AddListener(SetBgmVolume);
        }

        if (EffectSlider != null)
        {
            EffectSlider.value = audioSetting != null ? audioSetting.EffectVolume : 1f;
            EffectSlider.onValueChanged.AddListener(SetEffectVolume);
        }

        if (continueButton != null)
            continueButton.onClick.AddListener(HideMenu);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);

        ApplyBgmVolume();

        SetMenuState(false);
    }

    private void OnDestroy()
    {
        if (BGMSlider != null)
            BGMSlider.onValueChanged.RemoveListener(SetBgmVolume);

        if (EffectSlider != null)
            EffectSlider.onValueChanged.RemoveListener(SetEffectVolume);

        if (continueButton != null)
            continueButton.onClick.RemoveListener(HideMenu);

        if (quitButton != null)
            quitButton.onClick.RemoveListener(QuitGame);
    }

    public void ShowMenu()
    {
        SetMenuState(true);
    }

    public void HideMenu()
    {
        SetMenuState(false);
    }

    public void ToggleMenu()
    {
        if (menuPanel == null)
            ResolveMenuPanel();

        if (menuPanel != null)
            isMenuOpen = menuPanel.activeSelf;

        SetMenuState(!isMenuOpen);
    }

    private void SetMenuState(bool open)
    {
        if (menuPanel == null)
            ResolveMenuPanel();

        ResolveSliders();

        isMenuOpen = open;

        if (menuPanel != null)
            menuPanel.SetActive(open);
        else
            Debug.LogWarning("Pause menu panel not found. Assign menuPanel on PressESC.", this);

        Time.timeScale = open ? 0f : 1f;

        if (BGMSlider != null)
            BGMSlider.value = audioSetting != null ? audioSetting.BgmVolume : 1f;

        if (EffectSlider != null)
            EffectSlider.value = audioSetting != null ? audioSetting.EffectVolume : 1f;
    }

    private void SetBgmVolume(float volume)
    {
        if (audioSetting == null)
            return;

        audioSetting.SetBgmVolume(volume);
        ApplyBgmVolume();
    }

    private void SetEffectVolume(float volume)
    {
        if (audioSetting == null)
            return;

        audioSetting.SetEffectVolume(volume);
    }

    private void QuitGame()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void ResolveMenuPanel()
    {
        if (menuPanel != null)
            return;

        menuPanel = FindInactiveInScene("pressESC");
        menuPanel ??= FindInactiveInScene("PauseMenu");
        menuPanel ??= FindInactiveInScene("MenuPanel");
        menuPanel ??= FindInactiveInScene("EndUI");
        menuPanel ??= GameObject.Find("pressESC");
        menuPanel ??= GameObject.Find("PauseMenu");
        menuPanel ??= GameObject.Find("MenuPanel");
        menuPanel ??= GameObject.Find("EndUI");
    }

    private GameObject FindInactiveInScene(string objectName)
    {
        Transform[] allTransforms = Resources.FindObjectsOfTypeAll<Transform>();
        foreach (Transform t in allTransforms)
        {
            if (t == null || t.gameObject == null)
                continue;

            if (!t.gameObject.scene.IsValid())
                continue;

            if (t.name == objectName)
                return t.gameObject;
        }

        return null;
    }

    private void ResolveSliders()
    {
        if (BGMSlider == null)
            BGMSlider = FindSliderInScene("BGM", "BGMSlider");

        if (EffectSlider == null)
            EffectSlider = FindSliderInScene("Effect", "EffectSlider");
    }

    private void ResolveBgmSource()
    {
        if (bgmSource == null)
            bgmSource = FindBgmSourceInScene();

        if (bgmSource != null && !hasCapturedBgmBaseVolume)
        {
            bgmBaseVolume = bgmSource.volume;
            hasCapturedBgmBaseVolume = true;
        }
    }

    private void ApplyBgmVolume()
    {
        ResolveBgmSource();

        if (bgmSource == null || audioSetting == null)
            return;

        bgmSource.volume = bgmBaseVolume * audioSetting.BgmVolume;
    }

    private AudioSource FindBgmSourceInScene()
    {
        AudioSource[] audioSources = Resources.FindObjectsOfTypeAll<AudioSource>();
        foreach (AudioSource source in audioSources)
        {
            if (source == null || source.gameObject == null)
                continue;

            if (!source.gameObject.scene.IsValid())
                continue;

            if (source.clip != null && audioSetting != null && source.clip == audioSetting.BgmClip)
                return source;

            if (source.gameObject.name == "GameManager" || source.gameObject.name == "BGM")
                return source;
        }

        return null;
    }

    private Slider FindSliderInScene(params string[] objectNames)
    {
        Transform[] allTransforms = Resources.FindObjectsOfTypeAll<Transform>();
        foreach (Transform t in allTransforms)
        {
            if (t == null || t.gameObject == null)
                continue;

            if (!t.gameObject.scene.IsValid())
                continue;

            foreach (string objectName in objectNames)
            {
                if (t.name != objectName)
                    continue;

                Slider slider = t.GetComponent<Slider>();
                if (slider != null)
                    return slider;
            }
        }

        return null;
    }
}
