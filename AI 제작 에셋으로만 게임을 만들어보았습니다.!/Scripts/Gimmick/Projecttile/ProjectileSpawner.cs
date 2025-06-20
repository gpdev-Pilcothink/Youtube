using UnityEngine;

public class ProjectileSpawner : MonoBehaviour 
{
    [SerializeField] private GameObject _projectilePrefab; //������ ����ü ������
    private float _spawnRateMin = 0.5f;
    private float _spawnRateMax = 3f;

    private Transform target; //�߻��� ���
    private float _spawnRate; //���� �ֱ� (_spawnRateMin>=timeAfterSpawn && timeAfterSpawn<=spawnRateMax) 
    private float _timeAfterSpawn; //�ֱ� ���� �������� ���� �ð�

    void Start()
    {
        _timeAfterSpawn = 0f;
        _spawnRate = Random.Range(_spawnRateMin, _spawnRateMax);
        target = FindFirstObjectByType<PlayerCharacter>().transform; //PlayerCharacter�� �÷��̾ ������ �ִ� Ŭ�����̴�.
    }

    void Update()
    {
        _timeAfterSpawn += Time.deltaTime;


        if(_timeAfterSpawn>=_spawnRate)
        {
            _timeAfterSpawn = 0f;

            
            //����ü ����
            GameObject projectile = Instantiate(_projectilePrefab, transform.position, transform.rotation); 
            
            //��ġ����
            Vector3 targetPos = target.position + Vector3.up * 1.0f; // �㸮 �� ���� ����
            projectile.transform.LookAt(targetPos);

            //���� ���� �����ϱ�
            _spawnRate = Random.Range(_spawnRateMin, _spawnRateMax); 
        }
    }



}
