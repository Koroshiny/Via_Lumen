using UnityEngine;
using System.Collections;

public class TheLightController : MonoBehaviour
{
    [Header("Points")]
    [SerializeField] Transform lighthousePoint;
    [SerializeField] Transform mountainPoint;
    [SerializeField] Transform temporaryPoint;

    [Header("Flight")]
    [SerializeField] float delayBeforeFlight = 4f;
    [SerializeField] float flightDuration = 3f;
    [SerializeField] float arcHeight = 5f;

    bool isCollected;
    bool isFlying;

    void Start()
    {
        // стартуем на маяке
        transform.position = lighthousePoint.position;

        StartCoroutine(FlyToMountainAfterDelay());
    }

    IEnumerator FlyToMountainAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeFlight);
        yield return FlyArc(transform.position, mountainPoint.position);
    }

    IEnumerator FlyArc(Vector3 start, Vector3 end)
    {
        isFlying = true;

        float time = 0f;

        while (time < 1f)
        {
            time += Time.deltaTime / flightDuration;

            Vector3 pos = Vector3.Lerp(start, end, time);
            pos.y += Mathf.Sin(time * Mathf.PI) * arcHeight;

            transform.position = pos;
            yield return null;
        }

        transform.position = end;
        isFlying = false;
    }

    // --- ТЕЛЕПОРТ С КЛАДБИЩА ---
    public void TeleportToTemporaryPoint()
    {
        if (isCollected)
            return;

        StopAllCoroutines();
        isFlying = false;

        transform.position = temporaryPoint.position;
    }

    void OnTriggerEnter(Collider other)
    {
        if (isCollected)
            return;

        if (!other.CompareTag("Player"))
            return;

        CollectLight();
    }

    void CollectLight()
    {
        isCollected = true;

        // пока просто выключаем
        gameObject.SetActive(false);

        Debug.Log("LIGHT COLLECTED");

        // 👉 сюда потом:
        // катсцена
        // включение финальной точки
        // зажигание маяка
    }
}
