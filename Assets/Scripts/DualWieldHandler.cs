using UnityEngine;

public class DualWieldHandler : MonoBehaviour
{
    private WeaponBase mainWeapon;

    public void Initialize(WeaponBase main, float duration)
    {
        mainWeapon = main;
        WeaponBase thisWeapon = GetComponent<WeaponBase>();
        if (thisWeapon != null)
        {
            thisWeapon.UpdateStatsFromPlayer();
        }
        Destroy(gameObject, duration); // Tự hủy sau thời gian hiệu lực
    }
}