using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMenu : MonoBehaviour
{
    public void OnMenuButton()
    {
        SceneManager.LoadScene("Stage1");
    }
}
