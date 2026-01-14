using UnityEngine;

public class DirectionalLightTimeController : MonoBehaviour
{
    [Header("Lights")]
    [SerializeField] Light sunLight;
    [SerializeField] Light moonLight;

    [Header("Phases (in order)")]
    [SerializeField] DayPhase[] phases;

    int currentPhaseIndex = 0;

    void Start()
    {
        if (phases.Length == 0)
        {
            Debug.LogError("No day phases assigned!");
            return;
        }

        ApplyPhase(phases[0]);
    }

    // вызывается ТОЛЬКО триггерами
    public void GoToPhase(int phaseIndex)
    {
        // защита от дурака
        if (phaseIndex <= currentPhaseIndex)
            return;

        if (phaseIndex >= phases.Length)
            return;

        currentPhaseIndex = phaseIndex;
        ApplyPhase(phases[phaseIndex]);
    }

    void ApplyPhase(DayPhase phase)
    {
        // --- SUN ---
        if (sunLight != null)
        {
            sunLight.transform.rotation = Quaternion.Euler(
                phase.sunRotationX,
                sunLight.transform.rotation.eulerAngles.y,
                0f
            );

            sunLight.intensity = phase.sunIntensity;
            sunLight.color = phase.sunColor;
        }

        // --- MOON ---
        if (moonLight != null)
        {
            moonLight.intensity = phase.moonIntensity;
            moonLight.color = phase.moonColor;
        }
    }
}

[System.Serializable]
public class DayPhase
{
    public string name;

    [Header("Sun")]
    [Range(-90f, 90f)]
    public float sunRotationX;
    [Range(0f, 1.5f)]
    public float sunIntensity;
    public Color sunColor = Color.white; // добавляем цвет

    [Header("Moon")]
    [Range(0f, 0.5f)]
    public float moonIntensity;
    public Color moonColor = Color.blue; // добавляем цвет
}
