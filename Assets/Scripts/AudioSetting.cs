using UnityEngine;

[CreateAssetMenu(fileName = "AudioSetting", menuName = "Scriptable Objects/AudioSetting")]
public class AudioSetting : ScriptableObject
{
    public static AudioSetting Current { get; private set; }

    public event System.Action<float> BgmVolumeChanged;
    public event System.Action<float> EffectVolumeChanged;

    [Header("Volumes")]
    [SerializeField, Range(0f, 1f)] private float bgmVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float effectVolume = 1f;

    [Header("BGM")]
    [SerializeField] private AudioClip bgmClip;

    [Header("Effect Sounds")]
    [SerializeField] private AudioClip[] effectClips = new AudioClip[9];

    public float BgmVolume => bgmVolume;
    public float EffectVolume => effectVolume;
    public AudioClip BgmClip => bgmClip;
    public AudioClip[] EffectClips => effectClips;

    public void RegisterAsCurrent()
    {
        Current = this;
    }

    public void SetBgmVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        BgmVolumeChanged?.Invoke(bgmVolume);
    }

    public void SetEffectVolume(float volume)
    {
        effectVolume = Mathf.Clamp01(volume);
        EffectVolumeChanged?.Invoke(effectVolume);
    }

    public AudioClip GetEffectClip(int index)
    {
        if (effectClips == null || index < 0 || index >= effectClips.Length)
            return null;

        return effectClips[index];
    }
}
