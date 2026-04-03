using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

// PressESC 클래스는 ESC 키 입력을 감지하여 일시정지 메뉴를 토글하는 기능을 담당합니다. 메뉴 패널, 오디오 설정, 슬라이더, 버튼 등을 관리하며, 게임이 일시정지 상태일 때 시간 스케일을 조절하여 게임을 멈추거나 재개하는 역할도 합니다.
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
        EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }

    private void ResolveMenuPanel() // 메뉴 패널을 찾는 메서드입니다. 여러 이름으로 검색하여 유연성을 높였습니다.
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

    private void ResolveSliders()   // 슬라이더를 찾는 메서드입니다. 여러 이름으로 검색하여 유연성을 높였습니다.
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

    private void ApplyBgmVolume()   // BGM 볼륨을 적용하는 메서드입니다. AudioSetting에서 현재 볼륨을 가져와서 bgmSource에 적용합니다.
    {
        ResolveBgmSource();

        if (bgmSource == null || audioSetting == null)
            return;

        bgmSource.volume = bgmBaseVolume * audioSetting.BgmVolume;
    }

    private AudioSource FindBgmSourceInScene()  // BGM 오디오 소스를 찾는 메서드입니다. 여러 기준으로 검색하여 유연성을 높였습니다.
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
