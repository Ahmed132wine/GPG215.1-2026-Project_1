using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }
    public static event System.Action OnLevelWin;

    public CameraCinematic cinematicCamera;
    public TMP_Text objectiveText;
    public GameObject uiCanvas;

    public int targetsToWin = 10;
    private int currentKills = 0;

    private void Awake() => Instance = this;

    private void OnEnable()
    {
        ShootingTarget.OnTargetDefeated += HandleTargetDefeated;
        PlayerController.OnPlayerDeath += HandlePlayerDeath;
    }

    private void OnDisable()
    {
        ShootingTarget.OnTargetDefeated -= HandleTargetDefeated;
        PlayerController.OnPlayerDeath -= HandlePlayerDeath;
    }

    private void Start()
    {
        StartCoroutine(RunLevelSequence());
    }

    private IEnumerator RunLevelSequence()
    {
        UI uiController = FindObjectOfType<UI>();
        if (uiController != null)
        {
            uiController.SetGameplayUIVisible(false);
        }

        objectiveText.gameObject.SetActive(true);
        objectiveText.text = "Neutralize All Targets";

        yield return cinematicCamera.PlayPan();

        objectiveText.gameObject.SetActive(false);

        if (uiController != null)
        {
            uiController.SetGameplayUIVisible(true);
        }
    }

    private void HandleTargetDefeated()
    {
        currentKills++;

        if (currentKills >= targetsToWin)
        {
            StartCoroutine(WinLevelSequence());
        }
    }

    private void HandlePlayerDeath()
    {
        UI uiController = FindObjectOfType<UI>();
        if (uiController != null) uiController.SetGameplayUIVisible(false);

        objectiveText.gameObject.SetActive(true);
        objectiveText.color = Color.red;
        objectiveText.text = "MISSION FAILED";
    }

    private IEnumerator WinLevelSequence()
    {
        UI uiController = FindObjectOfType<UI>();
        if (uiController != null) uiController.SetGameplayUIVisible(false);

        objectiveText.gameObject.SetActive(true);
        objectiveText.color = Color.green;
        objectiveText.text = "MISSION ACCOMPLISHED";

        EnemyFactory factory = FindObjectOfType<EnemyFactory>();
        if (factory != null) factory.gameObject.SetActive(false);

        yield return new WaitForSeconds(3f);

        OnLevelWin?.Invoke();
        Time.timeScale = 0f;
    }
}
