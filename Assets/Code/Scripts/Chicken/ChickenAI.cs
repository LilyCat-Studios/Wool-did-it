using UnityEngine;
using UnityEngine.AI;

public class ChickenAI : MonoBehaviour
{
    [Header("References")]
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
    [SerializeField] private float _playerRefreshInterval = 1f;

    public ChickenState CurrentState => _currentState;

    private Transform[] _players;
    private float _roamingTimer;
    private float _playerRefreshTimer;

    private ChickenState _currentState = ChickenState.Idle;

    public enum ChickenState
    {
        Idle,
        Roaming,
        Fleeing,
        Angry
    }

    public void BecomeAngry(Transform target)
    {
        if (!target)
            return;

        if (_currentState == ChickenState.Angry)
        {
            _chickenAttack.SetTarget(target);
            return;
        }

        ChangeState(ChickenState.Angry);
        _chickenAttack.StartAttack(target);
    }

    private void Start()
    {
        RefreshPlayers();
    }

    private void Update()
    {
        UpdatePlayers();

        CheckForPlayer();

        switch (_currentState)
        {
            case ChickenState.Idle:
                HandleIdle();
                break;

            case ChickenState.Roaming:
                HandleRoaming();
                break;

            case ChickenState.Fleeing:
                HandleFleeing();
                break;

            case ChickenState.Angry:
                HandleAngry();
                break;
        }
    }

    private void ChangeState(ChickenState newState)
    {
        if (_currentState == newState)
            return;

        _currentState = newState;

        if (_agent && _agent.isOnNavMesh)
            _agent.ResetPath();
    }

    private void HandleIdle()
    {
        _roamingTimer += Time.deltaTime;

        if (_roamingTimer >= _roamingStartDelay)
            ChangeState(ChickenState.Roaming);
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
        if (_currentState == ChickenState.Angry ||
            _currentState == ChickenState.Idle)
            return;

        Transform closestPlayer = GetClosestPlayer();

        if (!closestPlayer)
            return;

        float distanceToPlayer = Vector3.Distance(
            transform.position,
            closestPlayer.position
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
        Transform closestPlayer = GetClosestPlayer();

        if (!closestPlayer)
            return;

        Vector3 fleeDirection =
            (transform.position - closestPlayer.position).normalized;

        Vector3 targetPosition =
            transform.position + fleeDirection * _fleeDistance;

        if (NavMesh.SamplePosition(
                targetPosition,
                out NavMeshHit hit,
                _sampleRadius,
                NavMesh.AllAreas))
        {
            _agent.SetDestination(hit.position);
        }
        else
        {
            Debug.LogWarning("Could not find a valid flee destination!");
        }
    }

    private void UpdatePlayers()
    {
        _playerRefreshTimer += Time.deltaTime;

        if (_playerRefreshTimer >= _playerRefreshInterval)
        {
            _playerRefreshTimer = 0f;
            RefreshPlayers();
        }
    }

    private void RefreshPlayers()
    {
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");

        _players = new Transform[playerObjects.Length];

        for (int i = 0; i < playerObjects.Length; i++)
        {
            _players[i] = playerObjects[i].transform;
        }
    }

    private Transform GetClosestPlayer()
    {
        Transform closestPlayer = null;
        float closestDistance = Mathf.Infinity;

        if (_players == null)
            return null;

        foreach (Transform player in _players)
        {
            if (!player)
                continue;

            float distance = Vector3.Distance(
                transform.position,
                player.position
            );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPlayer = player;
            }
        }

        return closestPlayer;
    }
}