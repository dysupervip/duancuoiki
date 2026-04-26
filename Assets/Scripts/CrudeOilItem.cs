using UnityEngine;

public class CrudeOilItem : MonoBehaviour
{
    [SerializeField] private int oilAmount = 1; // Mỗi lần nhặt được 1 dầu (có thể tăng)

    // Khi có Collider khác chạm vào (trigger)
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // Nếu là người chơi
        {
            Player player = other.GetComponent<Player>(); // Lấy script Player
            if (player != null)
            {
                player.AddCrudeOil(oilAmount); // Thêm dầu
                Destroy(gameObject); // Xóa vật phẩm dầu
            }
        }
    }
}