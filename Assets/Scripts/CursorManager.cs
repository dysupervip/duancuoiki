using UnityEngine;

public class CursorManager : MonoBehaviour
{
    [SerializeField] private Texture2D cursorNormal;   // Con trỏ mặc định
    [SerializeField] private Texture2D cursorShoot;    // Con trỏ khi bắn
    [SerializeField] private Texture2D cursorReload;   // Con trỏ khi nạp đạn

    private Vector2 hotspot = new Vector2(16, 48);     // Điểm nóng (đầu mũi tên)
    private WeaponBase currentWeapon;                  // Vũ khí hiện tại (để kiểm tra IsReloading)
    private bool isShooting = false;                   // Đang giữ chuột trái?

    public static CursorManager Instance;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        Cursor.SetCursor(cursorNormal, hotspot, CursorMode.Auto); // Bắt đầu với con trỏ mặc định
    }

    void Update()
    {
        // Nếu đang nạp đạn, không thay đổi con trỏ (giữ nguyên cursorReload)
        if (currentWeapon != null && currentWeapon.IsReloading) return;

        // Nhấn chuột trái -> con trỏ bắn
        if (Input.GetMouseButtonDown(0))
        {
            isShooting = true;
            Cursor.SetCursor(cursorShoot, hotspot, CursorMode.Auto);
        }
        // Thả chuột trái -> con trỏ mặc định
        else if (Input.GetMouseButtonUp(0))
        {
            isShooting = false;
            Cursor.SetCursor(cursorNormal, hotspot, CursorMode.Auto);
        }
    }

    public void SetCurrentWeapon(WeaponBase weapon)
    {
        // Hủy đăng ký sự kiện ở vũ khí cũ
        if (currentWeapon != null)
        {
            currentWeapon.OnReloadStarted -= OnReloadStarted;
            currentWeapon.OnReloadFinished -= OnReloadFinished;
        }

        currentWeapon = weapon;

        // Đăng ký sự kiện cho vũ khí mới
        if (currentWeapon != null)
        {
            currentWeapon.OnReloadStarted += OnReloadStarted;
            currentWeapon.OnReloadFinished += OnReloadFinished;
        }
    }

    void OnReloadStarted()
    {
        // Đổi sang con trỏ nạp đạn
        Cursor.SetCursor(cursorReload, hotspot, CursorMode.Auto);
    }

    void OnReloadFinished()
    {
        // Khi nạp xong, trả về con trỏ thích hợp
        if (isShooting)
            Cursor.SetCursor(cursorShoot, hotspot, CursorMode.Auto);
        else
            Cursor.SetCursor(cursorNormal, hotspot, CursorMode.Auto);
    }
}