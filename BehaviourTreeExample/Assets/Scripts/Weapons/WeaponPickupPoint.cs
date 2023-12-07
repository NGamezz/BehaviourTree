using UnityEngine;

[RequireComponent(typeof(Weapon))]
public class WeaponPickupPoint : MonoBehaviour
{
    private Weapon availableAiWeapon;

    private void Start()
    {
        SetWeapon();
    }

    public void SetWeapon()
    {
        availableAiWeapon = GetComponent<Weapon>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IWeaponOwner weaponOwner))
        {
            if (weaponOwner is MonoBehaviour owner)
            {
                Weapon weapon = (Weapon)owner.gameObject.AddComponent(availableAiWeapon.GetType());
                weapon.Owner = weaponOwner;
                weapon.AttackSpeed = availableAiWeapon.AttackSpeed;
                weapon.ParticleEffectPrefab = availableAiWeapon.ParticleEffectPrefab;
                weaponOwner.AcquireWeapon(weapon);
            }
        }
    }
}