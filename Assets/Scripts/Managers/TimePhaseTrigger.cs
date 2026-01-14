using UnityEngine;

public class TimePhaseTrigger : MonoBehaviour
{
    [SerializeField] DirectionalLightTimeController timeController;
    [SerializeField] int phaseIndex;

    bool triggered;

    void OnTriggerEnter(Collider other)
    {
        if (triggered)
            return;

        if (!other.CompareTag("Player"))
            return;

        triggered = true;
        timeController.GoToPhase(phaseIndex);
    }
}
