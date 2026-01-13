using System.Collections.Generic;
using UnityEngine;

public class PlayerLightTeleport : MonoBehaviour
{
    enum TeleportState { Normal, Selecting }

    [Header("References")]
    [SerializeField] Camera thirdPersonCamera;            // основная камера 3 лица
    [SerializeField] Camera firstPersonTeleportCamera;    // камера FPS для телепорта
    [SerializeField] GameObject playerVisualRoot;         // визуальные меши
    [SerializeField] PlayerController playerController;   // управление персонажем

    [Header("Input")]
    [SerializeField] KeyCode teleportModeKey = KeyCode.Q;
    [SerializeField] KeyCode teleportConfirmKey = KeyCode.T;
    [SerializeField] KeyCode nextTargetKey = KeyCode.RightArrow;
    [SerializeField] KeyCode prevTargetKey = KeyCode.LeftArrow;

    [Header("Debug / State")]
    [SerializeField] TeleportState state;
    [SerializeField] LightAnchor currentAnchor;
    [SerializeField] List<LightAnchor> availableAnchors = new List<LightAnchor>();
    [SerializeField] int selectedIndex;

    void Update()
    {
        UpdateCurrentAnchor();

        // Зажигание фонарей на E
        if (Input.GetKeyDown(KeyCode.E))
            TryLightAnchor();

        // Вход/выход в режим телепорта
        if (Input.GetKeyDown(teleportModeKey))
        {
            if (state == TeleportState.Normal)
                EnterTeleportMode();
            else
                ExitTeleportModeWithoutTeleport();
        }

        // Обработка выбора цели и телепорта
        if (state == TeleportState.Selecting)
            HandleSelectionInput();
    }

    void UpdateCurrentAnchor()
    {
        currentAnchor = null;
        foreach (var la in FindObjectsOfType<LightAnchor>())
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
        foreach (var la in FindObjectsOfType<LightAnchor>())
        {
            if (la.PlayerInside && !la.IsLit)
            {
                la.LightUp();
                break;
            }
        }
    }

    // -------------------------
    // Вход в режим телепорта
    // -------------------------
    void EnterTeleportMode()
    {
        if (playerController == null || playerVisualRoot == null || currentAnchor == null) return;

        state = TeleportState.Selecting;

        // Отключаем движение и визуальные меши
        playerController.SetMovementEnabled(false);
        playerVisualRoot.SetActive(false);

        // Отключаем 3rd person камеру
        if (thirdPersonCamera != null)
        {
            thirdPersonCamera.enabled = false;
            thirdPersonCamera.gameObject.SetActive(false);
        }

        // Включаем FPS камеру и перемещаем только её на AnchorViewPoint
        if (firstPersonTeleportCamera != null && currentAnchor.ViewPoint != null)
        {
            firstPersonTeleportCamera.gameObject.SetActive(true);
            firstPersonTeleportCamera.enabled = true;

            firstPersonTeleportCamera.transform.position = currentAnchor.ViewPoint.position;
            firstPersonTeleportCamera.transform.rotation = currentAnchor.ViewPoint.rotation;
        }

        // Собираем цели через SelectionZone
        CollectAvailableAnchors();
        SelectInitialAnchor();
    }

    // -------------------------
    // Выход без телепорта (Q)
    // -------------------------
    void ExitTeleportModeWithoutTeleport()
    {
        state = TeleportState.Normal;

        // Перемещаем игрока на TeleportPoint текущего фонаря
        if (currentAnchor != null && currentAnchor.GetTeleportPoint() != null)
        {
            transform.position = currentAnchor.GetTeleportPoint().position;
            transform.rotation = currentAnchor.GetTeleportPoint().rotation;
        }

        // Включаем визуальные элементы и движение
        playerVisualRoot.SetActive(true);
        playerController.SetMovementEnabled(true);

        // Включаем 3rd person камеру
        if (thirdPersonCamera != null)
        {
            thirdPersonCamera.gameObject.SetActive(true);
            thirdPersonCamera.enabled = true;
        }

        // Выключаем FPS камеру
        if (firstPersonTeleportCamera != null)
        {
            firstPersonTeleportCamera.enabled = false;
            firstPersonTeleportCamera.gameObject.SetActive(false);
        }

        ClearSelectionVFX();
        availableAnchors.Clear();
    }

    // -------------------------
    // Выход с телепортом (T)
    // -------------------------
    void ExitTeleportMode()
    {
        state = TeleportState.Normal;

        // Телепортируем игрока на ExitPoint выбранного фонаря
        if (availableAnchors.Count > 0 && selectedIndex >= 0 && selectedIndex < availableAnchors.Count)
        {
            var target = availableAnchors[selectedIndex];
            if (target != null && target.ExitPoint != null)
            {
                transform.position = target.ExitPoint.position;
                transform.rotation = target.ExitPoint.rotation;
            }
        }
        else if (currentAnchor != null && currentAnchor.GetTeleportPoint() != null)
        {
            // Если целей нет, возвращаем игрока на ExitPoint текущего фонаря
            transform.position = currentAnchor.GetTeleportPoint().position;
            transform.rotation = currentAnchor.GetTeleportPoint().rotation;
        }

        // Включаем визуальные элементы и движение
        playerVisualRoot.SetActive(true);
        playerController.SetMovementEnabled(true);

        // Включаем 3rd person камеру
        if (thirdPersonCamera != null)
        {
            thirdPersonCamera.gameObject.SetActive(true);
            thirdPersonCamera.enabled = true;
        }

        // Выключаем FPS камеру
        if (firstPersonTeleportCamera != null)
        {
            firstPersonTeleportCamera.enabled = false;
            firstPersonTeleportCamera.gameObject.SetActive(false);
        }

        ClearSelectionVFX();
        availableAnchors.Clear();
    }

    // -------------------------
    // Сбор доступных целей
    // -------------------------
    void CollectAvailableAnchors()
    {
        availableAnchors.Clear();
        if (currentAnchor == null) return;

        Collider selectionZone = currentAnchor.GetSelectionZone();
        if (selectionZone == null) return;

        foreach (var la in FindObjectsOfType<LightAnchor>())
        {
            if (!la.IsLit || la == currentAnchor) continue;

            Collider targetCollider = la.GetComponent<Collider>();
            if (targetCollider != null && selectionZone.bounds.Intersects(targetCollider.bounds))
                availableAnchors.Add(la);
        }
    }

    void SelectInitialAnchor()
    {
        selectedIndex = 0;
        UpdateSelection();
    }

    void HandleSelectionInput()
    {
        if (availableAnchors.Count == 0) return;

        if (Input.GetKeyDown(nextTargetKey))
        {
            selectedIndex = (selectedIndex + 1) % availableAnchors.Count;
            UpdateSelection();
        }
        if (Input.GetKeyDown(prevTargetKey))
        {
            selectedIndex = (selectedIndex - 1 + availableAnchors.Count) % availableAnchors.Count;
            UpdateSelection();
        }

        if (Input.GetKeyDown(teleportConfirmKey))
            ExitTeleportMode();
    }

    void UpdateSelection()
    {
        ClearSelectionVFX();
        if (availableAnchors.Count > 0)
            availableAnchors[selectedIndex].SetSelected(true);
    }

    void ClearSelectionVFX()
    {
        foreach (var la in FindObjectsOfType<LightAnchor>())
            la.SetSelected(false);
    }
}
