using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
/// <summary>
/// BaseUI는 UI를 업데이트 하는것을 초점을 맞춘다. (로직에 의한 계산이 있어서는 안됨)
/// UI에 들어오는 대부분의 정보는 GameManager에서 로직에 의해 계산되고 관리되어 값 및 참조형태로 전달된다.
/// </summary>


public class BaseUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _timeText; //실시간 타임 및 레벨 추적 UI

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
        //클리어나 게임 오버때 나오는 텍스트는 객체 초기화시 바로 false해줘야한다.
        //차후 EndGameUI 함수에서 다시 활성화할 예정임.
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
        //_timeText 에 실시간으로 레벨과 UpdateTime을 갱신함.
        _timeText.text = $"Time: {(int)surviveTime}  Level: {level}";
    }

    public void EndGameRecordText(float bestTime, int bestLevel)
    {
        _recordText.text = $"Best Time: {(int)bestTime}  , Best Level: {bestLevel}";
    }

}