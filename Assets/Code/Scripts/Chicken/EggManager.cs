using UnityEngine;

public class EggManager : MonoBehaviour
{
    [Header("Chicken Settings")]
    [SerializeField] private float _angerRadius = 10f;
    
    public int ActiveEggCount => _activeEggCount;
    public int TotalEggCount => _totalEggCount;
    
    private int _activeEggCount;
    private int _totalEggCount;

    public void RegisterEgg()
    {
        _activeEggCount++;
        _totalEggCount++;

        Debug.Log($"Active eggs: {_activeEggCount} | Total laid: {_totalEggCount}");
    }

    public void HandleEggStepped(Vector3 eggPosition)
    {
        if (_activeEggCount > 0)
            _activeEggCount--;

        Debug.Log($"Active eggs: {_activeEggCount}");

        ChickenAI[] chickens = FindObjectsOfType<ChickenAI>();

        foreach (ChickenAI chicken in chickens)
        {
            float distance = Vector3.Distance(
                eggPosition,
                chicken.transform.position
            );

            if (distance <= _angerRadius)
            {
                chicken.BecomeAngry();
            }
        }
    }
}