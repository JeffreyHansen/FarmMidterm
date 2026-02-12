using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Camera Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 2f, -5f);
    [SerializeField] private float followSpeed = 10f;
    [SerializeField] private float rotateSpeed = 120f;

    private float yaw;

    void LateUpdate()
    {
        if (!target) return;

        HandleRotation();
        HandleFollow();
    }

    private void HandleRotation()
    {
        if (Mouse.current == null) return;

        float mouseX = Mouse.current.delta.x.ReadValue();
        yaw += mouseX * rotateSpeed * Time.deltaTime;
    }

    private void HandleFollow()
    {
        Quaternion rotation = Quaternion.Euler(0f, yaw, 0f);
        Vector3 desiredPosition = target.position + rotation * offset;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            followSpeed * Time.deltaTime
        );

        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}
