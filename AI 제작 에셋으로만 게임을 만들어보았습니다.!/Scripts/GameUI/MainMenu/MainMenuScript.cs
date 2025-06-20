using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void OnStartButton()
    {
        SceneManager.LoadScene("MainMenu"); // 씬 이름이 정확해야 함 (예: Main.unity)
    }

    public void OnExitButton()
    {
        Application.Quit(); // 에디터에선 종료 안됨. 빌드에서만 작동
        Debug.Log("게임 종료"); // 에디터용 확인 로그
    }
}
