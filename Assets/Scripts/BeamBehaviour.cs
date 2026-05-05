using UnityEngine;

public class BeamBehaviour : MonoBehaviour
{
    [SerializeField] private float damage = 25f;          // Sát thương khi chạm Player
    [SerializeField] private float lifetime = 1.5f;        // Thời gian tồn tại trước khi tự hủy
    [SerializeField] private LayerMask playerLayer;        // Layer của Player

    private bool hasDealtDamage = false;                  // Chỉ gây sát thương 1 lần

    void Start()
    {
        Destroy(gameObject, lifetime);                    // Tự hủy sau lifetime giây
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra nếu đối tượng va chạm thuộc layer Player và chưa gây sát thương
        if (!hasDealtDamage && ((1 << collision.gameObject.layer) & playerLayer) != 0)
        {
            Player player = collision.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(damage);
                hasDealtDamage = true;
                Debug.Log($"Beam gây {damage} sát thương cho Player");
            }
        }
    }
}