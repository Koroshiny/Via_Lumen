using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLightTeleport : MonoBehaviour
{
    enum TeleportState
    {
        Normal,
        Selecting,
        Teleporting
    }

    [Header("References")]
    [SerializeField] Camera thirdPersonCamera;
    [SerializeField] Camera firstPersonTeleportCamera;
    [SerializeField] GameObject playerVisualRoot;
    [SerializeField] PlayerController playerController;

    [Header("Input")]
    [SerializeField] KeyCode teleportModeKey = KeyCode.Q;
    [SerializeField] KeyCode teleportConfirmKey = KeyCode.T;
    [SerializeField] KeyCode nextTargetKey = KeyCode.RightArrow;
    [SerializeField] KeyCode prevTargetKey = KeyCode.LeftArrow;

    [Header("Teleport Flight")]
    [SerializeField] float flightDuration = 1.2f;
    [SerializeField] float flightArcHeight = 2f;

    TeleportState state;
    LightAnchor currentAnchor;
    LightAnchor teleportTarget;

    List<LightAnchor> availableAnchors = new();
    LightAnchor[] allAnchors;

    int selectedIndex;
    bool teleportLock;

    void Awake()
    {
        allAnchors = FindObjectsOfType<LightAnchor>();
    }

    void Update()
    {
        if (state == TeleportState.Teleporting)
            return;

        UpdateCurrentAnchor();

        if (Input.GetKeyDown(KeyCode.E))
            TryLightAnchor();

        if (Input.GetKeyDown(teleportModeKey) && !teleportLock)
        {
            if (state == TeleportState.Normal)
                EnterTeleportMode();
            else if (state == TeleportState.Selecting)
                ExitTeleportModeCancel();
        }

        if (state == TeleportState.Selecting)
            HandleSelectionInput();
    }

    // ----------------------------------------------------

    void UpdateCurrentAnchor()
    {
        currentAnchor = null;
        foreach (var la in allAnchors)
        {
            if (la.IsLit && la.PlayerInside)
            {
                currentAnchor = la;
                break;
            }
        }
    }

    void TryLightAnchor()
    {
        foreach (var la in allAnchors)
        {
            if (la.PlayerInside && !la.IsLit)
            {
                la.LightUp();
                break;
            }
        }
    }

    // ----------------------------------------------------
    // ENTER / EXIT
    // ----------------------------------------------------

    void EnterTeleportMode()
    {
        if (currentAnchor == null)
            return;

        state = TeleportState.Selecting;

        playerController.SetMovementEnabled(false);
        playerVisualRoot.SetActive(false);

        thirdPersonCamera.gameObject.SetActive(false);
        firstPersonTeleportCamera.gameObject.SetActive(true);

        firstPersonTeleportCamera.transform.SetPositionAndRotation(
            currentAnchor.ViewPoint.position,
            currentAnchor.ViewPoint.rotation
        );

        CollectAvailableAnchors();
        SelectInitialAnchor();
    }

    void ExitTeleportModeCancel()
    {
        state = TeleportState.Normal;

        playerVisualRoot.SetActive(true);
        playerController.SetMovementEnabled(true);

        thirdPersonCamera.gameObject.SetActive(true);
        firstPersonTeleportCamera.gameObject.SetActive(false);

        ClearSelectionVFX();
        availableAnchors.Clear();
    }

    // ----------------------------------------------------
    // SELECTION
    // ----------------------------------------------------

    void CollectAvailableAnchors()
    {
        availableAnchors.Clear();
        if (currentAnchor == null)
            return;

        Collider zone = currentAnchor.GetSelectionZone();
        if (zone == null)
            return;

        foreach (var la in allAnchors)
        {
            if (!la.IsLit || la == currentAnchor)
                continue;

            Collider col = la.GetComponent<Collider>();
            if (col != null && zone.bounds.Intersects(col.bounds))
                availableAnchors.Add(la);
        }
    }

    void SelectInitialAnchor()
    {
        if (availableAnchors.Count == 0)
            return;

        selectedIndex = 0;
        UpdateSelection();
    }

    void HandleSelectionInput()
    {
        if (availableAnchors.Count == 0)
            return;

        if (Input.GetKeyDown(nextTargetKey))
            selectedIndex = (selectedIndex + 1) % availableAnchors.Count;

        if (Input.GetKeyDown(prevTargetKey))
            selectedIndex = (selectedIndex - 1 + availableAnchors.Count) % availableAnchors.Count;

        UpdateSelection();

        if (Input.GetKeyDown(teleportConfirmKey))
            StartTeleport();
    }

    void UpdateSelection()
    {
        ClearSelectionVFX();

        selectedIndex = Mathf.Clamp(selectedIndex, 0, availableAnchors.Count - 1);
        availableAnchors[selectedIndex].SetSelected(true);
    }

    void ClearSelectionVFX()
    {
        foreach (var la in allAnchors)
            la.SetSelected(false);
    }

    // ----------------------------------------------------
    // TELEPORT
    // ----------------------------------------------------

    void StartTeleport()
    {
        teleportLock = true;
        state = TeleportState.Teleporting;

        teleportTarget = availableAnchors[selectedIndex];

        ClearSelectionVFX();
        StartCoroutine(TeleportFlightCoroutine());
    }

    IEnumerator TeleportFlightCoroutine()
    {
        Transform cam = firstPersonTeleportCamera.transform;

        Vector3 startPos = cam.position;
        Vector3 endPos = teleportTarget.ViewPoint.position;

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / flightDuration;

            Vector3 pos = Vector3.Lerp(startPos, endPos, t);
            pos.y += Mathf.Sin(t * Mathf.PI) * flightArcHeight;

            cam.position = pos;
            cam.rotation = Quaternion.LookRotation((endPos - pos).normalized);

            yield return null;
        }

        CompleteTeleport();
    }

    void CompleteTeleport()
    {
        state = TeleportState.Normal;

        if (teleportTarget != null && teleportTarget.ExitPoint != null)
        {
            transform.SetPositionAndRotation(
                teleportTarget.ExitPoint.position,
                teleportTarget.ExitPoint.rotation
            );
        }

        teleportTarget = null;

        playerVisualRoot.SetActive(true);
        playerController.SetMovementEnabled(true);

        thirdPersonCamera.gameObject.SetActive(true);
        firstPersonTeleportCamera.gameObject.SetActive(false);

        availableAnchors.Clear();
        teleportLock = false;
    }
}
