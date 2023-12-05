using UnityEngine;

public class WeaponPickupPoint : MonoBehaviour
{
    [SerializeField] private IWeapon availableAiWeapon;

    private void Start()
    {
        SetWeapon(new AssaultRifle());
    }

    public void SetWeapon(IWeapon weapon)
    {
        availableAiWeapon = weapon;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IWeaponOwner weaponOwner))
        {
            weaponOwner.AcquireWeapon(availableAiWeapon);
        }
    }
}