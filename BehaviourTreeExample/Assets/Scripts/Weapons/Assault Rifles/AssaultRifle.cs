using System.Collections;
using UnityEngine;

//Not ideal, but should work for now.
public class AssaultRifle : Weapon
{
    private bool canAttack = true;
    private ParticleSystem particleEffect;

    private void Start()
    {
        particleEffect = Instantiate(ParticleEffectPrefab).GetComponent<ParticleSystem>();
    }

    public override void Attack(object thingToDamage)
    {
        if (canAttack)
        {
            StartCoroutine(StartAttack(thingToDamage));
        }
    }

    private IEnumerator StartAttack(object thingToDamage = null)
    {
        if (thingToDamage is not MonoBehaviour thingToDamageMono) { yield break; }
        if (Owner is not MonoBehaviour ownerMono) { yield break; }

        Debug.Log("Attack");
        canAttack = false;

        particleEffect.transform.position = thingToDamageMono.transform.position;
        particleEffect.Play();

        thingToDamageMono.GetComponent<IDamagable>().TakeDamage(ownerMono.gameObject, 10);

        yield return new WaitForSeconds(AttackSpeed);

        canAttack = true;
    }
}