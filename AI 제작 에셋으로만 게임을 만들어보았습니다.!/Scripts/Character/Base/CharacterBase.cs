using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

/****************CharacterBase****************
 * ChracterBase�� �� ���ӿ� ������ ��� ĳ������ ���� Ŭ�����Դϴ�.
 * ���� ��� ĳ���Ͱ� ���������� ������ �ִ� �ɷ�ġ���� ���� �� ���� �մϴ�.
 * ��� ĳ���ʹ� ����� �� �ֽ��ϴ�. ���� Die�Լ��� �����ϰ� �ֽ��ϴ�.
*********************************************/

public class CharacterBase : MonoBehaviour
{
    #region Base Stats (ĳ���� �⺻ ����)
    //�ڿ�
    [SerializeField] private bool SurviveStatus;
    [SerializeField] private int _level = 1;
    [SerializeField] private int _maxHP = 100;
    [SerializeField] private int _currentHP;

    [SerializeField] private int _maxMP = 100;
    [SerializeField] private int _currentMP;

    [SerializeField] private int _maxStamina = 100;
    [SerializeField] private int _currentStamina;

    //�ɷ�ġ (base�� ĳ������ �⺻ �����̰�, current�� ��� �� ����� ������ �ջ��� �� �ɷ�ġ�̴�.)
    [SerializeField] private int _baseSTR = 1; // Strength (��) :  attackPower�� �⿩�Ѵ�.
    [SerializeField] private int _bonusSTR = 0;
    [SerializeField] private int _currentSTR;

    [SerializeField] private int _baseDEX = 1;   // Dexterity (��ø) : attackSpeed, magicSpeed, moveSpeed�� �⿩�Ѵ�.
    [SerializeField] private int _bonusDEX = 0;
    [SerializeField] private int _currentDEX;

    [SerializeField] private int _baseINT = 1;   // Intelligence (����) : magicPower�� �⿩�Ѵ�.
    [SerializeField] private int _bonusINT = 0;
    [SerializeField] private int _currentINT;

    [SerializeField] private int _baseVIT = 1;   // Vitality (ü��) : defense, metality�� �⿩�Ѵ�.
    [SerializeField] private int _bonusVIT = 0;
    [SerializeField] private int _currentVIT;

    [SerializeField] private int _baseLUK = 1;   // Luck (��) : penetration�� �ٽ������� �⿩�ϰ� ��� �ɷ�ġ�� ���� �ణ �⿩�Ѵ�.
    [SerializeField] private int _bonusLUK = 0;
    [SerializeField] private int _currentLUK;
    #endregion

    #region Derived Stats (ĳ���� �Ļ� �ɷ�ġ)
    //�Ļ� �ɷ�ġ�� ���� Current�ɷ�ġ �θ� ����� �Ǹ� ���������� ��ȣ�ۿ뿡 ���Ǵ� ��ġ�̴�.
    [SerializeField] private float _attackPower; //���ݷ�
    [SerializeField] private float _attackSpeed; //���� �ӵ�
    [SerializeField] private float _magicPower; //���� ���ݷ�
    [SerializeField] private float _magicSpeed; //���� ���� �ӵ�
    [SerializeField] private float _penetration; //�����
    [SerializeField] private float _defense; //����
    [SerializeField] private int _mentality; //���ŷ�
    [SerializeField] private float _moveSpeed; //������ �ӵ�
    [SerializeField] private float _maxMoveSpeed = 10; //��� ĳ������ �ִ� ������ �ӵ�.
    #endregion

    #region Base Stats Property (�ɷ�ġ�� ��ȭ�� �����ɶ� �Ļ��ɷ�ġ�� ���� ���� ��.)
    public int Level { get => _level; set => _level = value; }
    public int MaxHP { get => _maxHP; set { _maxHP = value; } }
    public int MaxMP { get => _maxMP; set { _maxMP = value; } }
    public int MaxStamina { get => _maxStamina; set { _maxStamina = value; } }

    //�⺻ �� ���ʽ� �ɷ�ġ ������Ƽ(current ��� ����.)
    public int BaseSTR { get => _baseSTR; set { _baseSTR = value; RecalculateCurrentStats(); } }
    public int BaseDEX { get => _baseDEX; set { _baseDEX = value; RecalculateCurrentStats(); } }
    public int BaseINT { get => _baseINT; set { _baseINT = value; RecalculateCurrentStats(); } }
    public int BaseVIT { get => _baseVIT; set { _baseVIT = value; RecalculateCurrentStats(); } }
    public int BaseLUK { get => _baseLUK; set { _baseLUK = value; RecalculateCurrentStats(); } }

    public int BonusSTR { get => _bonusSTR; set { _bonusSTR = value; RecalculateCurrentStats(); } }
    public int BonusDEX { get => _bonusDEX; set { _bonusDEX = value; RecalculateCurrentStats(); } }
    public int BonusINT { get => _bonusINT; set { _bonusINT = value; RecalculateCurrentStats(); } }
    public int BonusVIT { get => _bonusVIT; set { _bonusVIT = value; RecalculateCurrentStats(); } }
    public int BonusLUK { get => _bonusLUK; set { _bonusLUK = value; RecalculateCurrentStats(); } }
    #endregion

    #region Current Stats Property (�ɷ�ġ�� ��ȭ�� �����ɶ� �Ļ��ɷ�ġ�� ���� ���� ��.)
    //�ڿ� ������Ƽ
    public int HP { get => _currentHP; set { _currentHP = value; } }
    public int MP { get => _currentMP; set { _currentMP = value; } }
    public int Stamina { get => _currentStamina; set { _currentStamina = value; } }

    //�ɷ�ġ ������Ƽ(�Ļ� �ɷ�ġ ��� ����.)
    public int STR { get => _currentSTR; set { _currentSTR = value; CalculateDerivedStats(); } }
    public int DEX { get => _currentDEX; set { _currentDEX = value; CalculateDerivedStats(); } }
    public int INT { get => _currentINT; set { _currentINT = value; CalculateDerivedStats(); } }
    public int VIT { get => _currentVIT; set { _currentVIT = value; CalculateDerivedStats(); } }
    public int LUK { get => _currentLUK; set { _currentLUK = value; CalculateDerivedStats(); } }
    public float MoveSpeed { get => _moveSpeed; }
    #endregion

    void OnEnable()
    {
        //ĳ���� Ȱ��ȭ�� �� �׻� ĳ���� �ִ� �ɷ�ġ�� �ʱ�ȭ ��.
        InitStats();
    }

    public void InitStats()
    {
        RecalculateCurrentStats();
        CalculateDerivedStats();
    }

    public void RecalculateCurrentStats()
    {
        STR = _baseSTR + _bonusSTR;
        DEX = _baseDEX + _bonusDEX;
        INT = _baseINT + _bonusINT;
        VIT = _baseVIT + _bonusVIT;
        LUK = _baseLUK + _bonusLUK;
    }

    public void CalculateDerivedStats() //�Ļ� �ɷ�ġ ���
    {
        _currentHP = _maxHP;
        _currentMP = _maxMP;
        _currentStamina = _maxStamina;
        //�켱 �Ļ� �ɷ�ġ�� ���� �ۼ��� ������.
        //��� ���� �߰� �ɷ� ��ȭ�� PlayerCharacterŬ�������� �̷���� ������.
        //����(�÷��̾ ���� �����ϰ� �� �ܴ� �ɷ�ġ�θ� �Ļ� �ɷ�ġ�� ������.
        _attackPower = 1f;
        _attackSpeed = 1f;
        _magicPower = 1f;
        _magicSpeed = 1f;
        _moveSpeed = 8f+((float)(_currentDEX-1)*0.1f)+((float)(_currentLUK-1)*0.01f);
        _defense = 1f;
        _mentality = 1;
       
    }

    public virtual void Die() //��� ĳ���ʹ� HP�� 0�̵Ǹ� �״´�.
    {
        SurviveStatus = false; //���� ���¸� �������� �ٲ�
        Animator animator = GetComponent<Animator>();//�ִϸ����� ������
        animator.SetBool("SurviveStatus", SurviveStatus); // �ִϸ��̼� ���� ��ȯ ����


        gameObject.SetActive(false); //������Ʈ �ı�
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        gameManager.Endgame();

    }


}
