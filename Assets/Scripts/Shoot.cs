using UnityEngine;

public class Shoot : MonoBehaviour
{
    [SerializeField] private GunData gunData;
    [SerializeField] private ParticleSystem gunParticles;
    [SerializeField] private LineRenderer shotLineRenderer;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireDistance = 30f;
    [SerializeField] private float lineToggleInterval = 0.06f;



    private bool lineVisible;
    private float nextToggleTime;

    private void Awake()
    {
        firePoint ??= transform;

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

        if (gunParticles != null)
        {
            gunParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }


    }

    private void Update()
    {
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
        Vector3 end = start + firePoint.forward * fireDistance;
        shotLineRenderer.SetPosition(0, start);
        shotLineRenderer.SetPosition(1, end);
    }

    private void PlayGunSound()
    {
        if (gunData == null || gunData.shotClip == null)
        {
            Debug.LogWarning("GunData 또는 GunData.shotClip이 비어 있습니다.", this);
            return;
        }

        Vector3 soundPosition = firePoint != null ? firePoint.position : transform.position;
        AudioSource.PlayClipAtPoint(gunData.shotClip, soundPosition);
    }
}
