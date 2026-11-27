using UnityEngine;

public class ChickenEggSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject _eggPrefab;
    [SerializeField] private Transform _eggSpawnPoint;
    [SerializeField] private EggManager _eggManager;

    [Header("Egg Settings")]
    [SerializeField] private float _eggSpawnInterval = 10f;

    private float _eggSpawnTimer;

    private void Update()
    {
        if (_eggSpawnInterval <= 0f)
            return;

        _eggSpawnTimer += Time.deltaTime;

        if (_eggSpawnTimer >= _eggSpawnInterval)
        {
            _eggSpawnTimer = 0f;
            SpawnEgg();
        }
    }

    private void SpawnEgg()
    {
        if (!_eggPrefab || !_eggSpawnPoint || !_eggManager)
        {
            Debug.LogError($"Missing reference on ChickenEggSpawner: {gameObject.name}");
            return;
        }

        GameObject eggObject = Instantiate(
            _eggPrefab,
            _eggSpawnPoint.position,
            _eggSpawnPoint.rotation
        );

        Egg egg = eggObject.GetComponent<Egg>();

        if (!egg)
        {
            Debug.LogError("Egg prefab is missing the Egg component!");
            Destroy(eggObject);
            return;
        }

        egg.Initialize(_eggManager);
        _eggManager.RegisterEgg();
    }
}