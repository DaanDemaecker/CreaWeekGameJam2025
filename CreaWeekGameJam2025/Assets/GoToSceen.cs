using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToSceen : MonoBehaviour
{
    public void GoToWinScreen(int i)
    {
        StartCoroutine(delay(1,i));
    }

    IEnumerator delay(float amount, int i)
    {
        yield return new WaitForSeconds(amount);
        SceneManager.LoadScene(i);
    }
}
