using UnityEngine;

public class WeaponPickupPoint : MonoBehaviour
{
    [SerializeField] private IWeapon availableAiWeapon;

    public void SetWeapon()
    {
        availableAiWeapon = new AssaultRifle();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Guard guard))
        {
            SetWeapon();
            guard.AcquireWeapon(availableAiWeapon);
        }
    }
}
