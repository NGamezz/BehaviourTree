using System.Diagnostics;

public class AssaultRifle : IWeapon
{
    IWeaponOwner owner;

    public void Pickup(IWeaponOwner owner)
    {
        this.owner = owner;
    }

    public void Attack()
    {
        UnityEngine.Debug.Log("Attack");
    }
}
