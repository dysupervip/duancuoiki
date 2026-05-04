using UnityEngine;
using System.Collections;

public class BeamSpell1 : MonoBehaviour
{
    [Header("Các phần của tia")]
    public GameObject cloud;
    public GameObject head;
    public GameObject mid;
    public GameObject foot;

    [Header("Thời gian hiển thị (giây)")]
    public float cloudDuration = 0.5f;
    public float headDelay = 0.2f;
    public float midDelay = 0.15f;
    public float footDelay = 0.3f;
    public float destroyDelay = 1f;

    void Start()
    {
        // Ban đầu tắt tất cả
        cloud.SetActive(false);
        head.SetActive(false);
        mid.SetActive(false);
        foot.SetActive(false);

        StartCoroutine(PlaySequence());
    }

    IEnumerator PlaySequence()
    {
        // 1. Cloud
        cloud.SetActive(true);
        yield return new WaitForSeconds(cloudDuration);

        // 2. Head
        head.SetActive(true);
        yield return new WaitForSeconds(headDelay);

        // 3. Mid
        mid.SetActive(true);
        yield return new WaitForSeconds(midDelay);

        // 4. Foot
        foot.SetActive(true);
        // Có thể gây sát thương ở đây
        yield return new WaitForSeconds(destroyDelay);

        Destroy(gameObject);
    }
}