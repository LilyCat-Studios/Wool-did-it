
using UnityEngine;
using UnityEngine.AI;

public class ChickenAttack : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NavMeshAgent _agent;

    [Header("Attack Settings")]
    [SerializeField] private float _attackDuration = 5f;
    [SerializeField] private float _attackRange = 1.5f;
    [SerializeField] private float _peckInterval = 1f;

    public bool IsAttacking => _isAttacking;
    
    private Transform _target;

    private float _attackTimer;
    private float _peckTimer;

    private bool _isAttacking;

    public void SetTarget(Transform target)
    {
        if (!_isAttacking)
            return;

        _target = target;
    }
    
    public void StartAttack(Transform target)
    {
        _target = target;
        _attackTimer = 0f;
        _peckTimer = 0f;
        _isAttacking = true;

        if (_agent != null && _agent.isOnNavMesh)
        {
            _agent.isStopped = false;
            _agent.ResetPath();
        }
    }

    public bool HandleAttack()
    {
        if (!_isAttacking)
            return true;

        _attackTimer += Time.deltaTime;

        if (_attackTimer >= _attackDuration)
        {
            StopAttack();
            return true;
        }

        if (!_target)
        {
            if (_agent && _agent.isOnNavMesh)
                _agent.isStopped = true;

            return false;
        }

        float distanceToTarget = Vector3.Distance(
            transform.position,
            _target.position
        );

        FaceTarget();

        if (distanceToTarget > _attackRange)
        {
            if (_agent && _agent.isOnNavMesh)
            {
                _agent.isStopped = false;
                _agent.SetDestination(_target.position);
            }
        }
        else
        {
            if (_agent && _agent.isOnNavMesh)
            {
                _agent.isStopped = true;
                _agent.ResetPath();
            }

            HandlePecking();
        }

        return false;
    }


    private void StopAttack()
    {
        _isAttacking = false;
        _target = null;

        if (_agent && _agent.isOnNavMesh)
        {
            _agent.isStopped = false;
            _agent.ResetPath();
        }
    }

    private void FaceTarget()
    {
        Vector3 direction = _target.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    private void HandlePecking()
    {
        _peckTimer += Time.deltaTime;

        if (_peckTimer >= _peckInterval)
        {
            _peckTimer = 0f;

            Debug.Log($"{gameObject.name} is pecking the player!");
        }
    }
}
