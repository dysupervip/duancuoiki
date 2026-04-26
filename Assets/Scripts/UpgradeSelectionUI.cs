using UnityEngine;
using UnityEngine.UI;

public class UpgradeSelectionUI : MonoBehaviour
{
    [System.Serializable]
    public class UpgradeOption
    {
        public string statName;   // Tên chỉ số: "HP", "Speed", "Armor"
        public Button button;     // Nút bấm
        public Text label;        // Nhãn hiển thị
    }

    [SerializeField] private UpgradeOption[] options; // Mảng 3 lựa chọn
    public bool HasChosen { get; private set; }       // Đã chọn xong chưa?

    private Player player;

    void Start()
    {
        player = FindAnyObjectByType<Player>(); // Tìm Player trong scene
    }

    // Hiện bảng lên
    public void Show()
    {
        HasChosen = false;
        gameObject.SetActive(true);       // Bật panel
        Time.timeScale = 0f;              // Dừng game (phòng trường hợp WaveManager chưa dừng)
        foreach (var opt in options)
        {
            opt.button.onClick.RemoveAllListeners(); // Xóa sự kiện cũ
            opt.button.onClick.AddListener(() => OnChoose(opt.statName)); // Gán sự kiện mới
            opt.button.interactable = true; // Cho phép bấm
        }
    }

    // Khi người chơi bấm vào 1 nút
    void OnChoose(string stat)
    {
        if (HasChosen) return;  // Chỉ chọn 1 lần
        HasChosen = true;

        // Áp dụng nâng cấp tương ứng
        switch (stat)
        {
            case "HP": player.UpgradeHP(20f); break;
            case "Speed": player.UpgradeMoveSpeed(1f); break;
            case "Armor": player.UpgradeArmor(0.15f); break;
        }

        // Vô hiệu hóa tất cả nút để không chọn thêm
        foreach (var opt in options)
            opt.button.interactable = false;
    }

    // Ẩn bảng
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}