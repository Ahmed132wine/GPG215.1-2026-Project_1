using UnityEngine;
using System.Collections;
using System;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    public enum StanceState { InCover, Aiming }
    public StanceState currentStance = StanceState.InCover;

    public IWeaponStrategy currentWeapon;
    private int weaponIndex = 0;
    private IWeaponStrategy[] weapons;

    public float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    [SerializeField] private int currentAmmo;

    public Transform leftCoverNode;
    public Transform rightCoverNode;
    public float movementSpeed = 8f;
    private Transform targetCoverNode;

    public Transform playerCamera;
    public Vector3 coverCameraLocalPosition = new Vector3(0f, 1.5f, -5f);
    public Vector3 aimCameraLocalPosition = new Vector3(0f, 1.5f, 0.6f);
    public float coverFOV = 60f;
    public float aimFOV = 35f;
    public float cameraTransitionSpeed = 12f;

    public float healthRegenRate = 5f;

    public Renderer playerRenderer;

    private float groundY;

    public static event Action<float> OnHealthChanged;
    public static event Action<int> OnAmmoChanged;
    public static event Action<StanceState> OnStanceChanged;
    public static event Action<string> OnWeaponChanged;
    public static event Action OnPlayerDamaged;
    public static event Action OnPlayerDeath;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        weapons = new IWeaponStrategy[] {
            new SniperStrategy(),
            new RifleStrategy()
        };
        currentWeapon = weapons[0];
    }

    private void Start()
    {
        currentHealth = maxHealth;
        currentAmmo = currentWeapon.MaxAmmo;
        targetCoverNode = leftCoverNode;

        groundY = transform.position.y;

        SetupCameraRig();

        OnHealthChanged?.Invoke(currentHealth);
        OnAmmoChanged?.Invoke(currentAmmo);
        OnStanceChanged?.Invoke(currentStance);
        OnWeaponChanged?.Invoke(currentWeapon.WeaponName);
    }

    private void SetupCameraRig()
    {
        if (playerCamera == null)
        {
            if (Camera.main == null)
            {
                return;
            }
            playerCamera = Camera.main.transform;
        }

        if (playerCamera.parent != transform)
        {
            playerCamera.SetParent(transform, false);
        }

        playerCamera.localPosition = coverCameraLocalPosition;
        playerCamera.localRotation = Quaternion.identity;

        Camera cam = playerCamera.GetComponent<Camera>();
        if (cam != null)
        {
            cam.fieldOfView = coverFOV;
        }
    }

    private void Update()
    {
        if (targetCoverNode != null)
        {
            Vector3 targetPos = new Vector3(targetCoverNode.position.x, groundY, targetCoverNode.position.z);
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * movementSpeed);
        }
        if (currentStance == StanceState.InCover && currentHealth < maxHealth)
        {
            RestoreHealth(healthRegenRate * Time.deltaTime);
        }

        HandleCameraPerspective();
    }

    private void HandleCameraPerspective()
    {
        if (playerCamera == null) return;

        Vector3 targetLocalPos = (currentStance == StanceState.Aiming) ? aimCameraLocalPosition : coverCameraLocalPosition;
        float targetFOV = (currentStance == StanceState.Aiming) ? aimFOV : coverFOV;

        playerCamera.localPosition = Vector3.Lerp(playerCamera.localPosition, targetLocalPos, Time.deltaTime * cameraTransitionSpeed);

        Camera cam = playerCamera.GetComponent<Camera>();
        if (cam != null)
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * cameraTransitionSpeed);
        }

        if (playerRenderer != null)
        {
            playerRenderer.enabled = (currentStance == StanceState.InCover);
        }
    }

    public void MoveLeft()
    {
        if (currentStance == StanceState.InCover) targetCoverNode = leftCoverNode;
    }

    public void MoveRight()
    {
        if (currentStance == StanceState.InCover) targetCoverNode = rightCoverNode;
    }

    public void ToggleStance()
    {
        currentStance = (currentStance == StanceState.InCover) ? StanceState.Aiming : StanceState.InCover;
        OnStanceChanged?.Invoke(currentStance);

        if (currentStance == StanceState.InCover && currentAmmo < currentWeapon.MaxAmmo)
        {
            Reload();
        }
    }

    public void SwitchWeapon()
    {
        if (currentStance != StanceState.InCover) return;

        weaponIndex = (weaponIndex + 1) % weapons.Length;
        currentWeapon = weapons[weaponIndex];
        currentAmmo = currentWeapon.MaxAmmo;

        OnWeaponChanged?.Invoke(currentWeapon.WeaponName);
        OnAmmoChanged?.Invoke(currentAmmo);
    }

    public void FireWeapon()
    {
        if (currentStance == StanceState.InCover) return;

        if (currentAmmo <= 0)
        {
            return;
        }

        if (!currentWeapon.CanFire()) return;

        currentAmmo--;
        OnAmmoChanged?.Invoke(currentAmmo);

        currentWeapon.Fire(this, Camera.main.transform);
    }

    public void TakeDamage(float amount)
    {
        if (currentHealth <= 0) return;
        if (currentStance == StanceState.InCover) return;

        currentHealth = Mathf.Max(0, currentHealth - amount);
        OnHealthChanged?.Invoke(currentHealth);

        OnPlayerDamaged?.Invoke();
        if (Camera.main != null && Camera.main.TryGetComponent(out GyroCameraLook gyroCam))
        {
            gyroCam.TriggerShake(0.2f, 0.4f);
        }

        if (currentHealth <= 0)
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayPlayerDeath();
            OnPlayerDeath?.Invoke();
        }
        else
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayPlayerDamage();
        }
    }

    public void RestoreHealth(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth);
    }

    private void Reload()
    {
        currentAmmo = currentWeapon.MaxAmmo;
        OnAmmoChanged?.Invoke(currentAmmo);
    }

    public float GetCurrentHealth() => currentHealth;
    public int GetCurrentAmmo() => currentAmmo;
}

public interface IWeaponStrategy
{
    void Fire(PlayerController player, Transform firePoint);
    int MaxAmmo { get; }
    bool CanFire();
    string WeaponName { get; }
}

public class RifleStrategy : IWeaponStrategy
{
    public int MaxAmmo => 30;
    public string WeaponName => "AK-47 (Assault)";

    private float fireRate = 0.15f;
    private float nextFireTime = 0f;

    public bool CanFire() => Time.time >= nextFireTime;

    public void Fire(PlayerController player, Transform firePoint)
    {
        nextFireTime = Time.time + fireRate;

        if (AudioManager.Instance != null) AudioManager.Instance.PlayRifleShot();

        if (Camera.main.TryGetComponent(out GyroCameraLook gyroCam))
        {
            gyroCam.ApplyRecoil(1.5f, 0.8f);
        }

        Ray ray = new Ray(firePoint.position, firePoint.forward);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.TryGetComponent(out ShootingTarget target))
            {
                target.TakeDamage(1);
            }
        }
    }
}

public class SniperStrategy : IWeaponStrategy
{
    public int MaxAmmo => 5;
    public string WeaponName => "Precision Sniper";

    private float fireRate = 2.0f;
    private float nextFireTime = 0f;

    public bool CanFire() => Time.time >= nextFireTime;

    public void Fire(PlayerController player, Transform firePoint)
    {
        nextFireTime = Time.time + fireRate;

        if (AudioManager.Instance != null) AudioManager.Instance.PlaySniperShot();

        if (Camera.main.TryGetComponent(out GyroCameraLook gyroCam))
        {
            gyroCam.ApplyRecoil(2.0f, 1.0f);
        }

        Ray ray = new Ray(firePoint.position, firePoint.forward);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.TryGetComponent(out ShootingTarget target))
            {
                target.TakeDamage(5);
            }
        }
    }

}
