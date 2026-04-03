using UnityEngine;

// Shoot 클래스는 플레이어가 총을 발사하는 기능을 담당합니다. 마우스 클릭으로 총을 발사하며, 레이캐스트를 사용하여 적에게 피해를 입히고, 총구에서 선을 표시하는 효과를 구현합니다. 또한, 총소리를 재생하고, 총알 이펙트를 발생시키는 기능도 포함되어 있습니다.
public class Shoot : MonoBehaviour
{
    [SerializeField] private GunData gunData;
    [SerializeField] private ParticleSystem gunParticles;
    [SerializeField] private LineRenderer shotLineRenderer;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireDistance = 30f;
    [SerializeField] private float lineToggleInterval = 0.06f;
    [SerializeField] private LayerMask hitMask = ~0;



    private bool lineVisible;
    private float nextToggleTime;
    private Vector3 currentLineEnd;
    private AudioSource gunAudioSource;

    private void Awake()
    {
        firePoint ??= transform;    // firePoint이 지정되지 않은 경우 현재 오브젝트의 위치와 회전을 사용
        gunAudioSource = GetComponent<AudioSource>();
        if (gunAudioSource == null)
            gunAudioSource = gameObject.AddComponent<AudioSource>();

        gunParticles = GetComponentInChildren<ParticleSystem>(true);
        shotLineRenderer = GetComponentInChildren<LineRenderer>(true);


        if (gunParticles != null && !gunParticles.gameObject.scene.IsValid())
        {
            gunParticles = Instantiate(gunParticles, firePoint.position, firePoint.rotation, firePoint);
        }

        if (shotLineRenderer == null)
        {
            Debug.LogError("LineRenderer를 찾지 못했습니다. Shoot 오브젝트 하위에 LineRenderer를 두거나 인스펙터에 지정하세요.", this);
            enabled = false;
            return;
        }

        shotLineRenderer.positionCount = 2;
        shotLineRenderer.useWorldSpace = true;
        shotLineRenderer.enabled = false;
        lineVisible = false;
        currentLineEnd = firePoint.position + firePoint.forward * fireDistance;

        if (gunParticles != null)
        {
            gunParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }


    }

    private void Update()
    {
        if (Time.timeScale == 0f)
        {
            if (lineVisible)
                EndFire();

            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            BeginFire();
        }

        if (Input.GetMouseButton(0))
        {
            TickFire();
        }

        if (Input.GetMouseButtonUp(0))
        {
            EndFire();
        }
    }

    private void BeginFire()
    {
        ShowLine();
        lineVisible = true;
        nextToggleTime = Time.time + lineToggleInterval;
    }

    private void TickFire()
    {
        if (lineVisible)
        {
            UpdateLinePositions();
        }

        if (Time.time < nextToggleTime)
        {
            return;
        }


        ToggleLine();
        nextToggleTime = Time.time + lineToggleInterval;
    }

    private void EndFire()  // 사격이 끝나면 선을 숨김
    {
        HideLine();
        lineVisible = false;
    }

    private void ToggleLine()   // 라인렌더러 토글
    {
        if (lineVisible)
        {
            HideLine();
            lineVisible = false;
        }
        else
        {
            ShowLine();
            lineVisible = true;
        }
    }

    private void ShowLine() // 총구에서 일정 거리까지 선을 표시
    {
        FireRaycast();
        UpdateLinePositions();
        shotLineRenderer.enabled = true;
        PlayGunSound();

        if (gunParticles != null)
        {
            gunParticles.transform.SetPositionAndRotation(firePoint.position, firePoint.rotation);
            gunParticles.Emit(Mathf.Max(1, 10));    // 방출할 입자 수를 조정하여 효과를 강화하거나 약화시킬 수 있습니다.
            gunParticles.Play();
        }
    }

    private void HideLine() // 선을 숨김
    {
        if (shotLineRenderer != null)
        {
            shotLineRenderer.enabled = false;
        }

        if (gunParticles != null)
        {
            gunParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }

    private void UpdateLinePositions()
    {
        Vector3 start = firePoint.position;
        Vector3 end = currentLineEnd;
        shotLineRenderer.SetPosition(0, start);
        shotLineRenderer.SetPosition(1, end);
    }

    private void FireRaycast()
    {
        Vector3 origin = firePoint.position;
        Vector3 direction = firePoint.forward;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, fireDistance, hitMask, QueryTriggerInteraction.Collide))
        {
            currentLineEnd = hit.point;

            // 데미지 처리
            LivingEntity target = hit.collider.GetComponentInParent<LivingEntity>();
            if (target != null)
            {
                // 데미지 적용
                target.OnDamage(gunData.damage, hit.point, -direction);

                Debug.Log($"Hit {hit.collider.name} with damage {gunData.damage}");
            }

            HitBox hitTarget = hit.collider.GetComponentInParent<HitBox>();
            if (hitTarget != null)
            {
                hitTarget.Colliders.Add(hit.collider);
            }
        }
        else
        {
            currentLineEnd = origin + direction * fireDistance;
        }
    }

    private void PlayGunSound() // 총소리 재생
    {
        if (gunData == null || gunData.shotClip == null || gunAudioSource == null)
        {
            return;
        }

        float effectVolume = AudioSetting.Current != null ? AudioSetting.Current.EffectVolume : 1f;
        gunAudioSource.PlayOneShot(gunData.shotClip, effectVolume);
    }
}
