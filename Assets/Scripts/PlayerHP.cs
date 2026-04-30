using UnityEngine;
using UnityEngine.UI;

public class PlayerHP : MonoBehaviour
{
    public static PlayerHP Instance;

    [SerializeField] private Image hpFill;

    private void Awake()
    {
        Instance = this;
    }

    public void UpdateHP(float currentHP, float maxHP)
    {
        if (hpFill != null)
        {
            hpFill.fillAmount = currentHP / maxHP;
        }
    }
}