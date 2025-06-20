using UnityEngine;

public class GroundGimmick : MonoBehaviour
{
    public float rotationSpeed = 0f;

    void Update()
    {
        RotaionGimmick();
    }

    void RotaionGimmick()
    {
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
    }
}
