using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 25f;
    [SerializeField] private float timeDestroy = 0.5f;
    void Start()
    {
        Destroy(gameObject, timeDestroy);
    }

    
    void Update()
    {
        MoveBullet();
    }
    void MoveBullet()
    {
       transform.Translate(Vector2.right * moveSpeed * Time.deltaTime) ;
    }
}
