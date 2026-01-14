using System.Collections.Generic;
using UnityEngine;

public class PlayerLightTeleport : MonoBehaviour
{
    enum TeleportState { Normal, Selecting }

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

    [Header("Debug / State")]
    [SerializeField] TeleportState state;
    [SerializeField] LightAnchor currentAnchor;
    [SerializeField] List<LightAnchor> availableAnchors = new();
    [SerializeField] int selectedIndex;

    LightAnchor[] allAnchors;

    bool teleportJustFinished;


    void Awake()
    {
        allAnchors = FindObjectsOfType<LightAnchor>();
    }

    void Update()
    {
        UpdateCurrentAnchor();

        if (Input.GetKeyDown(KeyCode.E))
            TryLightAnchor();

        if (Input.GetKeyDown(teleportModeKey) && !teleportJustFinished)
        {
            if (state == TeleportState.Normal)
                EnterTeleportMode();
            else
                ExitTeleportModeWithoutTeleport();
        }


        if (state == TeleportState.Selecting)
            HandleSelectionInput();
    }

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

    void EnterTeleportMode()
    {
        if (currentAnchor == null) return;

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

    void ExitTeleportModeWithoutTeleport()
    {
        ExitTeleportInternal(currentAnchor?.GetTeleportPoint());
    }

    void ExitTeleportMode()
    {
        Transform target = availableAnchors.Count > 0
            ? availableAnchors[selectedIndex].ExitPoint
            : currentAnchor?.GetTeleportPoint();

        ExitTeleportInternal(target);
    }

    void ExitTeleportInternal(Transform target)
    {
        state = TeleportState.Normal;

        if (target != null)
            transform.SetPositionAndRotation(target.position, target.rotation);

        playerVisualRoot.SetActive(true);
        playerController.SetMovementEnabled(true);

        thirdPersonCamera.gameObject.SetActive(true);
        firstPersonTeleportCamera.gameObject.SetActive(false);

        ClearSelectionVFX();
        availableAnchors.Clear();

        teleportJustFinished = true;
        Invoke(nameof(ClearTeleportLock), 0.1f);

    }

    void CollectAvailableAnchors()
    {
        availableAnchors.Clear();
        if (currentAnchor == null) return;

        Collider zone = currentAnchor.GetSelectionZone();
        if (zone == null) return;

        foreach (var la in allAnchors)
        {
            if (!la.IsLit || la == currentAnchor) continue;

            if (zone.bounds.Intersects(la.GetComponent<Collider>().bounds))
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
        if (availableAnchors.Count == 0) return;

        if (Input.GetKeyDown(nextTargetKey))
            selectedIndex = (selectedIndex + 1) % availableAnchors.Count;

        if (Input.GetKeyDown(prevTargetKey))
            selectedIndex = (selectedIndex - 1 + availableAnchors.Count) % availableAnchors.Count;

        UpdateSelection();

        if (Input.GetKeyDown(teleportConfirmKey))
            ExitTeleportMode();
    }

    void UpdateSelection()
    {
        ClearSelectionVFX();

        if (availableAnchors.Count == 0)
            return;

        selectedIndex = Mathf.Clamp(selectedIndex, 0, availableAnchors.Count - 1);
        availableAnchors[selectedIndex].SetSelected(true);
    }


    void ClearSelectionVFX()
    {
        if (state == TeleportState.Selecting)
            return;

        foreach (var la in allAnchors)
            la.SetSelected(false);
    }


    void ClearTeleportLock()
    {
        teleportJustFinished = false;
    }
}
