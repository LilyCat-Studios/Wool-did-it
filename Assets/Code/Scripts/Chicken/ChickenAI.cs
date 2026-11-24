using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class ChickenAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _player;
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private Transform _roamingCenter;

    [Header("Movement")]
    [SerializeField] private float _detectionRange = 5f;
    [SerializeField] private float _fleeDistance = 8f;
    
    [Header("Roaming")]
    [SerializeField] private float _roamingRadius = 8f;
    [SerializeField] private float _sampleRadius = 2f;
    
    private enum ChickenState
    {
        Idle,
        Roaming,
        Fleeing,
        Angry
    }
    
    private ChickenState _currentState = ChickenState.Roaming;

    private void Update()
    {
        switch (_currentState)
        {
            case ChickenState.Roaming:
                HandleRoaming();
                break;

            case ChickenState.Idle:
                break;

            case ChickenState.Fleeing:
                break;

            case ChickenState.Angry:
                break;
        }
    }
    
    private void ChangeState(ChickenState newState)
    {
        _currentState = newState;
    }
    
    private void HandleRoaming()
    {
        if (_agent.pathPending)
            return;

        if (_agent.hasPath &&
            _agent.remainingDistance > _agent.stoppingDistance)
            return;

        SetRandomDestination();
    }
    
    private void SetRandomDestination()
    {
        Vector2 randomPosition = Random.insideUnitCircle * _roamingRadius;

        Vector3 targetPosition = _roamingCenter.position +
                                 new Vector3(randomPosition.x, 0f, randomPosition.y);

        if (NavMesh.SamplePosition(
                targetPosition,
                out NavMeshHit hit,
                _sampleRadius,
                NavMesh.AllAreas))
        {
            _agent.SetDestination(hit.position);
        }
    }
}
