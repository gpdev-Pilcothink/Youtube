using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
/// <summary>
/// BaseUI�� UI�� ������Ʈ �ϴ°��� ������ �����. (������ ���� ����� �־�� �ȵ�)
/// UI�� ������ ��κ��� ������ GameManager���� ������ ���� ���ǰ� �����Ǿ� �� �� �������·� ���޵ȴ�.
/// </summary>


public class BaseUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _timeText; //�ǽð� Ÿ�� �� ���� ���� UI

    [SerializeField] private GameObject _gameoverText;
    [SerializeField] private TMP_Text _recordText;

    [SerializeField] private GameObject _clearText;

    [SerializeField] private Button _restartButton;
    [SerializeField] private GameObject _controlUI;
    public TMP_Text TimeText { get => _timeText; set => _timeText = value; }
    public TMP_Text RecordText { get => _recordText; set => _recordText = value; }
    public GameObject GameoverText { get => _gameoverText; set => _gameoverText = value; }
    public GameObject ClearText { get => _clearText; set => _clearText = value; }
    public Button RestartButton { get => _restartButton; set => _restartButton = value; }
    public GameObject ControlUI { get => _controlUI; set => _controlUI = value; }




    void Start()
    {
        //Ŭ��� ���� ������ ������ �ؽ�Ʈ�� ��ü �ʱ�ȭ�� �ٷ� false������Ѵ�.
        //���� EndGameUI �Լ����� �ٽ� Ȱ��ȭ�� ������.
        _clearText.SetActive(false); 
        _gameoverText.SetActive(false);

        _restartButton.onClick.AddListener(OnRestartButtonClicked);

    }

    private void OnRestartButtonClicked()
    {
        SceneManager.LoadScene("Stage1");
    }

    public void UpdateTimeText(float surviveTime, int level)
    {
        //_timeText �� �ǽð����� ������ UpdateTime�� ������.
        _timeText.text = $"Time: {(int)surviveTime}  Level: {level}";
    }

    public void EndGameRecordText(float bestTime, int bestLevel)
    {
        _recordText.text = $"Best Time: {(int)bestTime}  , Best Level: {bestLevel}";
    }

}