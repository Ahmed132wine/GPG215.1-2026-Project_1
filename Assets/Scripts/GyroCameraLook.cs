using UnityEngine;

public class GyroCameraLook : MonoBehaviour
{
    private bool gyroSupported;
    private Gyroscope gyro;
    private Quaternion baseRotation = Quaternion.Euler(90, 0, 90);

    [Header("Aim Sensitivity")]
    public float gyroSensitivity = 1.5f;
    public float mouseSensitivity = 3f;

    private float mouseX = 0f;
    private float mouseY = 0f;

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
    }

    public void DisableGyro()
    {
        if (gyroSupported)
        {
            Input.gyro.enabled = false;
        }
    }

    private void Update()
    {
        if (gyroSupported && Input.gyro.enabled)
        {
            
            Quaternion gyroRotation = Input.gyro.attitude;
            // Apply correct mobile rotation mapping
            transform.localRotation = baseRotation * new Quaternion(-gyroRotation.x, -gyroRotation.y, gyroRotation.z, gyroRotation.w);
        }
        else
        {
            
            if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
            {
                mouseX += Input.GetAxis("Mouse X") * mouseSensitivity;
                mouseY -= Input.GetAxis("Mouse Y") * mouseSensitivity;
                mouseY = Mathf.Clamp(mouseY, -40f, 40f); // Lock vertical viewport bounds

                transform.localRotation = Quaternion.Euler(mouseY, mouseX, 0f);
            }
        }
    }
}
