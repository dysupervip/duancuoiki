using UnityEngine;
using UnityEngine.UI;

public class ThanhTienTrinhUI : MonoBehaviour
{
    public static ThanhTienTrinhUI Instance;

    [SerializeField] private Image fillImage;

    private void Awake()
    {
        Instance = this;
    }

    public void UpdateProgress(int killed, int total)
    {
        if (total <= 0) return;

        float fill = 1f - ((float)killed / total);
        fillImage.fillAmount = fill;
    }

    public void ResetBar()
    {
        fillImage.fillAmount = 1f;
    }

    public void UpdateBossHP(float currentHp, float maxHp)
    {
        if (maxHp <= 0) return;

        fillImage.fillAmount = currentHp / maxHp;
    }

    public void SetColor(Color color)
    {
        fillImage.color = color;
    }
}