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
    [Tooltip("How fast the camera snaps up from a shot")]
    public float recoilKickSpeed = 20f;
    public float recoilRecoverySpeed = 8f;
    private Vector2 currentRecoilOffset;
    private Vector2 targetRecoilOffset;

    private float shakeTimer = 0f;
    private float shakeMagnitude = 0f;

    private void Start()
    {
        gyroSupported = SystemInfo.supportsGyroscope;
        if (gyroSupported)
        {
            gyro = Input.gyro;
        }

       
        mouseX = transform.localEulerAngles.y;
        mouseY = transform.localEulerAngles.x;
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

    public void SyncRotation()
    {
        mouseX = transform.localEulerAngles.y;
        mouseY = transform.localEulerAngles.x;
    }

    public void ApplyRecoil(float verticalKick, float horizontalKickMax)
    {

        float horizontalKick = Random.Range(-horizontalKickMax, horizontalKickMax);

       
        mouseX += horizontalKick * 0.3f;
        mouseY -= verticalKick * 0.4f; // Subtract to move camera up
        mouseY = Mathf.Clamp(mouseY, -40f, 40f);

        
        targetRecoilOffset += new Vector2(horizontalKick * 0.7f, -verticalKick * 0.6f);
    }
    
    public void TriggerShake(float duration, float magnitude)
    {
        shakeTimer = duration;
        shakeMagnitude = magnitude;
    }

    private void Update()
    {

        targetRecoilOffset = Vector2.Lerp(targetRecoilOffset, Vector2.zero, Time.deltaTime * recoilRecoverySpeed);
     
        currentRecoilOffset = Vector2.Lerp(currentRecoilOffset, targetRecoilOffset, Time.deltaTime * recoilKickSpeed);


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

        float currentShakeX = 0f;
        float currentShakeY = 0f;
        if (shakeTimer > 0)
        {
            currentShakeX = Random.Range(-1f, 1f) * shakeMagnitude;
            currentShakeY = Random.Range(-1f, 1f) * shakeMagnitude;
            shakeTimer -= Time.deltaTime;
        }

       
        
        Quaternion rotation = Quaternion.Euler(mouseY + currentRecoilOffset.y + currentShakeY, mouseX + currentRecoilOffset.x + currentShakeX, 0f);
        transform.localRotation = rotation;
        
    }
}