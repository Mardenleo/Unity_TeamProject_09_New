using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Story : MonoBehaviour
{
 public float delayTime = 4.0f; // 글자가 보일 시간

    void Start()
    {
        GetComponent<AudioSource>().Play();
        // 지정된 시간이 지나면 LoadNextScene 함수를 실행해라
        StartCoroutine(GoToNextScene());
    }

    IEnumerator GoToNextScene()
    {
        yield return new WaitForSeconds(delayTime);
        SceneManager.LoadScene("Main_Menu_Scene");
    }
}
