using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    // Mảng chứa các prefab quái thường (kéo prefab vào Inspector)
    [SerializeField] private GameObject[] normalEnemyPrefabs;
    // Các điểm spawn (Transform) đặt sẵn trong scene
    [SerializeField] private Transform[] spawnPoints;

    /// <summary>
    /// Spawn ngẫu nhiên một quái thường từ danh sách prefab.
    /// </summary>
    public GameObject SpawnRandomEnemy()
    {
        // Kiểm tra nếu chưa có prefab nào
        if (normalEnemyPrefabs.Length == 0)
        {
            Debug.LogError("EnemySpawner: Chưa có prefab quái trong mảng!");
            return null;
        }
        // Chọn ngẫu nhiên một prefab
        GameObject prefab = normalEnemyPrefabs[Random.Range(0, normalEnemyPrefabs.Length)];
        // Chọn ngẫu nhiên một điểm spawn
        Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
        // Tạo enemy tại điểm spawn, không xoay
        return Instantiate(prefab, point.position, Quaternion.identity);
    }
    /// Spawn boss tại điểm spawn ngẫu nhiên.
    /// Spawn một enemy cụ thể (theo prefab) tại điểm spawn ngẫu nhiên.
    
    public GameObject SpawnSpecificEnemy(GameObject enemyPrefab)
    {
        if (spawnPoints.Length == 0 || enemyPrefab == null)
        {
         Debug.LogError("EnemySpawner: Thiếu spawn point hoặc enemy prefab!");
         return null;
        }
        Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
        return Instantiate(enemyPrefab, point.position, Quaternion.identity);
    }
    public GameObject SpawnBoss(GameObject bossPrefab)
    {
        // Nếu không có điểm spawn hoặc bossPrefab null thì báo lỗi
        if (spawnPoints.Length == 0 || bossPrefab == null)
        {
            Debug.LogError("EnemySpawner: Thiếu spawn point hoặc boss prefab!");
            return null;
        }
        Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
        return Instantiate(bossPrefab, point.position, Quaternion.identity);
    }
}