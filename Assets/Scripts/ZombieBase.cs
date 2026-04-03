using UnityEngine;
using UnityEngine.AI;


// ZombieBase는 좀비의 기본적인 행동과 상태를 관리하는 클래스입니다. 좀비의 체력, 이동, 사망 처리 등을 담당하며, ZombieAttack과 상호작용하여 플레이어에게 피해를 입히는 역할도 합니다.
[RequireComponent(typeof(AudioSource))]
public class ZombieBase : LivingEntity
{
    public enum Status { Idle, Move, Death }    // 좀비의 현재 상태를 나타내는 열거형입니다. Idle은 대기 상태, Move는 이동 상태, Death는 사망 상태를 의미합니다.


    // Attack은 ZombieAttack 컴포넌트에 따로 작성하였습니다.
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

    private void ApplyZombieData()  // 좀비의 데이터를 적용하는 메서드입니다. ZombieData에서 설정된 체력, 이동 속도, 공격 피해 등을 좀비에 적용하여 초기화합니다. 이 메서드는 Awake에서 호출되어 좀비가 생성될 때 데이터를 적용하며, 필요에 따라 외부에서 다시 호출하여 좀비의 특성을 변경할 수도 있습니다.
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

    private void SetStatus(Status status)   // 좀비의 상태를 변경하는 메서드입니다. Idle, Move, Death 세 가지 상태 중 하나로 전환하며, 상태에 따라 애니메이션 트리거, NavMeshAgent의 작동 여부, 콜라이더 활성화 등을 제어합니다. 사망 상태로 전환될 때는 사망 애니메이션을 재생하고, 일정 시간이 지난 후 좀비가 땅으로 가라앉도록 처리합니다.
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

    private void UpdateDie()    // 사망 상태 처리 메소드, 사망 애니메이션 재생 후 좀비가 땅으로 가라 앉아서 방해되지 않게 설정했습니다.
    {
        if (Time.time < deathStartTime + DeathDelay)
            return;

        transform.Translate(Vector3.down * DeathSinkSpeed * Time.deltaTime, Space.World);   // 사망 후 일정 시간이 지나면 좀비가 땅으로 가라앉도록 처리합니다.

        if (Time.time >= deathStartTime + DeathDelay + 1f)
            Destroy(gameObject);
    }

    public void StartSinking()  // 좀비가 땅에 가라 앉는 시간
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
    {// 좀비가 피해를 입었을 때 처리하는 메서드입니다.
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

    private void PlayZombieSound(AudioClip clip)    // 좀비가 피해를 입거나 사망시 나오는 사운드를 관리하는 메서드입니다.
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