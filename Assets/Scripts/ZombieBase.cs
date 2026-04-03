using UnityEngine;
using UnityEngine.AI;


// ZombieBase는 좀비의 기본적인 행동과 상태를 관리하는 클래스입니다. 좀비의 체력, 이동, 사망 처리 등을 담당하며, ZombieAttack과 상호작용하여 플레이어에게 피해를 입히는 역할도 합니다.
[RequireComponent(typeof(AudioSource))]
public class ZombieBase : LivingEntity
{
    public enum Status { Idle, Move, Death }

    [SerializeField] private ZombieData zombieData;
    [SerializeField] private ParticleSystem zombieHitParticle;
    [SerializeField] private AudioClip zombieDamageClip;
    [SerializeField] private AudioClip zombieDeathClip;

    private Animator zombieAnimator;
    private AudioSource zombieAudioSource;
    private Collider zombieCollider;
    private NavMeshAgent agent;
    private Transform playerTarget;
    private Status currentStatus;
    private float deathStartTime;
    private const float DeathSinkSpeed = 1f;
    private const float DeathDelay = 0.5f;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        zombieAnimator = GetComponent<Animator>();
        zombieAudioSource = GetComponent<AudioSource>();
        if (zombieAudioSource == null)
            zombieAudioSource = gameObject.AddComponent<AudioSource>();
        zombieCollider = GetComponent<Collider>();

        if (agent == null)
        {
            Debug.LogError($"NavMeshAgent not found on {gameObject.name}.", this);
            enabled = false;
            return;
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        if (zombieHitParticle != null && !zombieHitParticle.gameObject.scene.IsValid())
        {
            zombieHitParticle = Instantiate(zombieHitParticle, transform.position, Quaternion.identity, transform);
            zombieHitParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        ApplyZombieData();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        if (agent == null)
            return;

        agent.enabled = true;

        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 10f, NavMesh.AllAreas))
            agent.Warp(hit.position);

        if (agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.ResetPath();
        }

        if (zombieCollider != null)
            zombieCollider.enabled = true;
        if (zombieHitParticle != null)
            zombieHitParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        currentStatus = Status.Idle;
        deathStartTime = 0f;
        playerTarget = FindPlayerTarget();
    }

    private void Update()
    {
        if (currentStatus == Status.Idle)
            UpdateIdle();
        else if (currentStatus == Status.Move)
            UpdateMove();
        else if (currentStatus == Status.Death)
            UpdateDie();
    }

    public void Setup(ZombieData data)
    {
        zombieData = data;
        ApplyZombieData();
    }

    private void ApplyZombieData()
    {
        if (zombieData == null)
        {
            Debug.LogError($"ZombieData is not assigned on {gameObject.name}.", this);
            return;
        }

        startingHealth = zombieData.maxHP;
        Health = startingHealth;

        if (agent != null)
            agent.speed = zombieData.Speed;

        var zombieAttack = GetComponent<ZombieAttack>();
        if (zombieAttack != null)
            zombieAttack.SetAttackDamage(zombieData.damage);
    }

    private bool CanUseAgent()
    {
        return agent != null && agent.enabled && agent.isOnNavMesh;
    }

    private void SetStatus(Status status)
    {
        if (currentStatus == status)
            return;

        currentStatus = status;

        if (zombieAnimator != null)
            zombieAnimator.SetBool("HasTarget", status == Status.Move);

        if (status == Status.Death)
        {
            if (zombieCollider != null)
                zombieCollider.enabled = false;

            if (agent != null)
            {
                agent.isStopped = true;
                agent.enabled = false;
            }

            PlayZombieSound(zombieDeathClip);

            if (zombieAnimator != null)
                zombieAnimator.SetTrigger("Death");

            deathStartTime = Time.time;
            return;
        }

        if (CanUseAgent())
            agent.isStopped = status == Status.Idle;
    }

    private Transform FindPlayerTarget()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            return playerObj.transform;

        PlayerMovement playerMovement = FindFirstObjectByType<PlayerMovement>();
        return playerMovement != null ? playerMovement.transform : null;
    }

    private bool IsTargetValid(Transform candidate)
    {
        if (candidate == null)
            return false;

        var living = candidate.GetComponent<LivingEntity>();
        return living == null || !living.IsDead;
    }

    private void UpdateIdle()
    {
        if (!IsTargetValid(playerTarget))
            playerTarget = FindPlayerTarget();

        if (IsTargetValid(playerTarget))
            SetStatus(Status.Move);
    }

    private void UpdateMove()
    {
        if (!IsTargetValid(playerTarget))
            playerTarget = FindPlayerTarget();

        if (!IsTargetValid(playerTarget))
        {
            SetStatus(Status.Idle);
            return;
        }

        if (CanUseAgent())
            agent.SetDestination(playerTarget.position);
    }

    private void UpdateDie()
    {
        if (Time.time < deathStartTime + DeathDelay)
            return;

        transform.Translate(Vector3.down * DeathSinkSpeed * Time.deltaTime, Space.World);

        if (Time.time >= deathStartTime + DeathDelay + 1f)
            Destroy(gameObject);
    }

    public void StartSinking()
    {
        deathStartTime = Time.time - DeathDelay;
    }

    public override void Die()
    {
        if (IsDead)
            return;

        base.Die();
        SetStatus(Status.Death);
    }

    public override void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (IsDead)
            return;

        if (zombieHitParticle != null)
        {
            zombieHitParticle.transform.position = hitPoint;
            zombieHitParticle.transform.rotation = Quaternion.LookRotation(hitNormal);
            zombieHitParticle.Play();
        }

        PlayZombieSound(zombieDamageClip);

        base.OnDamage(damage, hitPoint, hitNormal);

        if (IsDead)
            SetStatus(Status.Death);
    }

    private void PlayZombieSound(AudioClip clip)
    {
        if (clip == null)
            return;

        if (zombieAudioSource == null)
            zombieAudioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();

        if (zombieAudioSource != null)
        {
            float effectVolume = AudioSetting.Current != null ? AudioSetting.Current.EffectVolume : 1f;
            zombieAudioSource.PlayOneShot(clip, effectVolume);
        }
        else
        {
            float effectVolume = AudioSetting.Current != null ? AudioSetting.Current.EffectVolume : 1f;
            AudioSource.PlayClipAtPoint(clip, transform.position, effectVolume);
        }
    }
}