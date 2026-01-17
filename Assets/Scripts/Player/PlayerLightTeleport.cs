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
    [SerializeField] Rigidbody playerRb;

    [Header("Input")]
    [SerializeField] KeyCode teleportModeKey = KeyCode.Q;
    [SerializeField] KeyCode teleportConfirmKey = KeyCode.T;
    [SerializeField] KeyCode nextTargetKey = KeyCode.RightArrow;
    [SerializeField] KeyCode prevTargetKey = KeyCode.LeftArrow;

    [Header("Teleport Flight")]
    [SerializeField] float flightDuration = 1.2f;
    [SerializeField] float flightArcHeight = 2f;

    [Header("State")]
    [SerializeField] TeleportState state;
    [SerializeField] LightAnchor currentAnchor;
    [SerializeField] List<LightAnchor> availableAnchors = new();
    [SerializeField] int selectedIndex;

    LightAnchor[] allAnchors;
    LightAnchor teleportTarget;
    bool teleportLock;

    void Awake()
    {
        allAnchors = FindObjectsOfType<LightAnchor>();

        if (playerRb == null)
            playerRb = GetComponent<Rigidbody>();
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
    // CURRENT ANCHOR
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
    // ENTER / EXIT TELEPORT MODE
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

        Transform cam = firstPersonTeleportCamera.transform;
        cam.position = currentAnchor.ViewPoint.position;
        cam.rotation = currentAnchor.ViewPoint.rotation;

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
        // Сначала сброс всех подсветок
        foreach (var la in allAnchors)
            la.SetSelected(false);

        // Ограничиваем индекс
        selectedIndex = Mathf.Clamp(selectedIndex, 0, availableAnchors.Count - 1);

        // Включаем подсветку только выбранного
        if (availableAnchors.Count > 0)
            availableAnchors[selectedIndex].SetSelected(true);
    }

    // Убираем ClearSelectionVFX, оно теперь не нужно
    // void ClearSelectionVFX() { ... } - удаляем


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

        // 🔑 КЛЮЧЕВОЕ ИСПРАВЛЕНИЕ:
        // телепорт через Rigidbody, а не transform
        if (teleportTarget != null && teleportTarget.ExitPoint != null)
        {
            playerRb.position = teleportTarget.ExitPoint.position;
            playerRb.rotation = teleportTarget.ExitPoint.rotation;
            playerRb.velocity = Vector3.zero;
            Physics.SyncTransforms();
        }

        ClearSelectionVFX();

        StartCoroutine(TeleportFlightCoroutine(
            currentAnchor.ViewPoint,
            teleportTarget
        ));
    }

    IEnumerator TeleportFlightCoroutine(Transform fromView, LightAnchor target)
    {
        Transform cam = firstPersonTeleportCamera.transform;

        Vector3 startPos = fromView.position;
        Vector3 endPos = target.ViewPoint.position;

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
        teleportTarget = null;

        playerVisualRoot.SetActive(true);
        playerController.SetMovementEnabled(true);

        thirdPersonCamera.gameObject.SetActive(true);
        firstPersonTeleportCamera.gameObject.SetActive(false);

        availableAnchors.Clear();

        Invoke(nameof(ClearTeleportLock), 0.1f);
    }

    void ClearTeleportLock()
    {
        teleportLock = false;
    }
}
