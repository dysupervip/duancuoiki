using UnityEngine;

public class CrudeOilItem : MonoBehaviour
{
    [SerializeField] private int oilAmount = 1;
    [SerializeField] private AudioClip collectSound;

    private bool collected = false;

    // Khi có Collider khác chạm vào (trigger)
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;

        if (other.CompareTag("Player")) // Nếu là người chơi
        {
            collected = true;

            Player player = other.GetComponent<Player>();

            if (player != null)
            {
                // Thêm dầu
                player.AddCrudeOil(oilAmount);

                // Phát âm thanh nhặt dầu
                if (collectSound != null)
                {
                    AudioSource.PlayClipAtPoint(
                        collectSound,
                        transform.position
                    );
                }

                // Xóa vật phẩm dầu
                Destroy(transform.root.gameObject);
            }
        }
    }
}