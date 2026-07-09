using UnityEngine;

public class GyroCameraLook : MonoBehaviour
{
    private bool gyroSupported;
    private Gyroscope gyro;
    private Quaternion baseRotation = Quaternion.Euler(90, 0, 90);

    [Header("Aim Sensitivity")]
    public float gyroSensitivity = 1.5f;
    public float mouseSensitivity = 1.5f;

    private float mouseX = 0f;
    private float mouseY = 0f;

    [Header("Recoil System")]
    [Tooltip("How fast the camera returns to original aim after recoil kick")]
    public float recoilRecoverySpeed = 8f;
    private Vector2 currentRecoilOffset;
    private Vector2 targetRecoilOffset;

    private void Start()
    {
        gyroSupported = SystemInfo.supportsGyroscope;
        if (gyroSupported)
        {
            gyro = Input.gyro;
        }
    }

    public void EnableGyro()
    {
        if (gyroSupported)
        {
            Input.gyro.enabled = true;
        }
        else
        {
            // Reset mouse orientation relative to current camera looking vector
            mouseX = transform.localEulerAngles.y;
            mouseY = transform.localEulerAngles.x;
        }

        // Reset recoil offsets when returning to aim
        currentRecoilOffset = Vector2.zero;
        targetRecoilOffset = Vector2.zero;
    }

    public void DisableGyro()
    {
        if (gyroSupported)
        {
            Input.gyro.enabled = false;
        }
    }

    
    public void ApplyRecoil(float verticalKick, float horizontalKickMax)
    {
        
        float horizontalKick = Random.Range(-horizontalKickMax, horizontalKickMax);
        targetRecoilOffset += new Vector2(horizontalKick, -verticalKick);
    }

    private void Update()
    {
        
        targetRecoilOffset = Vector2.Lerp(targetRecoilOffset, Vector2.zero, Time.deltaTime * recoilRecoverySpeed);
        currentRecoilOffset = Vector2.Lerp(currentRecoilOffset, targetRecoilOffset, Time.deltaTime * (recoilRecoverySpeed * 2f));

        
        if (Input.touchCount > 0 || Input.GetMouseButton(0))
        {
            float deltaX = 0f;
            float deltaY = 0f;

            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Moved)
                {
                    deltaX = touch.deltaPosition.x * (mouseSensitivity * 0.1f);
                    deltaY = touch.deltaPosition.y * (mouseSensitivity * 0.1f);
                }
            }
            else
            {
                deltaX = Input.GetAxis("Mouse X") * mouseSensitivity;
                deltaY = Input.GetAxis("Mouse Y") * mouseSensitivity;
            }

            mouseX += deltaX;
            mouseY -= deltaY;
            mouseY = Mathf.Clamp(mouseY, -40f, 40f); 
        }

        
        Quaternion rotation = Quaternion.Euler(mouseY + currentRecoilOffset.y, mouseX + currentRecoilOffset.x, 0f);
        transform.localRotation = rotation;
    }
}