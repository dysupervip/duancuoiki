using UnityEngine;
using UnityEngine.UI;

public class SkillSelectionUI : MonoBehaviour
{
    [System.Serializable]
    public class SkillOption
    {
        public string skillName;   // Tên kỹ năng: "SpeedBoost", "Dash", "Pet"
        public Button button;      // Nút bấm
        public Text label;         // Nhãn hiển thị
    }

    [SerializeField] private SkillOption[] options;  // Mảng 3 lựa chọn
    public bool HasChosen { get; private set; }      // Đã chọn xong chưa?

    private Player player;

    // Không dùng Start nữa vì Player có thể chưa tồn tại lúc panel được tạo
    public void Show()
    {
        // Tìm Player mỗi lần Show để chắc chắn không bị null
        player = FindAnyObjectByType<Player>();
        if (player == null)
        {
            Debug.LogError("Player không tồn tại! Không thể hiển thị bảng kỹ năng.");
            gameObject.SetActive(false);
            return;
        }

        HasChosen = false;
        gameObject.SetActive(true);
        // Không tự ý đặt Time.timeScale, để WaveManager quản lý

        foreach (var opt in options)
        {
            // Kiểm tra kỹ năng đã được mở khóa chưa
            bool alreadyUnlocked = false;
            switch (opt.skillName)
            {
                case "SpeedBoost": alreadyUnlocked = player.HasSpeedBoost; break;
                case "Dash": alreadyUnlocked = player.HasDash; break;
                case "Pet": alreadyUnlocked = player.HasPet; break;
            }

            if (alreadyUnlocked)
            {
                // Ẩn nút nếu đã sở hữu
                opt.button.gameObject.SetActive(false);
            }
            else
            {
                // Hiện nút và gán sự kiện
                opt.button.gameObject.SetActive(true);

                // Tạo biến tạm để tránh lỗi closure
                string skillNameLocal = opt.skillName;
                opt.button.onClick.RemoveAllListeners();
                opt.button.onClick.AddListener(() => OnChoose(skillNameLocal));
                opt.button.interactable = true;
            }
        }
    }

    void OnChoose(string skillName)
    {
        if (HasChosen) return;
        HasChosen = true;

        // Mở khóa kỹ năng trong Player
        switch (skillName)
        {
            case "SpeedBoost": player.UnlockSpeedBoost(); break;
            case "Dash": player.UnlockDash(); break;
            case "Pet": player.UnlockPet(); break;
        }

        // Vô hiệu hóa tất cả nút
        foreach (var opt in options)
            opt.button.interactable = false;

        // Tự ẩn panel
        gameObject.SetActive(false);
    }

    public void Hide()
    {
        if (gameObject.activeSelf)
            gameObject.SetActive(false);
    }
}