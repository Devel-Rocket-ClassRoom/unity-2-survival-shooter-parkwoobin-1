using UnityEditor.PackageManager;
using UnityEngine;

[CreateAssetMenu(fileName = "ZombieData", menuName = "Scriptable Objects/ZombieData")]
public class ZombieData : ScriptableObject
{
    public float maxHP;
    public float damage;
    public float Speed;
}
