using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;                       // Player/CameraTarget
    public Vector3 targetOffset = new Vector3(0f, 0.3f, 0f);

    [Header("Distance")]
    public float distance = 4.5f;
    public float minDistance = 1.5f;
    public float maxDistance = 7.0f;
    public float zoomSpeed = 2.0f;

    [Header("Height")]
    public float height = 2.1f;

    [Header("Rotation")]
    public float mouseSensitivity = 2.0f;
    public float pitchMin = -15f;                  // daha az aşağı baksın
    public float pitchMax = 80f;                   // daha çok yukarı baksın
    public bool invertMouseY = false;

    [Header("Start View")]
    public bool overrideStartAngles = true;
    public float startYaw = 0f;
    public float startPitch = 15f;                 // başlangıçta aşağı bakmasın

    [Header("Smoothing")]
    public float positionSmooth = 15f;
    public float rotationSmooth = 20f;

    [Header("Collision")]
    public LayerMask collisionMask = ~0;           // her şey
    public float collisionRadius = 0.25f;
    public float collisionPadding = 0.1f;

    [Header("Cursor")]
    public bool lockCursor = true;

    float yaw;
    float pitch;
    float currentDistance;

    void Start()
    {
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (overrideStartAngles)
        {
            yaw = startYaw;
            pitch = startPitch;
        }
        else
        {
            Vector3 e = transform.eulerAngles;
            yaw = e.y;
            pitch = e.x;
        }

        currentDistance = Mathf.Clamp(distance, minDistance, maxDistance);
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Mouse look
        float mx = Input.GetAxis("Mouse X");
        float my = Input.GetAxis("Mouse Y");

        yaw += mx * mouseSensitivity;

        if (!invertMouseY)
            pitch -= my * mouseSensitivity;
        else
            pitch += my * mouseSensitivity;

        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

        // Zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.0001f)
        {
            currentDistance -= scroll * zoomSpeed;
            currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
        }

        Quaternion desiredRot = Quaternion.Euler(pitch, yaw, 0f);

        // Kamera odak noktası
        Vector3 focusPoint = target.position + Vector3.up * height + targetOffset;

        // İstenilen kamera pozisyonu
        Vector3 desiredCamPos = focusPoint - (desiredRot * Vector3.forward) * currentDistance;

        // Collision (duvara girmesin)
        Vector3 castDir = (desiredCamPos - focusPoint).normalized;
        float castDist = Vector3.Distance(focusPoint, desiredCamPos);

        if (Physics.SphereCast(focusPoint, collisionRadius, castDir, out RaycastHit hit,
                               castDist, collisionMask, QueryTriggerInteraction.Ignore))
        {
            float safeDist = Mathf.Max(hit.distance - collisionPadding, 0.1f);
            desiredCamPos = focusPoint + castDir * safeDist;
        }

        // Smooth
        transform.position = Vector3.Lerp(transform.position, desiredCamPos, positionSmooth * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, rotationSmooth * Time.deltaTime);
    }
}