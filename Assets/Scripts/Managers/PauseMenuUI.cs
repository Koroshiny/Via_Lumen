using UnityEngine;

public class PauseMenuUI : MonoBehaviour
{
    [SerializeField] GameObject mainPanel;
    [SerializeField] GameObject settingsPanel;

    void Start()
    {
        ShowMain();
    }

    public void ShowMain()
    {
        mainPanel.SetActive(true);
        settingsPanel.SetActive(false);
    }

    public void ShowSettings()
    {
        mainPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }
}
