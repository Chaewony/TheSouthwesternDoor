using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameQuit : MonoBehaviour
{
    public void OnClick()
    {
        Time.timeScale = 1f;

        StartCoroutine(SceneQuit());
        
    }

    IEnumerator SceneQuit()
    {
        yield return new WaitForSeconds(0.5f);
        Application.Quit();
    }
}
