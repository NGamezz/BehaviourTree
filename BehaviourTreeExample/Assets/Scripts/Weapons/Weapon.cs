using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    public float AttackSpeed;
    public object Owner;
    public GameObject ParticleEffectPrefab;

    public abstract void Attack(object thingToDamage);
}
