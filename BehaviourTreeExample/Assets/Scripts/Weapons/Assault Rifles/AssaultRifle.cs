using UnityEngine;

//Not ideal, but should work for now.
public class AssaultRifle : Weapon
{
    private ParticleSystem particleEffect;

    private void Start ()
    {
        particleEffect = Instantiate(ParticleEffectPrefab).GetComponent<ParticleSystem>();
    }

    public override void Attack ( IDamagable thingToDamage, Vector3 positionForParticles, GameObject owner )
    {
        Debug.Log("Attack");

        particleEffect.transform.position = positionForParticles;
        particleEffect.Play();
        thingToDamage.TakeDamage(owner, 25);
    }
}