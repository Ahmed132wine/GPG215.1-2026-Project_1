using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class UI : MonoBehaviour
{
    public Slider healthBar;
    public TMP_Text ammoText;
    public TMP_Text statusText;
    public TMP_Text weaponText;
    public GameObject crosshairOverlay;

    public Image focusVignette;
    public Image damageFlashOverlay;

    public GameObject pauseMenuPanel;
    public GameObject deathScreenPanel;
    public GameObject winScreenPanel;

    public Button pauseButton;
    public Button resumeButton;
    public Button[] restartButtons;
    public Button[] quitButtons;

    public Button aimButton;
    public Button fireButton;
    public Button switchWeaponButton;
    public Button leftCoverButton;
    public Button rightCoverButton;

    private Coroutine flashCoroutine;

    private void OnEnable()
    {
        PlayerController.OnHealthChanged += UpdateHealthBar;
        PlayerController.OnAmmoChanged += UpdateAmmoCounter;
        PlayerController.OnStanceChanged += UpdateStanceUI;
        PlayerController.OnWeaponChanged += UpdateWeaponUI;
        PlayerController.OnPlayerDamaged += TriggerDamageFlash;
        PlayerController.OnPlayerDeath += ShowDeathScreen;
        LevelManager.OnLevelWin += ShowWinScreen;
    }

    private void OnDisable()
    {
        PlayerController.OnHealthChanged -= UpdateHealthBar;
        PlayerController.OnAmmoChanged -= UpdateAmmoCounter;
        PlayerController.OnStanceChanged -= UpdateStanceUI;
        PlayerController.OnWeaponChanged -= UpdateWeaponUI;
        PlayerController.OnPlayerDamaged -= TriggerDamageFlash;
        PlayerController.OnPlayerDeath -= ShowDeathScreen;
        LevelManager.OnLevelWin -= ShowWinScreen;
    }

    private void Start()
    {
        if (aimButton != null) aimButton.onClick.AddListener(OnAimPressed);
        if (fireButton != null) fireButton.onClick.AddListener(OnFirePressed);
        if (switchWeaponButton != null) switchWeaponButton.onClick.AddListener(OnSwitchWeaponPressed);
        if (leftCoverButton != null) leftCoverButton.onClick.AddListener(OnLeftCoverPressed);
        if (rightCoverButton != null) rightCoverButton.onClick.AddListener(OnRightCoverPressed);

        if (pauseButton != null) pauseButton.onClick.AddListener(PauseGame);
        if (resumeButton != null) resumeButton.onClick.AddListener(ResumeGame);
        foreach (Button btn in restartButtons) { if (btn != null) btn.onClick.AddListener(RestartGame); }
        foreach (Button btn in quitButtons) { if (btn != null) btn.onClick.AddListener(QuitGame); }

        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (deathScreenPanel != null) deathScreenPanel.SetActive(false);
        if (winScreenPanel != null) winScreenPanel.SetActive(false);
    }

    public void SetGameplayUIVisible(bool isVisible)
    {
        if (healthBar != null) healthBar.gameObject.SetActive(isVisible);
        if (ammoText != null) ammoText.gameObject.SetActive(isVisible);
        if (statusText != null) statusText.gameObject.SetActive(isVisible);
        if (weaponText != null) weaponText.gameObject.SetActive(isVisible);
        if (aimButton != null) aimButton.gameObject.SetActive(isVisible);
        if (fireButton != null) fireButton.gameObject.SetActive(isVisible);
        if (switchWeaponButton != null) switchWeaponButton.gameObject.SetActive(isVisible);
        if (leftCoverButton != null) leftCoverButton.gameObject.SetActive(isVisible);
        if (rightCoverButton != null) rightCoverButton.gameObject.SetActive(isVisible);
        if (pauseButton != null) pauseButton.gameObject.SetActive(isVisible);

        if (isVisible && PlayerController.Instance != null)
        {
            UpdateStanceUI(PlayerController.Instance.currentStance);
        }
        else
        {
            if (crosshairOverlay != null) crosshairOverlay.SetActive(false);
            if (focusVignette != null) focusVignette.enabled = false;
        }
    }

    private void PauseGame()
    {
        Time.timeScale = 0f;
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
    }

    private void ResumeGame()
    {
        Time.timeScale = 1f;
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
    }

    private void ShowDeathScreen()
    {
        StartCoroutine(DeathMenuRoutine());
    }

    private IEnumerator DeathMenuRoutine()
    {
        yield return new WaitForSeconds(1.5f);
        Time.timeScale = 0f;
        SetGameplayUIVisible(false);
        if (deathScreenPanel != null) deathScreenPanel.SetActive(true);
    }

    private void ShowWinScreen()
    {
        SetGameplayUIVisible(false);
        if (winScreenPanel != null) winScreenPanel.SetActive(true);
    }

    private void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void QuitGame()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
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

            if (focusVignette != null)
            {
                focusVignette.enabled = true;
            }

            Camera.main.GetComponent<GyroCameraLook>()?.EnableGyro();
        }
        else
        {
            statusText.text = "IN COVER";
            statusText.color = Color.green;
            crosshairOverlay.SetActive(false);

            if (focusVignette != null)
            {
                focusVignette.enabled = false;
            }

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

    private void TriggerDamageFlash()
    {
        if (damageFlashOverlay != null && gameObject.activeInHierarchy)
        {
            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
            }
            flashCoroutine = StartCoroutine(DamageFlashRoutine());
        }
    }

    private IEnumerator DamageFlashRoutine()
    {
        damageFlashOverlay.color = new Color(1f, 0f, 0f, 0.4f);
        damageFlashOverlay.gameObject.SetActive(true);

        float elapsed = 0f;
        float flashDuration = 0.2f;

        while (elapsed < flashDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(0.4f, 0f, elapsed / flashDuration);
            damageFlashOverlay.color = new Color(1f, 0f, 0f, alpha);
            yield return null;
        }

        damageFlashOverlay.color = new Color(1f, 0f, 0f, 0f);
        damageFlashOverlay.gameObject.SetActive(false);
    }
}
