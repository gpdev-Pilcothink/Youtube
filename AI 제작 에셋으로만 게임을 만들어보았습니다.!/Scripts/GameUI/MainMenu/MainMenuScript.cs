using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void OnStartButton()
    {
        SceneManager.LoadScene("MainMenu"); // �� �̸��� ��Ȯ�ؾ� �� (��: Main.unity)
    }

    public void OnExitButton()
    {
        Application.Quit(); // �����Ϳ��� ���� �ȵ�. ���忡���� �۵�
        Debug.Log("���� ����"); // �����Ϳ� Ȯ�� �α�
    }
}
