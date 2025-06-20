using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

/****************CharacterBase****************
 * ChracterBase는 이 게임에 나오는 모든 캐릭터의 조상 클래스입니다.
 * 따라서 모든 캐릭터가 공통적으로 가지고 있는 능력치들을 유지 및 관리 합니다.
 * 모든 캐릭터는 사망할 수 있습니다. 따라서 Die함수를 포함하고 있습니다.
*********************************************/

public class CharacterBase : MonoBehaviour
{
    #region Base Stats (캐릭터 기본 스텟)
    //자원
    [SerializeField] private bool SurviveStatus;
    [SerializeField] private int _level = 1;
    [SerializeField] private int _maxHP = 100;
    [SerializeField] private int _currentHP;

    [SerializeField] private int _maxMP = 100;
    [SerializeField] private int _currentMP;

    [SerializeField] private int _maxStamina = 100;
    [SerializeField] private int _currentStamina;

    //능력치 (base는 캐릭터의 기본 스탯이고, current는 장비 및 디버프 버프를 합산한 총 능력치이다.)
    [SerializeField] private int _baseSTR = 1; // Strength (힘) :  attackPower에 기여한다.
    [SerializeField] private int _bonusSTR = 0;
    [SerializeField] private int _currentSTR;

    [SerializeField] private int _baseDEX = 1;   // Dexterity (민첩) : attackSpeed, magicSpeed, moveSpeed에 기여한다.
    [SerializeField] private int _bonusDEX = 0;
    [SerializeField] private int _currentDEX;

    [SerializeField] private int _baseINT = 1;   // Intelligence (지능) : magicPower에 기여한다.
    [SerializeField] private int _bonusINT = 0;
    [SerializeField] private int _currentINT;

    [SerializeField] private int _baseVIT = 1;   // Vitality (체력) : defense, metality에 기여한다.
    [SerializeField] private int _bonusVIT = 0;
    [SerializeField] private int _currentVIT;

    [SerializeField] private int _baseLUK = 1;   // Luck (운) : penetration에 핵심적으로 기여하고 모든 능력치에 아주 약간 기여한다.
    [SerializeField] private int _bonusLUK = 0;
    [SerializeField] private int _currentLUK;
    #endregion

    #region Derived Stats (캐릭터 파생 능력치)
    //파생 능력치는 오직 Current능력치 로만 계산이 되며 실질적으로 상호작용에 사용되는 수치이다.
    [SerializeField] private float _attackPower; //공격력
    [SerializeField] private float _attackSpeed; //공격 속도
    [SerializeField] private float _magicPower; //마법 공격력
    [SerializeField] private float _magicSpeed; //마법 시전 속도
    [SerializeField] private float _penetration; //관통력
    [SerializeField] private float _defense; //방어력
    [SerializeField] private int _mentality; //정신력
    [SerializeField] private float _moveSpeed; //움직임 속도
    [SerializeField] private float _maxMoveSpeed = 10; //모든 캐릭터의 최대 움직임 속도.
    #endregion

    #region Base Stats Property (능력치의 변화가 감지될땐 파생능력치가 같이 변경 됨.)
    public int Level { get => _level; set => _level = value; }
    public int MaxHP { get => _maxHP; set { _maxHP = value; } }
    public int MaxMP { get => _maxMP; set { _maxMP = value; } }
    public int MaxStamina { get => _maxStamina; set { _maxStamina = value; } }

    //기본 및 보너스 능력치 프로퍼티(current 계산 포함.)
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

    #region Current Stats Property (능력치의 변화가 감지될땐 파생능력치가 같이 변경 됨.)
    //자원 프로퍼티
    public int HP { get => _currentHP; set { _currentHP = value; } }
    public int MP { get => _currentMP; set { _currentMP = value; } }
    public int Stamina { get => _currentStamina; set { _currentStamina = value; } }

    //능력치 프로퍼티(파생 능력치 계산 포함.)
    public int STR { get => _currentSTR; set { _currentSTR = value; CalculateDerivedStats(); } }
    public int DEX { get => _currentDEX; set { _currentDEX = value; CalculateDerivedStats(); } }
    public int INT { get => _currentINT; set { _currentINT = value; CalculateDerivedStats(); } }
    public int VIT { get => _currentVIT; set { _currentVIT = value; CalculateDerivedStats(); } }
    public int LUK { get => _currentLUK; set { _currentLUK = value; CalculateDerivedStats(); } }
    public float MoveSpeed { get => _moveSpeed; }
    #endregion

    void OnEnable()
    {
        //캐릭이 활성화될 때 항상 캐릭터 최대 능력치로 초기화 됨.
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

    public void CalculateDerivedStats() //파생 능력치 계산
    {
        _currentHP = _maxHP;
        _currentMP = _maxMP;
        _currentStamina = _maxStamina;
        //우선 파생 능력치는 차후 작성할 예정임.
        //장비에 따른 추가 능력 변화는 PlayerCharacter클래스에서 이루어질 예정임.
        //장비는(플레이어만 착용 가능하고 그 외는 능력치로만 파생 능력치를 관리함.
        _attackPower = 1f;
        _attackSpeed = 1f;
        _magicPower = 1f;
        _magicSpeed = 1f;
        _moveSpeed = 8f+((float)(_currentDEX-1)*0.1f)+((float)(_currentLUK-1)*0.01f);
        _defense = 1f;
        _mentality = 1;
       
    }

    public virtual void Die() //모든 캐릭터는 HP가 0이되면 죽는다.
    {
        SurviveStatus = false; //생존 상태를 죽음으로 바꿈
        Animator animator = GetComponent<Animator>();//애니메이터 얻어오고
        animator.SetBool("SurviveStatus", SurviveStatus); // 애니메이션 상태 전환 유도


        gameObject.SetActive(false); //오브젝트 파괴
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        gameManager.Endgame();

    }


}
