using UnityEngine;
using System.Collections;

public class OrbSpellController : MonoBehaviour
{
    [Header("Cấu hình")]
    public GameObject beamPrefab;          // Prefab 1 tia
    public int beamCount = 4;
    public float startAngleOffset = 0f;    // Góc bắt đầu
    public float rotationAmount = 90f;     // Góc xoay thêm
    public float rotationDuration = 1f;
    public float destroyDelay = 0.5f;

    private GameObject[] beams;

    void Start()
    {
        SpawnBeams();
        StartCoroutine(RotateAndDestroy());
    }

    void SpawnBeams()
    {
        beams = new GameObject[beamCount];
        float angleStep = 360f / beamCount;
        for (int i = 0; i < beamCount; i++)
        {
            float angle = startAngleOffset + i * angleStep;
            Quaternion rot = Quaternion.Euler(0f, 0f, angle);
            GameObject beam = Instantiate(beamPrefab, transform.position, rot, transform);
            beams[i] = beam;
        }
    }

    IEnumerator RotateAndDestroy()
    {
        float startAngle = transform.eulerAngles.z;
        float targetAngle = startAngle + rotationAmount;
        float elapsed = 0f;

        while (elapsed < rotationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / rotationDuration;
            float currentAngle = Mathf.Lerp(startAngle, targetAngle, t);
            transform.rotation = Quaternion.Euler(0f, 0f, currentAngle);
            yield return null;
        }

        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (beams != null)
        {
            foreach (GameObject beam in beams)
                if (beam != null) Destroy(beam);
        }
    }
}