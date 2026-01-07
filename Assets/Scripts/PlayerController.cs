using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float movementSpeed = 5f;
    [SerializeField] Rigidbody rb;
    [SerializeField] float jumpForce = 7f;

    float currentSpeed;
    Vector3 direction;
    bool isGrounded = true;

    [Header("Teleport")]
    [SerializeField] float teleportArcHeight = 3f;
    [SerializeField] float teleportDuration = 0.6f;
    [SerializeField] float interactionRadius = 2f;

    [Header("Look Settings")]
    [SerializeField] float maxLookDistance = 20f; // Дальность рейкаста (Raycast)
    [SerializeField] float lookRadius = 10f;      // Радиус проверки фонарей для подсветки/телепорта

    LightAnchor currentAnchor;        // ближайший фонарь, к которому игрок привязан
    LightAnchor previousAnchor;       // для телепортации
    LightAnchor highlightedAnchor;    // фонарь, на который игрок смотрит

    bool isTeleporting = false;
    float teleportTimer;
    Vector3 teleportStart;
    Vector3 teleportEnd;

    [Header("Debug")]
    [SerializeField] LightAnchor debugCurrentAnchor;
    [SerializeField] LightAnchor debugPreviousAnchor;
    [SerializeField] float debugMaxLookDistance;

    [Header("References")]
    [SerializeField] Camera playerCamera;
    [SerializeField] LineRenderer lineRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentSpeed = movementSpeed;
        debugMaxLookDistance = maxLookDistance;

        if (lineRenderer != null)
            lineRenderer.positionCount = 0;
    }

    void Update()
    {
        HandleMovement();
        UpdateLookingAnchor();
        UpdateCurrentAnchor();

        if (Input.GetKeyDown(KeyCode.E))
            TrySetCurrentAnchor();

        if (Input.GetKeyDown(KeyCode.Q))
            TryTeleport();

        debugCurrentAnchor = currentAnchor;
        debugPreviousAnchor = previousAnchor;
    }

    void FixedUpdate()
    {
        if (isTeleporting)
        {
            TeleportArc();
            return;
        }

        rb.MovePosition(transform.position + direction * currentSpeed * Time.deltaTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        isGrounded = true;
    }

    void HandleMovement()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        direction = new Vector3(moveHorizontal, 0f, moveVertical);
        direction = transform.TransformDirection(direction);

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    void UpdateLookingAnchor()
    {
        LightAnchor anchorLookingAt = null;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxLookDistance))
        {
            LightAnchor hitAnchor = hit.collider.GetComponentInParent<LightAnchor>();
            if (hitAnchor != null && hitAnchor.IsLit && hitAnchor != currentAnchor)
            {
                anchorLookingAt = hitAnchor;
            }
        }

        // Снимаем подсветку с предыдущего, если смотрим на другой
        if (highlightedAnchor != null && highlightedAnchor != anchorLookingAt)
            highlightedAnchor.SetHighlight(false);

        highlightedAnchor = anchorLookingAt;

        if (highlightedAnchor != null)
            highlightedAnchor.SetHighlight(true);
    }

    void UpdateCurrentAnchor()
    {
        // Находим ближайший зажженный фонарь в радиусе lookRadius
        Collider[] hits = Physics.OverlapSphere(transform.position, lookRadius);
        LightAnchor nearest = null;
        float minDist = float.MaxValue;

        foreach (var hit in hits)
        {
            LightAnchor la = hit.GetComponentInParent<LightAnchor>();
            if (la != null && la.IsLit)
            {
                float dist = Vector3.Distance(transform.position, la.transform.position);
                if (dist < minDist)
                {
                    nearest = la;
                    minDist = dist;
                }
            }
        }

        if (nearest != null)
            currentAnchor = nearest;
    }

    void TrySetCurrentAnchor()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, interactionRadius);

        foreach (var hit in hits)
        {
            LightAnchor anchor = hit.GetComponentInParent<LightAnchor>();
            if (anchor != null && !anchor.IsLit)
            {
                previousAnchor = currentAnchor;
                currentAnchor = anchor;

                anchor.LightUp();
                Debug.Log($"[Player] Anchor lit: {anchor.name}");
                return;
            }
        }
    }

    void TryTeleport()
    {
        if (highlightedAnchor == null)
        {
            Debug.Log("[Player] Teleport failed: not looking at any lit anchor");
            return;
        }

        teleportStart = transform.position;
        teleportEnd = highlightedAnchor.teleportPoint.position;
        teleportTimer = 0f;
        isTeleporting = true;

        previousAnchor = currentAnchor;

        Debug.Log($"[Player] Teleporting from {teleportStart} to {teleportEnd}");
    }

    void TeleportArc()
    {
        teleportTimer += Time.deltaTime;
        float t = teleportTimer / teleportDuration;

        if (t >= 1f)
        {
            rb.MovePosition(teleportEnd);
            isTeleporting = false;
            if (lineRenderer != null)
                lineRenderer.positionCount = 0;
            return;
        }

        Vector3 midPoint = (teleportStart + teleportEnd) / 2f;
        midPoint.y += teleportArcHeight;

        Vector3 a = Vector3.Lerp(teleportStart, midPoint, t);
        Vector3 b = Vector3.Lerp(midPoint, teleportEnd, t);
        Vector3 position = Vector3.Lerp(a, b, t);

        rb.MovePosition(position);

        // Линия дуги
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 3;
            lineRenderer.SetPosition(0, teleportStart);
            lineRenderer.SetPosition(1, midPoint);
            lineRenderer.SetPosition(2, teleportEnd);
        }
    }
}
