using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsUIController : MonoBehaviour
{
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;
    }

    //public void BackToMenu()
    //{
    //    SceneManager.LoadScene("Menu");
    //}
}
