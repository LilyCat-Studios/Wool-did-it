using UnityEngine;

public class Egg : MonoBehaviour
{
    private EggManager _eggManager;
    private bool _hasBeenTriggered;

    public void Initialize(EggManager eggManager)
    {
        _eggManager = eggManager;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_hasBeenTriggered)
            return;

        if (!other.CompareTag("Player"))
            return;

        if (_eggManager == null)
        {
            Debug.LogError("EggManager reference is missing!");
            return;
        }

        _hasBeenTriggered = true;

        _eggManager.HandleEggStepped(
            transform.position,
            other.transform
        );

        Destroy(gameObject);
    }
}