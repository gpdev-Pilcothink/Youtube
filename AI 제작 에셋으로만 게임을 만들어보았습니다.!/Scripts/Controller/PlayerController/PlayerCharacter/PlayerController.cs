using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
using UnityEngine.Windows;

public class PlayerController : MonoBehaviour
{
    private IPlayerInput _inputInterface;
    private Rigidbody _playerRigidbody;
    private PlayerCharacter _playerCharacter;


    private MobileInput _mobileInputImpl;
    [SerializeField] private Joystick _mobileJoystick;
    [SerializeField] private Button _mobileAttackButton;


    private float _rotateSpeed = 180f;


    private Animator _playerAnimator;//플레이어 캐릭터 애니메이터

    void Start() 
    {
        _playerAnimator = GetComponent<Animator>();
        _playerRigidbody = GetComponent<Rigidbody>();
        _playerCharacter = GetComponent<PlayerCharacter>();
#if UNITY_EDITOR
    // 에디터에서 강제로 PC 입력 사용
    _inputInterface = new PCInput();
    Debug.Log("에디터 입력 분기 들어옴");
#elif UNITY_ANDROID || UNITY_IOS
    Debug.Log("모바일 입력 분기 들어옴");
        _mobileInputImpl = new MobileInput(_mobileJoystick);
        _inputInterface = _mobileInputImpl;

        // 모바일 버튼에 이벤트 등록
        _mobileAttackButton.onClick.AddListener(() =>
        {
            _mobileInputImpl.SetAttackPressed(true);
        });
#else
        _inputInterface = new PCInput();
        Debug.Log("PC 입력 분기 들어옴");
#endif

    }

    void Update()
    {
        //수평축과 수직축의 입력값을 감지하여 저장
        HandlePlayerMove();
        HandlePlayerRotate();
    }

    void HandlePlayerMove()
    {
        Vector2 inputVec = _inputInterface.GetMoveInput(); //모바일이든 pc든 GetMoveInput이 존재함.
        Vector3 velocity = new Vector3(inputVec.x, _playerRigidbody.linearVelocity.y, inputVec.y) * _playerCharacter.MoveSpeed;
        _playerRigidbody.linearVelocity = velocity;

        float animatorMoveAmount = inputVec.magnitude;
        _playerAnimator.SetFloat("Move", animatorMoveAmount);

        if (_inputInterface.IsAttackPressed())
        {
            Debug.Log("공격!");
        }
    }
    void HandlePlayerRotate()
    {
        Vector2 inputVec = _inputInterface.GetMoveInput();
        Vector3 moveDir = new Vector3(inputVec.x, 0, inputVec.y);

        if (moveDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            Quaternion smoothed = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _rotateSpeed);
            transform.rotation = smoothed;
        }
    }

}
