using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHurt : LivingEntity
{
    private static readonly int HashDeath = Animator.StringToHash("Death");
    [SerializeField] private float hurtFlashDuration = 0.15f;

    public Slider hpSlider;
    private Animator playerAnimator;
    private GameManager gameManager;
    private UIManager uiManager;
    private PlayerInput playerInput;

    [SerializeField] private AudioClip playerDeathClip;
    [SerializeField] private AudioClip playerHurtClip;
    [SerializeField] private Image hurtOverlay;
    private AudioSource playerAudioSource;
    private Shoot shoot;
    private Coroutine hurtFlashCoroutine;

    private void Awake()
    {
        playerAnimator = GetComponent<Animator>();
        playerAnimator ??= GetComponentInChildren<Animator>();

        gameManager = FindFirstObjectByType<GameManager>();
        uiManager = FindFirstObjectByType<UIManager>();
        playerInput = GetComponent<PlayerInput>();
        shoot = GetComponent<Shoot>();
        playerAudioSource = GetComponent<AudioSource>();
        if (playerAudioSource == null)
            playerAudioSource = gameObject.AddComponent<AudioSource>();

        if (playerAnimator == null)
            Debug.LogError("Player Animator not found. Death trigger cannot be played.", this);

        if (hurtOverlay != null)
        {
            var color = hurtOverlay.color;
            color.a = 0f;
            hurtOverlay.color = color;
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        UpdateHPSlider();
    }

    private void Update()
    {
        if (!IsDead)
            return;

        if (Input.GetKeyDown(KeyCode.R))
            ReloadLevel();
    }

    private void UpdateHPSlider()
    {
        if (hpSlider == null)
        {
            Debug.LogWarning("HP Slider not assigned on Player.", this);
            return;
        }

        hpSlider.maxValue = startingHealth;
        hpSlider.value = Health;
    }

    public override void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (IsDead)
            return;

        if (playerAudioSource != null && playerHurtClip != null)
        {
            float effectVolume = AudioSetting.Current != null ? AudioSetting.Current.EffectVolume : 1f;
            playerAudioSource.PlayOneShot(playerHurtClip, effectVolume);
        }

        if (hurtOverlay != null)
        {
            if (hurtFlashCoroutine != null)
                StopCoroutine(hurtFlashCoroutine);

            hurtFlashCoroutine = StartCoroutine(PlayHurtFlash());
        }

        base.OnDamage(damage, hitPoint, hitNormal);

        UpdateHPSlider();
    }

    public override void Die()
    {
        if (IsDead)
            return;

        PrepareDeathAnimation();
        DisablePlayerControls();

        base.Die();

        ShowGameOverUI();
    }

    private void PrepareDeathAnimation()
    {
        if (playerAnimator == null)
            return;

        if (playerAudioSource != null && playerDeathClip != null)
        {
            float effectVolume = AudioSetting.Current != null ? AudioSetting.Current.EffectVolume : 1f;
            playerAudioSource.PlayOneShot(playerDeathClip, effectVolume);
        }

        playerAnimator.enabled = true;
        playerAnimator.Rebind();
        playerAnimator.Update(0f);
        playerAnimator.SetTrigger(HashDeath);
        StartCoroutine(EnsureDeathAnimation());
    }

    private void DisablePlayerControls()
    {
        if (playerInput != null)
            playerInput.enabled = false;

        if (shoot != null)
            shoot.enabled = false;
    }

    private void ShowGameOverUI()
    {
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();

        if (gameManager != null)
            gameManager.EndGame();
        else
            Debug.LogError("GameManager not found. Cannot trigger game over.", this);
    }

    public void RestartLevel()
    {
        ShowGameOverUI();
    }

    private void ReloadLevel()
    {
        if (uiManager == null)
            uiManager = FindFirstObjectByType<UIManager>();

        if (uiManager != null)
            uiManager.SetActiveGameOverUI(false);

        UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        UnityEngine.SceneManagement.SceneManager.LoadScene(scene.buildIndex);
    }

    private IEnumerator EnsureDeathAnimation()
    {
        yield return null;

        if (playerAnimator == null)
            yield break;

        AnimatorStateInfo stateInfo = playerAnimator.GetCurrentAnimatorStateInfo(0);
        if (!stateInfo.IsName("Death"))
            playerAnimator.Play("Death", 0, 0f);
    }

    private IEnumerator PlayHurtFlash()
    {
        Color color = hurtOverlay.color;
        color.a = 0.6f;
        hurtOverlay.color = color;

        float timer = 0f;
        while (timer < hurtFlashDuration)
        {
            timer += Time.deltaTime;
            color.a = Mathf.Lerp(0.05f, 0f, timer / hurtFlashDuration);
            hurtOverlay.color = color;
            yield return null;
        }

        color.a = 0f;
        hurtOverlay.color = color;
        hurtFlashCoroutine = null;
    }
}
