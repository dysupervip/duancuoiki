using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [System.Serializable]
    public class Phase
    {
        public bool isBossPhase;              // Phase boss?
        public GameObject bossPrefab;         // Prefab boss (nếu là boss phase)
        public int totalEnemies = 10;         // Số quái cần tiêu diệt (với phase thường)
        public float spawnDelay = 1.5f;       // Thời gian nghỉ giữa các lần spawn
        public WeaponBase newWeaponPrefab;    // Súng mới sẽ trang bị sau phase thường
        public int oilDropQuota = 3;          // Số dầu tối đa có thể rơi trong phase này
    }

    [SerializeField] private List<Phase> phases;                 // Danh sách các phase
    [SerializeField] private EnemySpawner spawner;               // Tham chiếu tới EnemySpawner
    [SerializeField] private Player player;                      // Tham chiếu tới Player
    [SerializeField] private UpgradeSelectionUI upgradeUI;       // Bảng chọn nâng cấp chỉ số

    [Header("Mở khóa enemy theo phase")]
    [SerializeField] private List<GameObject> enemyLibrary;      // Các loại enemy theo thứ tự mở khóa

    private List<GameObject> unlockedEnemyPrefabs;               // Danh sách enemy đã mở khóa

    private int currentPhaseIndex = 0;   // Phase hiện tại (0..3)
    private int enemiesKilled;           // Số quái đã bị tiêu diệt trong phase hiện tại
    private int enemiesSpawned;          // Số quái đã spawn trong phase hiện tại
    private int oilRemaining;            // Số dầu còn lại có thể rơi trong phase hiện tại

    public static WaveManager Instance;  // Singleton

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        unlockedEnemyPrefabs = new List<GameObject>();
        StartCoroutine(RunPhase(currentPhaseIndex));
    }

    // Coroutine điều khiển 1 phase
    IEnumerator RunPhase(int index)
    {
        if (index >= phases.Count) yield break; // Hết phase thì dừng

        Phase phase = phases[index];
        oilRemaining = phase.oilDropQuota; // Đặt lại quota dầu

        if (!phase.isBossPhase)
        {
            // ===== PHASE THƯỜNG =====

            // --- Mở khóa enemy mới (nếu còn) ---
            if (currentPhaseIndex < enemyLibrary.Count)
            {
                GameObject newEnemy = enemyLibrary[currentPhaseIndex];
                if (!unlockedEnemyPrefabs.Contains(newEnemy))
                {
                    unlockedEnemyPrefabs.Add(newEnemy);
                    Debug.Log($"Mở khóa enemy mới: {newEnemy.name}");
                }
            }

            // --- Reset đếm ---
            enemiesKilled = 0;
            enemiesSpawned = 0;

            // --- Vòng lặp spawn + kiểm tra điều kiện kết thúc ---
            while (enemiesKilled < phase.totalEnemies)
            {
                // Spawn quái nếu còn quota spawn
                if (enemiesSpawned < phase.totalEnemies)
                {
                    yield return new WaitForSeconds(phase.spawnDelay);

                    if (unlockedEnemyPrefabs.Count > 0)
                    {
                        GameObject prefab = unlockedEnemyPrefabs[Random.Range(0, unlockedEnemyPrefabs.Count)];
                        spawner.SpawnSpecificEnemy(prefab);
                        enemiesSpawned++;
                    }
                    else
                    {
                        Debug.LogError("Chưa có enemy nào được mở khóa!");
                        break;
                    }
                }
                else
                {
                    // Đã spawn đủ, chỉ chờ người chơi tiêu diệt hết
                    // Nhưng nếu không còn enemy nào trên scene mà vẫn chưa đủ, ta kết thúc luôn
                    if (GameObject.FindGameObjectsWithTag("Enemy").Length == 0)
                    {
                        Debug.LogWarning($"Không còn enemy, nhưng mới tiêu diệt {enemiesKilled}/{phase.totalEnemies}. Kết thúc phase.");
                        break;
                    }
                    yield return null; // chờ 1 frame rồi kiểm tra tiếp
                }
            }

            // --- Tự động trang bị súng mới (nếu có) ---
            if (phase.newWeaponPrefab != null)
                player.EquipWeapon(phase.newWeaponPrefab);

            // --- Hiển thị nâng cấp chỉ số ---
            yield return StartCoroutine(ShowStatUpgrade());
        }
        else
        {
            // ===== PHASE BOSS =====
            yield return new WaitForSeconds(1f); // Đợi 1 giây trước khi spawn boss
            spawner.SpawnBoss(phase.bossPrefab);
            // Với boss, ta dùng cách đợi đến khi boss chết (dùng tag "Enemy")
            yield return new WaitWhile(() => GameObject.FindGameObjectsWithTag("Enemy").Length > 0);
        }

        // Chuyển sang phase tiếp theo
        currentPhaseIndex++;
        StartCoroutine(RunPhase(currentPhaseIndex));
    }

    // Hiện UI chọn nâng cấp (tạm dừng game)
    IEnumerator ShowStatUpgrade()
    {
        Time.timeScale = 0f; // Đóng băng game
        upgradeUI.Show();    // Hiện bảng chọn
        yield return new WaitUntil(() => upgradeUI.HasChosen); // Chờ người chơi chọn xong
        upgradeUI.Hide();
        Time.timeScale = 1f; // Tiếp tục game
    }

    // Được gọi từ Enemy khi nó chết
    public void HandleEnemyDeath()
    {
        enemiesKilled++;
    }

    // Được Enemy gọi để xin rơi dầu; trả về true nếu còn quota và đã rơi
    public bool TryDropOil(Vector3 position)
    {
        if (oilRemaining > 0)
        {
            oilRemaining--;
            // Load prefab dầu từ thư mục Resources (phải có file tên "CrudeOilItem")
            GameObject oilPrefab = Resources.Load<GameObject>("CrudeOilItem");
            if (oilPrefab != null)
                Instantiate(oilPrefab, position, Quaternion.identity);
            return true;
        }
        return false;
    }
}