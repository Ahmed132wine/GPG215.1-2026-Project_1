using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI : MonoBehaviour
{
    [Header("UI Element Hooks")]
    public Slider healthBar;
    public TMP_Text ammoText;
    public TMP_Text statusText;
    public TMP_Text weaponText;
    public GameObject crosshairOverlay;

    [Header("Control Buttons")]
    public Button aimButton;
    public Button fireButton;
    public Button switchWeaponButton;
    public Button leftCoverButton;
    public Button rightCoverButton;

    private void OnEnable()
    {
        // Subscribe to Player State broadcasts (Observer Pattern)
        PlayerController.OnHealthChanged += UpdateHealthBar;
        PlayerController.OnAmmoChanged += UpdateAmmoCounter;
        PlayerController.OnStanceChanged += UpdateStanceUI;
        PlayerController.OnWeaponChanged += UpdateWeaponUI;
    }

    private void OnDisable()
    {
        // Unsubscribe on disable to prevent memory leaks or null reference exceptions
        PlayerController.OnHealthChanged -= UpdateHealthBar;
        PlayerController.OnAmmoChanged -= UpdateAmmoCounter;
        PlayerController.OnStanceChanged -= UpdateStanceUI;
        PlayerController.OnWeaponChanged -= UpdateWeaponUI;
    }

    private void Start()
    {
        // Bind UI input interactions directly
        if (aimButton != null) aimButton.onClick.AddListener(OnAimPressed);
        if (fireButton != null) fireButton.onClick.AddListener(OnFirePressed);
        if (switchWeaponButton != null) switchWeaponButton.onClick.AddListener(OnSwitchWeaponPressed);
        if (leftCoverButton != null) leftCoverButton.onClick.AddListener(OnLeftCoverPressed);
        if (rightCoverButton != null) rightCoverButton.onClick.AddListener(OnRightCoverPressed);
    }

    private void UpdateHealthBar(float currentHealth)
    {
        if (healthBar != null && PlayerController.Instance != null)
        {
            healthBar.value = currentHealth / PlayerController.Instance.maxHealth;
        }
    }

    private void UpdateAmmoCounter(int currentAmmo)
    {
        if (ammoText != null)
        {
            ammoText.text = $"AMMO: {currentAmmo}";
        }
    }

    private void UpdateWeaponUI(string weaponName)
    {
        if (weaponText != null)
        {
            weaponText.text = weaponName;
        }
    }

    private void UpdateStanceUI(PlayerController.StanceState stance)
    {
        if (statusText == null || crosshairOverlay == null) return;

        if (stance == PlayerController.StanceState.Aiming)
        {
            statusText.text = "VULNERABLE (AIMING)";
            statusText.color = Color.red;
            crosshairOverlay.SetActive(true);

            // Enable gyroscope adjustments when popped out of cover
            Camera.main.GetComponent<GyroCameraLook>()?.EnableGyro();
        }
        else
        {
            statusText.text = "SAFE IN COVER";
            statusText.color = Color.green;
            crosshairOverlay.SetActive(false);

            // Disable gyroscopes to lock orientation and reset camera view behind the cover
            Camera.main.GetComponent<GyroCameraLook>()?.DisableGyro();
            Camera.main.transform.localRotation = Quaternion.identity;
        }
    }

    private void OnAimPressed()
    {
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.ToggleStance();
        }
    }

    private void OnFirePressed()
    {
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.FireWeapon();
        }
    }

    private void OnSwitchWeaponPressed()
    {
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.SwitchWeapon();
        }
    }

    private void OnLeftCoverPressed()
    {
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.MoveLeft();
        }
    }

    private void OnRightCoverPressed()
    {
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.MoveRight();
        }
    }
}
