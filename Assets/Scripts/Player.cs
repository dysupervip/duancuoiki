using UnityEngine;
using UnityEngine.UI;
public class Player : MonoBehaviour
{
    [SerializeField] private float movespeed = 5f;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    [SerializeField] private float maxHp = 100f;
    private float currentHp;
    [SerializeField] private Image hpBar;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }
    void Start()
    {
        currentHp = maxHp;
        UpdateHpBar();
    }

    
    void Update()
    {
        MovePlayer();
    }
    void MovePlayer()
    {
        Vector2 playerInput= new Vector2(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical"));
        rb.linearVelocity = playerInput.normalized * movespeed;
        if(playerInput.x<0){
            spriteRenderer.flipX = true ;
        }else if(playerInput.x>0){
            spriteRenderer.flipX = false;
        }

        if(playerInput != Vector2.zero)
        {
            animator.SetBool("isRun",true);
        }
        else
        {
            animator.SetBool("isRun",false);
        }
    }
    public void TakeDamage(float damage)
    {
        currentHp -= damage;
        currentHp = Mathf.Max(currentHp, 0);
        UpdateHpBar();
        if (currentHp <=0 )
        {
            Die();
        }
    }
    public void Heal(float healValue)
    {
        if (currentHp < maxHp )
        {
            currentHp += healValue;
            currentHp = Mathf.Min(currentHp,maxHp);
            UpdateHpBar();
        }
    }
    private void Die()
    {
        Destroy(gameObject);
    }
    private void UpdateHpBar()
    {
        hpBar.fillAmount = currentHp / maxHp;
    }
}
