using UnityEngine; // Bắt buộc để dùng các lớp của Unity (MonoBehaviour, GameObject...)

public class GameManager : MonoBehaviour
{
    // Singleton: cho phép truy cập GameManager từ bất kỳ script nào bằng GameManager.Instance
    public static GameManager Instance;

    [Header("Kết thúc game")]
    [SerializeField] private GameObject victoryPanel; // Kéo Panel chiến thắng vào đây trong Inspector

    void Awake()
    {
        // Đảm bảo chỉ có 1 GameManager tồn tại
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject); // Nếu đã có thì hủy bản mới
    }

    // Hàm được gọi khi người chơi thắng (nhặt chìa khóa boss)
    public void WinGame()
    {
        Debug.Log("Người chơi thắng!"); // In ra console
        victoryPanel.SetActive(true);    // Hiện bảng chiến thắng
        Time.timeScale = 0f;            // Dừng toàn bộ game (đóng băng thời gian)
    }
}