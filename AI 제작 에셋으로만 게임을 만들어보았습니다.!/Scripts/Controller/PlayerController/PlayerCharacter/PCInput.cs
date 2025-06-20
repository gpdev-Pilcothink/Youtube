using UnityEngine;

public class PCInput : IPlayerInput
{
    public Vector2 GetMoveInput()
    {
        return new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    }

    public bool IsAttackPressed()
    {
        return Input.GetKeyDown(KeyCode.Space);
    }
}
