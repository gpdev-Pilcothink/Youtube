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


    private Animator _playerAnimator;//�÷��̾� ĳ���� �ִϸ�����

    void Start() 
    {
        _playerAnimator = GetComponent<Animator>();
        _playerRigidbody = GetComponent<Rigidbody>();
        _playerCharacter = GetComponent<PlayerCharacter>();
#if UNITY_EDITOR
    // �����Ϳ��� ������ PC �Է� ���
    _inputInterface = new PCInput();
    Debug.Log("������ �Է� �б� ����");
#elif UNITY_ANDROID || UNITY_IOS
    Debug.Log("����� �Է� �б� ����");
        _mobileInputImpl = new MobileInput(_mobileJoystick);
        _inputInterface = _mobileInputImpl;

        // ����� ��ư�� �̺�Ʈ ���
        _mobileAttackButton.onClick.AddListener(() =>
        {
            _mobileInputImpl.SetAttackPressed(true);
        });
#else
        _inputInterface = new PCInput();
        Debug.Log("PC �Է� �б� ����");
#endif

    }

    void Update()
    {
        //������� �������� �Է°��� �����Ͽ� ����
        HandlePlayerMove();
        HandlePlayerRotate();
    }

    void HandlePlayerMove()
    {
        Vector2 inputVec = _inputInterface.GetMoveInput(); //������̵� pc�� GetMoveInput�� ������.
        Vector3 velocity = new Vector3(inputVec.x, _playerRigidbody.linearVelocity.y, inputVec.y) * _playerCharacter.MoveSpeed;
        _playerRigidbody.linearVelocity = velocity;

        float animatorMoveAmount = inputVec.magnitude;
        _playerAnimator.SetFloat("Move", animatorMoveAmount);

        if (_inputInterface.IsAttackPressed())
        {
            Debug.Log("����!");
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
