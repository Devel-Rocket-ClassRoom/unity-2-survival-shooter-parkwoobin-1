using System;
using UnityEngine;
using UnityEngine.Events;

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
