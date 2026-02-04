using UnityEngine;

public class SlideTrigger : MonoBehaviour
{
    public SlideOnSplineAdvanced slide;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            slide.StartSlide();
        }
    }
}
