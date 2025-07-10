using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    private Vector3 offset;

    public void Initialize(Vector3 puppetPosition)
    {
        offset = transform.position - puppetPosition;
    }

    public void UpdatePosition(Vector3 puppetPosition)
    {
        Vector3 desiredPosition = puppetPosition + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, 5f * Time.deltaTime);
    }
}
