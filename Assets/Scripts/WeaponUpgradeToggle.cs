using UnityEngine;

public class WeaponUpgradeToggle : MonoBehaviour
{
    [SerializeField] private GameObject weaponUpgradePanel; // Kéo panel nâng cấp vào đây

    void Update()
    {
        // Chỉ phản hồi khi game đang chạy bình thường (không tạm dừng, không UI khác)
        if (Input.GetKeyDown(KeyCode.Tab) && Time.timeScale == 1f)
        {
            if (weaponUpgradePanel != null)
            {
                bool isActive = weaponUpgradePanel.activeSelf;
                weaponUpgradePanel.SetActive(!isActive);
            }
        }
    }
}