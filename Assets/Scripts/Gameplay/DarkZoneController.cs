using System.Collections.Generic;
using UnityEngine;

public class DarkZoneController : MonoBehaviour
{
    [Header("Lamp")]
    [SerializeField] LightAnchor controllingLamp;

    [Header("Blockers")]
    [SerializeField] List<Collider> blockerColliders = new();

    [Header("Dark VFX")]
    [SerializeField] List<ParticleSystem> darkParticles = new();

    bool cleared;

    void Update()
    {
        if (cleared)
            return;

        if (controllingLamp != null && controllingLamp.IsLit)
            ClearZone();
    }

    void ClearZone()
    {
        cleared = true;

        // Disable all blockers
        foreach (var col in blockerColliders)
        {
            if (col != null)
                col.enabled = false;
        }

        // Stop emitting particles, let existing ones die
        foreach (var ps in darkParticles)
        {
            if (ps != null)
                ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }
}
