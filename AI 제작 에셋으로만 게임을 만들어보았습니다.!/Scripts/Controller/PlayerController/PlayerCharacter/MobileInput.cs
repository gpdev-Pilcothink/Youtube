using UnityEngine;
using UnityEngine.InputSystem;

public class MobileInput : IPlayerInput
{
    private Joystick _joystick;
    private bool _attackPressed;

    public MobileInput(Joystick joystick)
    {
        _joystick = joystick;
    }

    public void SetAttackPressed(bool pressed)
    {
        _attackPressed = pressed;
    }

    public Vector2 GetMoveInput()
    {
        return new Vector2(_joystick.Horizontal, _joystick.Vertical);
    }

    public bool IsAttackPressed()
    {
        bool wasPressed = _attackPressed;
        _attackPressed = false; // ÇÑ ¹ø¸¸ true ¹ÝÈ¯ÇÏ°í ²¨ÁÜ
        return wasPressed;
    }
}
