using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    public float AttackSpeed;
    public object Owner;
    public GameObject ParticleEffectPrefab;

    public abstract void Attack ( IDamagable thingToDamage, Vector3 positionForParticles, GameObject owner );
}
