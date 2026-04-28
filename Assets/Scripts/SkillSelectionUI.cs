using UnityEngine;
using UnityEngine.UI;

public class SkillSelectionUI : MonoBehaviour
{
    [System.Serializable]
    public class SkillOption
    {
        public string skillName;    // Tên skill: "SpeedBoost", "Dash", "Pet"
        public Button button;       // Nút bấm
        public Text label;          // Nhãn hiển thị
    }

    [SerializeField] private SkillOption[] options;  // Mảng 3 lựa chọn
    public bool HasChosen { get; private set; }     // Đã chọn chưa?

    private Player player;

    void Start()
    {
        player = FindAnyObjectByType<Player>(); // Tìm Player trong scene
    }

    public void Show()
    {
        HasChosen = false;
        gameObject.SetActive(true);               // Hiện bảng
        Time.timeScale = 0f;                      // Dừng game (đề phòng)
        foreach (var opt in options)
        {
            opt.button.onClick.RemoveAllListeners();         // Xóa sự kiện cũ
            opt.button.onClick.AddListener(() => OnChoose(opt.skillName)); // Gán sự kiện mới
            opt.button.interactable = true;                  // Cho phép bấm
        }
    }

    void OnChoose(string skillName)
    {
        if (HasChosen) return;          // Chỉ chọn 1 lần
        HasChosen = true;

        // Mở khóa skill tương ứng trong Player
        switch (skillName)
        {
            case "SpeedBoost": player.UnlockSpeedBoost(); break;
            case "Dash": player.UnlockDash(); break;
            case "Pet": player.UnlockPet(); break;
        }

        // Vô hiệu hóa tất cả nút
        foreach (var opt in options) opt.button.interactable = false;
    }

    public void Hide()
    {
        gameObject.SetActive(false);    // Ẩn bảng
    }
}