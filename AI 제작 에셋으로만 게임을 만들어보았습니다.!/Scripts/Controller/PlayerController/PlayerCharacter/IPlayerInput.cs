using UnityEngine;

public interface IPlayerInput
{
    Vector2 GetMoveInput();
    bool IsAttackPressed();
}
