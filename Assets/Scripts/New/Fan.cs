using UnityEngine;

public class Fan : MonoBehaviour
{
    [SerializeField] Vector3 windDirection;
    [SerializeField] Rigidbody rb;
    [SerializeField] GameObject player;

    [SerializeField] private LayerMask obstacleMask; // ƒобавл€ем эту переменную дл€ слоев преп€тствий

    private void OnTriggerStay(Collider other)
    {
        // ѕровер€ем, что игрок в зоне и нет преп€тстви€ между вентил€тором и игроком
        if (IsPlayerInZoneAndVisible())
        {
            rb.AddForce(windDirection, ForceMode.Acceleration);
        }
    }

    private bool IsPlayerInZoneAndVisible()
    {
        if (player == null || rb == null) return false;

        // ѕровер€ем луч от вентил€тора к игроку
        Vector3 directionToPlayer = player.transform.position - transform.position;
        RaycastHit hit;

        // Ѕросаем луч, провер€€ только преп€тстви€ из указанной маски
        bool hasObstacle = Physics.Raycast(
            transform.position,
            directionToPlayer.normalized,
            out hit,
            directionToPlayer.magnitude,
            obstacleMask
        );

        // ≈сли луч попал во что-то - провер€ем, это игрок или нет
        if (hasObstacle)
        {
            // ≈сли луч попал в игрока - ветер действует
            if (hit.collider.gameObject == player)
            {
                return true;
            }
            // ≈сли луч попал в другое преп€тствие - ветер не действует
            return false;
        }

        // ≈сли луч ничего не задел, игрок виден
        return true;
    }

    // ќпционально: визуализаци€ луча в редакторе дл€ отладки
    //private void OnDrawGizmosSelected()
    //{
    //    if (player != null)
    //    {
    //        Gizmos.color = Color.yellow;
    //        Gizmos.DrawLine(transform.position, player.transform.position);
    //    }
    //}
}