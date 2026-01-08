using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float movementSpeed = 5f;
    [SerializeField] Rigidbody rb;
    [SerializeField] float jumpForce = 7f;

    Vector3 direction;
    bool isGrounded = true;

    [Header("Teleport")]
    [SerializeField] float teleportArcHeight = 3f;
    [SerializeField] float teleportDuration = 0.6f;
    [SerializeField] float interactionRadius = 2f;

    [Header("Look Settings")]
    [SerializeField] float maxLookDistance = 20f;
    [SerializeField] float lookRadius = 10f;

    LightAnchor currentAnchor;
    LightAnchor previousAnchor;
    LightAnchor highlightedAnchor;

    bool isTeleporting;
    float teleportTimer;
    Vector3 teleportStart;
    Vector3 teleportEnd;

    [Header("Debug")]
    [SerializeField] LightAnchor debugCurrentAnchor;
    [SerializeField] LightAnchor debugPreviousAnchor;

    [Header("References")]
    [SerializeField] Camera playerCamera;
    [SerializeField] LineRenderer lineRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

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

        rb.MovePosition(transform.position + direction * movementSpeed * Time.deltaTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        isGrounded = true;
    }

    // -------------------------
    // Movement
    // -------------------------
    void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        direction = transform.TransformDirection(new Vector3(h, 0f, v));

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    // -------------------------
    // Look logic
    // -------------------------
    void UpdateLookingAnchor()
    {
        LightAnchor newAnchor = null;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxLookDistance))
        {
            LightAnchor la = hit.collider.GetComponentInParent<LightAnchor>();
            if (la != null && la.IsLit && la != currentAnchor)
                newAnchor = la;
        }

        if (highlightedAnchor != null && highlightedAnchor != newAnchor)
            highlightedAnchor.SetHighlight(false);

        highlightedAnchor = newAnchor;

        if (highlightedAnchor != null)
            highlightedAnchor.SetHighlight(true);
    }

    // -------------------------
    // Current anchor
    // -------------------------
    void UpdateCurrentAnchor()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, lookRadius);

        LightAnchor nearest = null;
        float minDist = float.MaxValue;

        foreach (var hit in hits)
        {
            LightAnchor la = hit.GetComponentInParent<LightAnchor>();
            if (la != null && la.IsLit)
            {
                float d = Vector3.Distance(transform.position, la.transform.position);
                if (d < minDist)
                {
                    minDist = d;
                    nearest = la;
                }
            }
        }

        if (nearest != null)
            currentAnchor = nearest;
    }

    // -------------------------
    // Light up
    // -------------------------
    void TrySetCurrentAnchor()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, interactionRadius);

        foreach (var hit in hits)
        {
            LightAnchor la = hit.GetComponentInParent<LightAnchor>();
            if (la != null && !la.IsLit)
            {
                previousAnchor = currentAnchor;
                currentAnchor = la;
                la.LightUp();
                return;
            }
        }
    }

    // -------------------------
    // Teleport
    // -------------------------
    void TryTeleport()
    {
        if (highlightedAnchor == null)
            return;

        teleportStart = transform.position;
        teleportEnd = highlightedAnchor.GetTeleportPoint().position;

        teleportTimer = 0f;
        isTeleporting = true;

        previousAnchor = currentAnchor;
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

        Vector3 mid = (teleportStart + teleportEnd) * 0.5f;
        mid.y += teleportArcHeight;

        Vector3 pos =
            Vector3.Lerp(
                Vector3.Lerp(teleportStart, mid, t),
                Vector3.Lerp(mid, teleportEnd, t),
                t);

        rb.MovePosition(pos);

        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 3;
            lineRenderer.SetPosition(0, teleportStart);
            lineRenderer.SetPosition(1, mid);
            lineRenderer.SetPosition(2, teleportEnd);
        }
    }
}
