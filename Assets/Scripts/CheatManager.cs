using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CheatManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] PlayerController player;
    [SerializeField] Transform startLevelPoint;   // объект, куда телепорт на старт
    [SerializeField] Transform endLevelPoint;     // объект, куда телепорт в конец уровня

    [Header("Light Anchors")]
    [SerializeField] LightAnchor[] allAnchors;   // все фонари на уровне

    [Header("Settings")]
    [SerializeField] string[] levelNames;        // список сцен по порядку
    [SerializeField] bool debugLogs = true;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
            TeleportToStart();

        if (Input.GetKeyDown(KeyCode.F2))
            TeleportToEnd();

        if (Input.GetKeyDown(KeyCode.F3))
            LightUpAllAnchors();

        if (Input.GetKeyDown(KeyCode.F4))
            ExtinguishAllAnchors();

        if (Input.GetKeyDown(KeyCode.F5))
            RestartLevel();

        if (Input.GetKeyDown(KeyCode.F6))
            NextLevel();
    }

    void TeleportToStart()
    {
        if (startLevelPoint != null && player != null)
        {
            player.transform.position = startLevelPoint.position;
            if (debugLogs) Debug.Log("[Cheat] Teleported player to start point");
        }
    }

    void TeleportToEnd()
    {
        if (endLevelPoint != null && player != null)
        {
            player.transform.position = endLevelPoint.position;
            if (debugLogs) Debug.Log("[Cheat] Teleported player to end point");
        }
    }

    void LightUpAllAnchors()
    {
        foreach (var anchor in allAnchors)
        {
            if (anchor != null && !anchor.IsLit)
                anchor.LightUp();
        }
        if (debugLogs) Debug.Log("[Cheat] All anchors lit");
    }

    void ExtinguishAllAnchors()
    {
        foreach (var anchor in allAnchors)
        {
            if (anchor != null && anchor.IsLit)
                anchor.Extinguish();
        }
        if (debugLogs) Debug.Log("[Cheat] All anchors extinguished");
    }

    void RestartLevel()
    {
        if (player != null)
        {
            // Можно сбросить настройки игрока (позиция, телепорт, фонари и т.д.)
            TeleportToStart();
            ExtinguishAllAnchors();
            if (debugLogs) Debug.Log("[Cheat] Level restarted with default settings");
        }
    }

    void NextLevel()
    {
        int currentIndex = -1;
        string currentScene = SceneManager.GetActiveScene().name;

        for (int i = 0; i < levelNames.Length; i++)
        {
            if (levelNames[i] == currentScene)
            {
                currentIndex = i;
                break;
            }
        }

        if (currentIndex != -1)
        {
            int nextIndex = (currentIndex + 1) % levelNames.Length;
            SceneManager.LoadScene(levelNames[nextIndex]);
            if (debugLogs) Debug.Log($"[Cheat] Loaded next level: {levelNames[nextIndex]}");
        }
        else
        {
            if (debugLogs) Debug.LogWarning("[Cheat] Current scene not found in levelNames array");
        }
    }
}
