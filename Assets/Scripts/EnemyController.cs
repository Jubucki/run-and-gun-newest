using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    
    private static readonly int isWalking = Animator.StringToHash("isWalking");
    [Header("References")]
    [SerializeField] private Transform[] patrolPoints;
    
    [Header("Settings")]
    [SerializeField] private float patrolWaitTime = 0f;
    [SerializeField] private float StopAtDistance = 0.5f;
    [SerializeField] private float wanderRadius = 50f;
    
    private NavMeshAgent _agent;
    private Animator _animator;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
    }
    
    private void Start()
    {
        SetNewDestination();
    }
    
    private void Update()
    {
        Patrol();
        UpdateAnimatons();
    }

    private void Patrol()
    {
        if (_agent == null || !_agent.isOnNavMesh) return;
        
        if (!_agent.pathPending && _agent.remainingDistance <= StopAtDistance)
        {
            SetNewDestination();
        }
    }

    private void SetNewDestination()
    {
        if (_agent == null || !_agent.isOnNavMesh) return;

        Vector3 targetPoint = Vector3.zero;
        bool foundPoint = false;

        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            // Pick a random patrol point
            int randomIndex = UnityEngine.Random.Range(0, patrolPoints.Length);
            targetPoint = patrolPoints[randomIndex].position;
            
            // Sample position to ensure it's on NavMesh
            if (NavMesh.SamplePosition(targetPoint, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
            {
                targetPoint = hit.position;
            }
            foundPoint = true;
        }
        else
        {
            // Pick a random NavMesh point within wanderRadius
            for (int i = 0; i < 5; i++) // 5 retries
            {
                Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * wanderRadius;
                randomDirection += transform.position;
                
                if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, 5.0f, NavMesh.AllAreas))
                {
                    targetPoint = hit.position;
                    foundPoint = true;
                    break;
                }
            }
        }

        if (foundPoint)
        {
            _agent.SetDestination(targetPoint);
        }
    }

    private void UpdateAnimatons()
    {
        // Force isWalking to true if we are moving towards a target, 
        // even if velocity is 0 because of NavMesh issues.
        bool isWalking = (_agent != null && _agent.hasPath) || (_agent != null && _agent.velocity.sqrMagnitude > 0.01f);
        _animator.SetBool("isWalking", isWalking);
    }
}
