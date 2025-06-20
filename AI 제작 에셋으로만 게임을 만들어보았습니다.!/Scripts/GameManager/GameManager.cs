using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    [SerializeField] private BaseUI StageBaseUI;
    [SerializeField] private List<GameObject> _levelObjects;             // 1~10레벨 오브젝트 최대 10개
    [SerializeField] private List<GroundGimmick> _gimmickTargets;        // 레벨 5, 10일 때 회전속도 증가 대상

    private float _surviveTime;
    private float _nextLevelTime;
    private int _level;
    private bool _isGameover;
    private bool _isClearTriggered;

    void Start()
    {
        _surviveTime = 0f;
        _nextLevelTime = 10f;
        _level = 1;
        _isGameover = false;
        _isClearTriggered = false;


        foreach (var obj in _levelObjects)
        {
            obj.SetActive(false);
        }

        UpdateTimeText(); //초기 UI설정.
    }

    void Update()
    {
        //게임 오버 상태면 GameManager가 할수 있는게 없음.
        if (_isGameover) return;


        //게임 오버가 아닐때면 GameManager 로직이 수행
        LevelManager();
        UpdateTimeText();
    }

    private void LevelManager()
    {
        _surviveTime += Time.deltaTime;

        if (_surviveTime >= _nextLevelTime)
        {
            _level++;
            _nextLevelTime += 10f;

            // 레벨 5 또는 10일 때만: 회전 속도 증가
            if (_level == 5 || _level == 10)
            {
                foreach (var gimmick in _gimmickTargets)
                {
                    gimmick.rotationSpeed += 60f;
                }
            }
            // 항상: 오브젝트 활성화
            else if (_level <= _levelObjects.Count)
            {
                _levelObjects[_level - 1].SetActive(true);
            }

            // 레벨 11일 경우: 클리어 처리
            else if (_level == 11 && !_isClearTriggered)
            {
                foreach (var obj in _levelObjects)
                {
                    Destroy(obj);
                }

                ClearStage();
            }
        }
    }

    private void UpdateTimeText()
    {
        StageBaseUI.UpdateTimeText(_surviveTime, _level);
    }


    public void ClearStage()
    {
        StageBaseUI.ClearText.SetActive(true);
        _isClearTriggered = true;
    }


    /// <summary>
    /// EndGame()은 플레이어가 죽었을때만 수행되는 함수이며 매우 중요한 함수임.
    /// 첫 번째 :  isGameover의 상태를 변경해줘서 게임 오버 상태를 활성화함
    /// 두 번째 : Stage에서 기본적으로 있는 BaseUI에서 해줘야할 로직을 처리함.
    /// 세 번째 : 최대 레벨과 최대 생존시간이 발생했을땐 로컬 저장을 진행함
    /// 네 번째 : 지금까지 최대 생존시간과 최대 레벨을 표시해준다. 
    /// (이번 생존시간과 이번 생존 레벨은 상단에서 확인이 가능해서 별도 표시 x)
    /// </summary>
    public void Endgame() //차후 Character
    {
        _isGameover = true;

        StageBaseUI.GameoverText.SetActive(true);
        StageBaseUI.ControlUI.SetActive(false);
        float bestTime = PlayerPrefs.GetFloat("BestTime", 0);
        int bestLevel = PlayerPrefs.GetInt("BestLevel", 1);

        if (_surviveTime > bestTime)
        {
            bestTime = _surviveTime;
            PlayerPrefs.SetFloat("BestTime", bestTime); //베스트 타임 영구 로컬 저장
        }

        if (_level > bestLevel)
        {
            bestLevel = _level;
            PlayerPrefs.SetInt("BestLevel", bestLevel); //베스트 레벨 영구 로컬 저장
        }

        StageBaseUI.EndGameRecordText(bestTime, bestLevel);
    }
}
