using UnityEngine;

public class PlayerCharacter : CharacterBase
{
    public override void Die()
    {
        //기존에 CharacterBase의 Die기능을 수행
        base.Die(); 


        //Player가 죽은거면 게임이 끝난거기 때문에 gameManager의 Endgame함수를 호출함.
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        gameManager.Endgame();
    }
}
