using UnityEngine;

public class Projectile : MonoBehaviour
{
    private float _speed = 8f;
    private float _time = 3f;
    private Rigidbody _rigidbody;
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.linearVelocity = transform.forward * _speed;

        Destroy(gameObject, _time); //��� ������Ʈ�� Time�ʵڿ� �ı�
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            PlayerCharacter playerCharacter = other.GetComponent<PlayerCharacter>(); 
            if (playerCharacter != null)
            {
                playerCharacter.Die();
            }
        }

    }
}


