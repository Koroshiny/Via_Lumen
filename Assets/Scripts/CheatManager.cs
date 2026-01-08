using UnityEngine;
using UnityEngine.SceneManagement;

public class CheatManager : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Transform player;

    [Header("Teleport Points")]
    [SerializeField] private Transform levelStartPoint;
    [SerializeField] private Transform levelEndPoint;

    [Header("Cheat Keys")]
    [SerializeField] private KeyCode teleportToStartKey = KeyCode.F1;
    [SerializeField] private KeyCode teleportToEndKey = KeyCode.F2;
    [SerializeField] private KeyCode lightAllKey = KeyCode.F3;
    [SerializeField] private KeyCode extinguishAllKey = KeyCode.F4;
    [SerializeField] private KeyCode restartLevelKey = KeyCode.F5;
    [SerializeField] private KeyCode nextLevelKey = KeyCode.F6;
    [SerializeField] private KeyCode previousLevelKey = KeyCode.F7;

    void Update()
    {
        if (Input.GetKeyDown(teleportToStartKey))
            TeleportTo(levelStartPoint);

        if (Input.GetKeyDown(teleportToEndKey))
            TeleportTo(levelEndPoint);

        if (Input.GetKeyDown(lightAllKey))
            LightAllAnchors();

        if (Input.GetKeyDown(extinguishAllKey))
            ExtinguishAllAnchors();

        if (Input.GetKeyDown(restartLevelKey))
            RestartLevel();

        if (Input.GetKeyDown(nextLevelKey))
            LoadNextLevel();

        if (Input.GetKeyDown(previousLevelKey))
            LoadPreviousLevel();
    }

    // -------------------------
    // Телепорт
    // -------------------------
    void TeleportTo(Transform target)
    {
        if (player == null || target == null) return;

        player.position = target.position;
        Debug.Log($"[Cheat] Teleport to {target.name}");
    }

    // -------------------------
    // Фонари
    // -------------------------
    void LightAllAnchors()
    {
        LightAnchor[] anchors = FindObjectsOfType<LightAnchor>();
        foreach (var a in anchors)
            a.LightUp();

        Debug.Log("[Cheat] All anchors lit");
    }

    void ExtinguishAllAnchors()
    {
        LightAnchor[] anchors = FindObjectsOfType<LightAnchor>();
        foreach (var a in anchors)
            a.Extinguish();

        Debug.Log("[Cheat] All anchors extinguished");
    }

    // -------------------------
    // Уровни
    // -------------------------
    void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void LoadNextLevel()
    {
        int index = SceneManager.GetActiveScene().buildIndex;
        if (index + 1 < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(index + 1);
    }

    void LoadPreviousLevel()
    {
        int index = SceneManager.GetActiveScene().buildIndex;
        if (index - 1 >= 0)
            SceneManager.LoadScene(index - 1);
    }
}
