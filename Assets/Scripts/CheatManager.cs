using UnityEngine;
using UnityEngine.SceneManagement;

public class CheatManager : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] Transform levelStartPoint;
    [SerializeField] Transform levelEndPoint;

    LightAnchor[] anchors;

    void Awake()
    {
        anchors = FindObjectsOfType<LightAnchor>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1)) TeleportTo(levelStartPoint);
        if (Input.GetKeyDown(KeyCode.F2)) TeleportTo(levelEndPoint);
        if (Input.GetKeyDown(KeyCode.F3)) LightAll();
        if (Input.GetKeyDown(KeyCode.F4)) ExtinguishAll();
        if (Input.GetKeyDown(KeyCode.F5)) SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void TeleportTo(Transform target)
    {
        if (target == null) return;
        player.position = target.position;
    }

    void LightAll()
    {
        foreach (var a in anchors) a.LightUp();
    }

    void ExtinguishAll()
    {
        foreach (var a in anchors) a.Extinguish();
    }
}
