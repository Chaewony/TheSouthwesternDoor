using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GotoTuto5 : MonoBehaviour
{
    public GameObject scoreManagerObject;
    private ScoreManager scoreManager;
    private void Start()
    {
        scoreManagerObject = GameObject.Find("ScoreManager");
        this.scoreManager = scoreManagerObject.GetComponent<ScoreManager>();
    }
    public void OnClick()
    {
        StartCoroutine(GotoTutorialScene4());
    }

    IEnumerator GotoTutorialScene4()
    {
        yield return new WaitForSeconds(1.0f);
        SceneManager.LoadScene("Tutorial5");
    }
}
