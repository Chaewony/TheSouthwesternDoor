using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pause : MonoBehaviour
{
    public static bool GameIsPaused = false;
    public GameObject pauseMenuCanvas;
    //public GameObject TitleBtn;

    public void OnClick()
    {
        if (GameIsPaused)
        {
            Resume();
        }
        else
        {
            OnPause();
        }
    }

    public void Resume()
    {
        pauseMenuCanvas.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;
    }

    public void OnPause()
    {
        pauseMenuCanvas.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;
    }

    /*public void ToSettingMenu()
    {
        Debug.Log("아직 미구현입니다...");
    }

    public void ToMain()
    {
        Debug.Log("아직 미구현입니다...");
        //Time.timeScale = 1f;
        //SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Debug.Log("아직 미구현입니다...");
        Application.Quit();
    }*/
}
