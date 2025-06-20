using UnityEngine;

public class ProjectileSpawner : MonoBehaviour 
{
    [SerializeField] private GameObject _projectilePrefab; //생성할 투사체 프리팹
    private float _spawnRateMin = 0.5f;
    private float _spawnRateMax = 3f;

    private Transform target; //발사할 대상
    private float _spawnRate; //생성 주기 (_spawnRateMin>=timeAfterSpawn && timeAfterSpawn<=spawnRateMax) 
    private float _timeAfterSpawn; //최근 생성 시점에서 지난 시간

    void Start()
    {
        _timeAfterSpawn = 0f;
        _spawnRate = Random.Range(_spawnRateMin, _spawnRateMax);
        target = FindFirstObjectByType<PlayerCharacter>().transform; //PlayerCharacter는 플레이어만 가지고 있는 클래스이다.
    }

    void Update()
    {
        _timeAfterSpawn += Time.deltaTime;


        if(_timeAfterSpawn>=_spawnRate)
        {
            _timeAfterSpawn = 0f;

            
            //투사체 생성
            GameObject projectile = Instantiate(_projectilePrefab, transform.position, transform.rotation); 
            
            //위치조정
            Vector3 targetPos = target.position + Vector3.up * 1.0f; // 허리 → 가슴 높이
            projectile.transform.LookAt(targetPos);

            //스폰 시점 변경하기
            _spawnRate = Random.Range(_spawnRateMin, _spawnRateMax); 
        }
    }



}
