using UnityEngine;

public class PlayerCharacter : CharacterBase
{
    public override void Die()
    {
        //������ CharacterBase�� Die����� ����
        base.Die(); 


        //Player�� �����Ÿ� ������ �����ű� ������ gameManager�� Endgame�Լ��� ȣ����.
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        gameManager.Endgame();
    }
}
