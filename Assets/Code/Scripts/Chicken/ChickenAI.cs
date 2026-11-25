using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class ChickenAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _player;
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private Transform _roamingCenter;
    [SerializeField] private Renderer _renderer;

    [Header("Movement")]
    [SerializeField] private float _detectionRange = 5f;
    [SerializeField] private float _fleeDistance = 8f;
    [SerializeField] private float _fleeExitRange = 7f;
    
    [Header("Roaming")]
    [SerializeField] private float _roamingRadius = 8f;
    [SerializeField] private float _sampleRadius = 2f;
    
    [Header("Attack")]
    [SerializeField] private ChickenAttack _chickenAttack;
    
    [Header("Timers")]
    [SerializeField] private float _roamingStartDelay = 5f;

    private float _roamingTimer;
    
    private enum ChickenState
    {
        Idle,
        Roaming,
        Fleeing,
        Angry
    }
    
    private ChickenState _currentState = ChickenState.Idle;

    public void BecomeAngry()
    {
        if (_currentState == ChickenState.Angry)
            return;

        ChangeState(ChickenState.Angry);
        _chickenAttack.StartAttack(_player);
    }
    
    private void Update()
    {
        CheckForPlayer();
        
        switch (_currentState)
        {
            case ChickenState.Idle:
                Color orange = Color.Lerp(Color.white, Color.orange, 0.5f);
                _renderer.material.color = orange;
                HandleIdle();
                break;
            case ChickenState.Roaming:
                Color green = Color.Lerp(Color.white, Color.green, 0.5f);
                _renderer.material.color = green;
                HandleRoaming();
                break;
            case ChickenState.Fleeing:
                Color blue = Color.Lerp(Color.white, Color.blue, 0.5f);
                _renderer.material.color = blue;
                HandleFleeing();
                break;

            case ChickenState.Angry:
                Color red = Color.Lerp(Color.white, Color.red, 0.5f);
                _renderer.material.color = red;
                HandleAngry();
                break;
        }
    }
    
    private void ChangeState(ChickenState newState)
    {
        if (_currentState == newState)
            return;

        _currentState = newState;

        _agent.ResetPath();
    }
    
    private void HandleIdle()
    {
        _roamingTimer += Time.deltaTime;

        if (_roamingTimer >= _roamingStartDelay)
        {
            ChangeState(ChickenState.Roaming);
        }
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

    private void HandleFleeing()
    {
        if (_agent.pathPending)
            return;

        if (_agent.hasPath &&
            _agent.remainingDistance > _agent.stoppingDistance)
            return;

        SetFleeDestination();
    }
    
    private void HandleAngry()
    {
        bool attackFinished = _chickenAttack.HandleAttack();

        if (attackFinished)
        {
            ChangeState(ChickenState.Roaming);
        }
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
    
    private void CheckForPlayer()
    {
        if (!_player)
            return;
        
        if (_currentState == ChickenState.Angry || _currentState == ChickenState.Idle)
            return;

        float distanceToPlayer = Vector3.Distance(
            transform.position,
            _player.position
        );

        if (_currentState != ChickenState.Fleeing &&
            distanceToPlayer <= _detectionRange)
        {
            ChangeState(ChickenState.Fleeing);
        }
        else if (_currentState == ChickenState.Fleeing &&
                 distanceToPlayer >= _fleeExitRange)
        {
            ChangeState(ChickenState.Roaming);
        }
    }

    private void SetFleeDestination()
    {
        Vector3 fleeDirection = (transform.position - _player.position).normalized;

        Vector3 targetPosition = transform.position + fleeDirection * _fleeDistance;

        if (NavMesh.SamplePosition(
                targetPosition,
                out NavMeshHit hit,
                _sampleRadius,
                NavMesh.AllAreas))
        {
            bool destinationSet = _agent.SetDestination(hit.position);

            Debug.Log($"Flee destination set: {destinationSet}");
        }
        else
        {
            Debug.LogWarning("Could not find a valid flee destination!");
        }
    }
}
