using UnityEngine;
using System.Collections;

public class ShootingTarget : MonoBehaviour
{
    public enum TargetType { Grunt, Elite, Medic }

    [Header("Target Properties")]
    public TargetType type = TargetType.Grunt;
    public float health = 3f;
    public float shootInterval = 2.0f;

    [Header("Visual Feedback Settings")]
    [Tooltip("Can be a SpriteRenderer or MeshRenderer (for 3D Capsules)")]
    public Renderer targetRenderer;
    public Color hitColor = Color.red;
    private Color originalColor;

    public static event System.Action OnTargetDefeated; 

    private PlayerController player;
    private float nextShootTime;
    private bool isDead = false;

    private void Start()
    {
        player = PlayerController.Instance;
        nextShootTime = Time.time + Random.Range(1.0f, shootInterval);

       
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<Renderer>();
        }

        if (targetRenderer != null)
        {
            
            originalColor = targetRenderer.material.color;
        }
    }

    private void Update()
    {
        if (isDead) return;

        // Enemy offense cycle: Attack player when they pop out of cover
        if (Time.time >= nextShootTime)
        {
            ShootAtPlayer();
            nextShootTime = Time.time + shootInterval;
        }
    }

    private void ShootAtPlayer()
    {
        if (PlayerController.Instance != null && PlayerController.Instance.currentStance == PlayerController.StanceState.Aiming)
        {
            // Deal damage based on enemy class
            float damage = (type == TargetType.Elite) ? 15f : 8f;
            PlayerController.Instance.TakeDamage(damage);
            Debug.Log($"{type} fired! Player took {damage} damage.");
        }
    }

    
    public void TakeDamage(float amount)
    {
        if (isDead) return;

        health -= amount;
        StartCoroutine(HitFeedbackRoutine());

        if (health <= 0)
        {
            Die();
        }
    }

    private IEnumerator HitFeedbackRoutine()
    {
        if (targetRenderer != null)
        {
            targetRenderer.material.color = hitColor;
        }

        yield return new WaitForSeconds(0.15f);

        if (targetRenderer != null && !isDead)
        {
            targetRenderer.material.color = originalColor;
        }
    }

    private void Die()
    {
        isDead = true;
        Debug.Log($"{type} defeated!");

        if (AudioManager.Instance != null) AudioManager.Instance.PlayEnemyDeath();

        //Notify Level Manager that a kill happened
        OnTargetDefeated?.Invoke();

       
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        StartCoroutine(CollapseRoutine());

        // Medic Strategy Benefit: Drop a health pack or heal instantly
        if (type == TargetType.Medic && PlayerController.Instance != null)
        {
            PlayerController.Instance.RestoreHealth(25f);
            Debug.Log("Medic down! Restored 25 HP.");
        }

        Destroy(gameObject, 2f);
    }

    private IEnumerator CollapseRoutine()
    {
        float elapsed = 0f;
        float duration = 0.5f;
        Quaternion startRot = transform.rotation;
        // Tumble 90 degrees backward on the Z/X axis
        Quaternion endRot = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z + 90f);

        while (elapsed < duration)
        {
            transform.rotation = Quaternion.Slerp(startRot, endRot, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.rotation = endRot;
    }
}
