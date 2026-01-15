using UnityEngine;

public class LightTeleportTrigger : MonoBehaviour
{
    [SerializeField] TheLightController lightController;

    bool triggered;

    void OnTriggerEnter(Collider other)
    {
        if (triggered)
            return;

        if (!other.CompareTag("Player"))
            return;

        triggered = true;
        lightController.TeleportToTemporaryPoint();
    }
}
