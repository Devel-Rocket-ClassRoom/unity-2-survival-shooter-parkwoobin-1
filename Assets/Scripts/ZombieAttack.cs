using UnityEngine;

// ZombieAttack는 좀비가 플레이어에게 공격을 가하는 기능을 담당하는 클래스입니다. 좀비가 플레이어와 충돌할 때 일정 간격으로 피해를 입히도록 구현되어 있습니다.
public class ZombieAttack : MonoBehaviour
{
    [SerializeField] private float attackInterval = 1f;

    private ZombieBase zombieBase;
    private float attackDamage;
    private float lastAttackTime;

    private void Awake()
    {
        zombieBase = GetComponent<ZombieBase>();
        if (zombieBase == null)
            Debug.LogError($"ZombieBase not found on {gameObject.name}.", this);
    }

    public void SetAttackDamage(float damage)
    {
        attackDamage = damage;
    }

    private void OnTriggerEnter(Collider other) => TryAttack(other);
    private void OnTriggerStay(Collider other) => TryAttack(other);
    private void OnCollisionEnter(Collision collision) => TryAttack(collision.collider);
    private void OnCollisionStay(Collision collision) => TryAttack(collision.collider);

    private void TryAttack(Collider other)
    {
        if (zombieBase == null || zombieBase.IsDead || Time.time <= lastAttackTime + attackInterval)
            return;

        PlayerHurt playerHurt = other.GetComponentInParent<PlayerHurt>();
        if (playerHurt == null || playerHurt.IsDead)
            return;

        lastAttackTime = Time.time;
        Debug.Log($"[Attack] {gameObject.name} → {playerHurt.name} : {attackDamage} damage", this);
        playerHurt.OnDamage(attackDamage, transform.position, -transform.forward);
    }
}
