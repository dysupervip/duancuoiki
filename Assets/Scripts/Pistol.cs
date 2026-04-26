using UnityEngine;
public class Pistol : WeaponBase
{
    private float rotateOffset = 180f;

    protected override void RotateGun()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 displacement = transform.position - mousePos;
        float angle = Mathf.Atan2(displacement.y, displacement.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle + rotateOffset);

        if (angle < -90 || angle > 90)
            transform.localScale = new Vector3(1, 1, 1);
        else
            transform.localScale = new Vector3(1, -1, 1);
    }
}