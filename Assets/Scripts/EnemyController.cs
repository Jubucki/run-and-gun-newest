using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    private static readonly int isWalking = Animator.StringToHash("isWalking");
    private static readonly int isAttacking = Animator.StringToHash("Attack");

    [Header("References")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private Transform player;

    [Header("Settings")]
    [SerializeField] private float patrolWaitTime = 0f;
    [SerializeField] private float StopAtDistance = 0.5f;
    [SerializeField] private float wanderRadius = 50f;

    [Header("Chase Settings")]
    [SerializeField] private float chaseRange = 10f;
    [SerializeField] private float loseRange = 15f;

    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private int damageAmount = 10;

    [Header("Audio")]
    [SerializeField] private AudioClip ambientSound;
    [SerializeField] private float ambientVolume = 0.6f;

    private AudioSource _audioSource;
    private NavMeshAgent _agent;
    private Animator _animator;
    private bool isChasing = false;
    private float lastAttackTime = -999f;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();

        // Audio
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
            _audioSource = gameObject.AddComponent<AudioSource>();

        if (ambientSound != null)
        {
            _audioSource.clip = ambientSound;
            _audioSource.loop = true;
            _audioSource.spatialBlend = 1f; // 3D sound
            _audioSource.volume = ambientVolume;
            _audioSource.Play();
        }

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }
    }

    private void Start()
    {
        SetNewDestination();
    }

    private void Update()
    {
        CheckPlayerDistance();

        if (isChasing)
        {
            HandleChaseAndAttack();
        }
        else
        {
            Patrol();
        }

        UpdateAnimatons();
    }

    private void CheckPlayerDistance()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (!isChasing && distance <= chaseRange)
        {
            isChasing = true;
        }
        else if (isChasing && distance > loseRange)
        {
            isChasing = false;
            SetNewDestination();
        }
    }

    private void HandleChaseAndAttack()
    {
        if (_agent == null || !_agent.isOnNavMesh || player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= attackRange)
        {
            _agent.isStopped = true;
            _agent.ResetPath();

            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 5f);
            }

            if (Time.time - lastAttackTime >= attackCooldown)
            {
                lastAttackTime = Time.time;
                _animator.SetTrigger(isAttacking);
                PerformAttack();
            }
        }
        else
        {
            _agent.isStopped = false;
            _agent.SetDestination(player.position);
        }
    }

    private void PerformAttack()
    {
        if (player == null) return;

        PlayerHealth ph = player.GetComponent<PlayerHealth>();
        if (ph != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance <= attackRange + 0.5f)
            {
                ph.TakeDamage(damageAmount);
            }
        }
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
            int randomIndex = UnityEngine.Random.Range(0, patrolPoints.Length);
            targetPoint = patrolPoints[randomIndex].position;

            if (NavMesh.SamplePosition(targetPoint, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
            {
                targetPoint = hit.position;
            }
            foundPoint = true;
        }
        else
        {
            for (int i = 0; i < 5; i++)
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
        if (_animator == null || _animator.runtimeAnimatorController == null) return;

        bool moving = _agent != null && _agent.velocity.sqrMagnitude > 0.01f;
        _animator.SetBool(isWalking, moving);
    }
}