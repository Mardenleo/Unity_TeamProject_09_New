using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class IntroSequence : MonoBehaviour
{
    public float delayTime = 5.0f; // 글자가 보일 시간

    void Start()
    {
        // 지정된 시간이 지나면 LoadNextScene 함수를 실행해라
        StartCoroutine(GoToNextScene());
    }

    IEnumerator GoToNextScene()
    {
        yield return new WaitForSeconds(delayTime);
        SceneManager.LoadScene("90 Minutes story");
    }
}