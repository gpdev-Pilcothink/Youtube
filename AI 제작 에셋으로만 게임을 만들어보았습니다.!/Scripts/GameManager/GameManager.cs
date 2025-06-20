using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    [SerializeField] private BaseUI StageBaseUI;
    [SerializeField] private List<GameObject> _levelObjects;             // 1~10���� ������Ʈ �ִ� 10��
    [SerializeField] private List<GroundGimmick> _gimmickTargets;        // ���� 5, 10�� �� ȸ���ӵ� ���� ���

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

        UpdateTimeText(); //�ʱ� UI����.
    }

    void Update()
    {
        //���� ���� ���¸� GameManager�� �Ҽ� �ִ°� ����.
        if (_isGameover) return;


        //���� ������ �ƴҶ��� GameManager ������ ����
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

            // ���� 5 �Ǵ� 10�� ����: ȸ�� �ӵ� ����
            if (_level == 5 || _level == 10)
            {
                foreach (var gimmick in _gimmickTargets)
                {
                    gimmick.rotationSpeed += 60f;
                }
            }
            // �׻�: ������Ʈ Ȱ��ȭ
            else if (_level <= _levelObjects.Count)
            {
                _levelObjects[_level - 1].SetActive(true);
            }

            // ���� 11�� ���: Ŭ���� ó��
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
    /// EndGame()�� �÷��̾ �׾������� ����Ǵ� �Լ��̸� �ſ� �߿��� �Լ���.
    /// ù ��° :  isGameover�� ���¸� �������༭ ���� ���� ���¸� Ȱ��ȭ��
    /// �� ��° : Stage���� �⺻������ �ִ� BaseUI���� ������� ������ ó����.
    /// �� ��° : �ִ� ������ �ִ� �����ð��� �߻������� ���� ������ ������
    /// �� ��° : ���ݱ��� �ִ� �����ð��� �ִ� ������ ǥ�����ش�. 
    /// (�̹� �����ð��� �̹� ���� ������ ��ܿ��� Ȯ���� �����ؼ� ���� ǥ�� x)
    /// </summary>
    public void Endgame() //���� Character
    {
        _isGameover = true;

        StageBaseUI.GameoverText.SetActive(true);
        StageBaseUI.ControlUI.SetActive(false);
        float bestTime = PlayerPrefs.GetFloat("BestTime", 0);
        int bestLevel = PlayerPrefs.GetInt("BestLevel", 1);

        if (_surviveTime > bestTime)
        {
            bestTime = _surviveTime;
            PlayerPrefs.SetFloat("BestTime", bestTime); //����Ʈ Ÿ�� ���� ���� ����
        }

        if (_level > bestLevel)
        {
            bestLevel = _level;
            PlayerPrefs.SetInt("BestLevel", bestLevel); //����Ʈ ���� ���� ���� ����
        }

        StageBaseUI.EndGameRecordText(bestTime, bestLevel);
    }
}
