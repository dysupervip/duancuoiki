using UnityEngine;

public class WeaponUpgradeToggle : MonoBehaviour
{
    [SerializeField] private GameObject weaponUpgradePanel; // Kéo panel nâng cấp vào đây
    [SerializeField] private WeaponUpgradeUI weaponUpgradeUI;

    void Update()
    {
        // Chỉ phản hồi khi game đang chạy bình thường (không tạm dừng, không UI khác)
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (weaponUpgradePanel != null)
            {
                weaponUpgradeUI?.Toggle();
            }
        }
    }
}