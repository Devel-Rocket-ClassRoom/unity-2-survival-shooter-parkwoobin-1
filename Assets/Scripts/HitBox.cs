using System.Collections.Generic;
using UnityEngine;

public class HitBox : MonoBehaviour
{
    private List<Collider> colliders = new List<Collider>();   // 이미 데미지를 입은 타겟 목록
    public List<Collider> Colliders
    {
        get { return colliders; }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!Colliders.Contains(other))
        {
            Colliders.Add(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        colliders.Remove(other);
    }
}
