using UnityEngine;
using System.Collections;
using System;

public class PlayerController : MonoBehaviour
{
    // 1. SINGLETON PATTERN
    public static PlayerController Instance { get; private set; }

    // 2. STATE PATTERN (Player Stance States)
    public enum StanceState { InCover, Aiming }
    [Header("Stance State")]
    public StanceState currentStance = StanceState.InCover;

    // 3. STRATEGY PATTERN (Weapons)
    public IWeaponStrategy currentWeapon;
    private int weaponIndex = 0;
    private IWeaponStrategy[] weapons;



    [Header("Player Parameters")]
    public float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    [SerializeField] private int currentAmmo;

    [Header("Cover Movement System")]
    public Transform leftCoverNode;
    public Transform rightCoverNode;
    public float movementSpeed = 8f;
    private Transform targetCoverNode;

    [Header("Camera Perspective Zoom Settings")]
    [Tooltip("Drag your Main Camera here. If left empty, it will auto-detect Camera.main.")]
    public Transform playerCamera;
    public Vector3 coverCameraLocalPosition = new Vector3(0f, 1.5f, -5f); // 3rd Person View
    public Vector3 aimCameraLocalPosition = new Vector3(0f, 1.5f, 0.6f);   // 1st Person POV 
    public float coverFOV = 60f;
    public float aimFOV = 35f; // Zoom in perspective
    public float cameraTransitionSpeed = 12f;

    [Header("Player Visuals")]
    [Tooltip("Drag your Player Capsule's MeshRenderer or SpriteRenderer here to prevent clipping in 1st Person POV.")]
    public Renderer playerRenderer;

    
    
    private float groundY;

    // 4. OBSERVER PATTERN (UI Actions)
    public static event Action<float> OnHealthChanged;
    public static event Action<int> OnAmmoChanged;
    public static event Action<StanceState> OnStanceChanged;
    public static event Action<string> OnWeaponChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Initialize Strategies (Weapons)
        weapons = new IWeaponStrategy[] {
            new RifleStrategy(),
            new SniperStrategy()
        };
        currentWeapon = weapons[0];
    }

    private void Start()
    {
        currentHealth = maxHealth;
        currentAmmo = currentWeapon.MaxAmmo;
        targetCoverNode = leftCoverNode; // Start at left cover

        
        groundY = transform.position.y;

        
        SetupCameraRig();

       
        OnHealthChanged?.Invoke(currentHealth);
        OnAmmoChanged?.Invoke(currentAmmo);
        OnStanceChanged?.Invoke(currentStance);
        OnWeaponChanged?.Invoke(currentWeapon.WeaponName);
    }

    private void SetupCameraRig()
    {
        // Auto-assign camera 
        if (playerCamera == null)
        {
            if (Camera.main == null)
            {
                Debug.LogError("PlayerController: No Camera tagged 'MainCamera' found in the scene. " +
                    "Select your camera and set its Tag to 'MainCamera' in the Inspector.");
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
        if (currentStance != StanceState.InCover) return; // Only switch while safe

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
            Debug.Log("Out of ammo! Return to cover to reload.");
            return;
        }

        currentAmmo--;
        OnAmmoChanged?.Invoke(currentAmmo);

       
        currentWeapon.Fire(this, Camera.main.transform);
    }

    public void TakeDamage(float amount)
    {
        if (currentStance == StanceState.InCover) return; // Safe behind cover

        currentHealth = Mathf.Max(0, currentHealth - amount);
        OnHealthChanged?.Invoke(currentHealth);

        if (currentHealth <= 0)
        {
            Debug.LogError("Player Eliminated! Level Failed.");
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
    string WeaponName { get; }
}

public class RifleStrategy : IWeaponStrategy
{
    public int MaxAmmo => 30;
    public string WeaponName => "AK-47 (Assault)";

    public void Fire(PlayerController player, Transform firePoint)
    {
        // Apply moderate, rapid-fire recoil
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

    public void Fire(PlayerController player, Transform firePoint)
    {
        // Apply heavy, massive recoil for the sniper rifle
        if (Camera.main.TryGetComponent(out GyroCameraLook gyroCam))
        {
            gyroCam.ApplyRecoil(5.0f, 2.0f);
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
