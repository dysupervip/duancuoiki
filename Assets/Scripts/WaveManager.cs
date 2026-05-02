using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [System.Serializable]
    public class Phase
    {
        public bool isBossPhase;
        public GameObject bossPrefab;
        public int totalEnemies = 10;
        public float spawnDelay = 1.5f;
        public int oilDropQuota = 3;
        // Hỗ trợ MiniBoss trong phase thường
        public bool spawnMiniBoss = false;
        public GameObject miniBossPrefab;
    }

    [SerializeField] private List<Phase> phases;
    [SerializeField] private EnemySpawner spawner;
    [SerializeField] private Player player;
    [SerializeField] private SkillSelectionUI skillSelectionUI;

    [Header("Mở khóa enemy theo phase")]
    [SerializeField] private List<GameObject> enemyLibrary; // 3 loại enemy thường

    private List<GameObject> unlockedEnemyPrefabs = new List<GameObject>();
    private int currentPhaseIndex = 0;
    private int enemiesKilled;
    private int enemiesSpawned;
    private int oilRemaining;

    public static WaveManager Instance;

    void Awake() { Instance = this; }
    void Start() { StartCoroutine(RunPhase(currentPhaseIndex)); }

    IEnumerator RunPhase(int index)
    {
        if (index >= phases.Count) yield break;
        Phase phase = phases[index];
        oilRemaining = phase.oilDropQuota;
        if (ThanhTienTrinhUI.Instance != null)
        {
            ThanhTienTrinhUI.Instance.ResetBar();
            ThanhTienTrinhUI.Instance.SetColor(phase.isBossPhase ? new Color(0.6f, 0f, 1f) : Color.yellow);
        }

        if (!phase.isBossPhase)
        {
            // Mở khóa enemy mới (nếu còn)
            if (currentPhaseIndex < enemyLibrary.Count)
            {
                GameObject newEnemy = enemyLibrary[currentPhaseIndex];
                if (!unlockedEnemyPrefabs.Contains(newEnemy))
                    unlockedEnemyPrefabs.Add(newEnemy);
            }

            enemiesKilled = 0;
            enemiesSpawned = 0;

            // Spawn MiniBoss ngay từ đầu nếu có
            if (phase.spawnMiniBoss && phase.miniBossPrefab != null)
            {
                spawner.SpawnSpecificEnemy(phase.miniBossPrefab);
                enemiesSpawned++;
            }

            // Vòng lặp spawn quái thường
            while (enemiesKilled < phase.totalEnemies)
            {
                if (enemiesSpawned < phase.totalEnemies)
                {
                    yield return new WaitForSeconds(phase.spawnDelay);
                    if (unlockedEnemyPrefabs.Count > 0)
                    {
                        GameObject prefab = unlockedEnemyPrefabs[Random.Range(0, unlockedEnemyPrefabs.Count)];
                        spawner.SpawnSpecificEnemy(prefab);
                        enemiesSpawned++;
                    }
                }
                else
                {
                    if (GameObject.FindGameObjectsWithTag("Enemy").Length == 0) break;
                    yield return null;
                }
            }

            // Chỉ hiển thị chọn kỹ năng nếu còn kỹ năng chưa mở khóa
            if (skillSelectionUI.HasAvailableSkills())
            {
                yield return StartCoroutine(ShowSkillSelection());
            }
            else
            {
                Debug.Log("Tất cả kỹ năng đã mở khóa. Bỏ qua chọn kỹ năng.");
            }
        }
        else
        {
            // Phase boss (phase 4)
            yield return new WaitForSeconds(1f);
            GameObject boss = spawner.SpawnBoss(phase.bossPrefab);
            Enemy bossEnemy = boss.GetComponent<Enemy>();
            bossEnemy.HideHpBar();
            while (bossEnemy != null)
            {
                ThanhTienTrinhUI.Instance.UpdateBossHP(bossEnemy.GetCurrentHP(), bossEnemy.GetMaxHP() / 3f);
                yield return null;
            }
        }

        currentPhaseIndex++;
        StartCoroutine(RunPhase(currentPhaseIndex));
    }

    IEnumerator ShowSkillSelection()
    {
        Time.timeScale = 0f;
        skillSelectionUI.Show();
        yield return new WaitUntil(() => skillSelectionUI.HasChosen);
        skillSelectionUI.Hide();
        Time.timeScale = 1f;
    }

    public void HandleEnemyDeath()
    {
        enemiesKilled++;
        if (ThanhTienTrinhUI.Instance != null)
            ThanhTienTrinhUI.Instance.UpdateProgress(enemiesKilled, phases[currentPhaseIndex].totalEnemies);
    }

    public bool TryDropOil(Vector3 position)
    {
        if (oilRemaining > 0)
        {
            oilRemaining--;
            GameObject oilPrefab = Resources.Load<GameObject>("CrudeOilItem");
            if (oilPrefab != null) Instantiate(oilPrefab, position, Quaternion.identity);
            return true;
        }
        return false;
    }
}