using System;
using UnityEngine;
using UnityEngine.Events;

// LivingEntity는 게임에서 생명체의 기본적인 특성과 행동을 정의하는 클래스입니다. 플레이어와 좀비 모두 이 클래스를 상속하여 체력, 피해 처리, 치유, 사망 등의 기능을 구현합니다. OnDamage, OnHeal, Die 메서드를 통해 각각 피해를 입히고, 치유하며, 사망 처리를 할 수 있습니다. 또한, OnDead 이벤트를 통해 사망 시 추가적인 행동을 연결할 수 있습니다.
public class LivingEntity : MonoBehaviour
{
    public float startingHealth = 100f; // 초기 플레이어 체력
    public float Health { get; protected set; }
    public bool IsDead { get; protected set; }

    public UnityEvent OnDead;

    protected virtual void OnEnable()
    {
        IsDead = false;
        Health = startingHealth;
    }


    public virtual void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        Health -= damage;
        if (Health <= 0)
        {
            Health = 0;
            Die();
        }
    }

    public virtual void OnHeal(float add)
    {
        if (IsDead) return;

        Health += add;
        if (Health > startingHealth)
        {
            Health = startingHealth;
        }
    }

    public virtual void Die()
    {
        OnDead?.Invoke();
        IsDead = true;
    }

}
