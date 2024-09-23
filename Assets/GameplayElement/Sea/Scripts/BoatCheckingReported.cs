using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class BoatCheckingReported : MonoBehaviour
{
    public enum BoatState
    {
        IDLE,
        MOVING,
        FIXING
    }

    [SerializeField] private BoatState state;
    [SerializeField] public CustomEvent _eventFixing_Started;
    [SerializeField] public CustomEvent _eventFixing_Update;
    [SerializeField] public CustomEvent _eventFixing_Finish;
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private List<SeaElement> _allReported = new List<SeaElement>();

    private Vector3 _closestSpawnPoint;
    private Transform _transform;
    private Transform[] _spawnPoints;
    private SeaElement _currentTarget;
    private Coroutine _coroutineFixing;

    public BoatState State => state;

    private void Start()
    {
        _transform = transform;
        state = BoatState.IDLE;
    }

    private void Update()
    {
        if (_currentTarget == null || state != BoatState.MOVING) return;

        // S'arrêter quand arrivé
        if (_agent.remainingDistance != 0 && _agent.remainingDistance <= _agent.stoppingDistance)
        {
            // Go In Fixing State
            // handle Réparer Boat / Buoy
            StartFixing();
        }
    }

    public void ActiveChecking()
    {
        UpdateTarget();

        PositionCloseToReported();

        ReachTarget(_currentTarget.transform.position);

    }

    private void PositionCloseToReported()
    {
        var targetPos = _currentTarget.transform.position;
        _closestSpawnPoint = GetClosestPoint(targetPos);

        _agent.Move(_closestSpawnPoint - _transform.position);
        transform.LookAt(targetPos, Vector3.up);
    }

    private void ReachTarget(Vector3 position)
    {
        state = BoatState.MOVING;

        // Reach Target 
        _agent.SetDestination(position);
    }

    private void StartFixing()
    {
        state = BoatState.FIXING;
        _coroutineFixing = StartCoroutine(Fixing_Coroutine(_currentTarget.GetFixingDuration()));
    }

    private void UpdateTarget()
    {
        // Aller chercher le [0] du dictionnary
        _currentTarget = _allReported[0];
        _currentTarget.InitializeForFixing();
    }

    private void RemoveCurrentReported()
    {
        _allReported.Remove(_currentTarget);
        _currentTarget = null;
    }

    private void HandleGoingBackOrContinue()
    {
        // Check si autre Report
        if (_allReported.Count == 0)
        {
            // Sinon retourner au spawn point
            GoingBack();
            return;
        }

        // Si oui recommencer avec nouveau point
        UpdateTarget();
        ReachTarget(_currentTarget.transform.position);
    }

    private void GoingBack()
    {
        _closestSpawnPoint = GetClosestPoint(transform.position);

        ReachTarget(_closestSpawnPoint);
    }

    public void UpdateList(SeaElement go)
    {
        _allReported.Add(go);
    }

    public void SetSpawnPoints(Transform[] spawnPoints)
    {
        _spawnPoints = spawnPoints;
    }

    private Vector3 GetClosestPoint(Vector3 comparePosition)
    {
        var closestPoint = _spawnPoints[0].position;
        foreach (var item in _spawnPoints)
        {
            if ((comparePosition - item.position).magnitude < (comparePosition - closestPoint).magnitude)
            {
                closestPoint = item.position;
            }
        }
        return closestPoint;
    }

    IEnumerator Fixing_Coroutine(float fixingDuration)
    {
        _eventFixing_Started.Raise();

        float time = 0;

        while (time < fixingDuration)
        {
            _eventFixing_Update.Raise();
            time += Time.deltaTime;
            yield return null;
        }

        // Notify réparation finit
        _eventFixing_Finish.Raise();

        RemoveCurrentReported();
        HandleGoingBackOrContinue();

        _coroutineFixing = null;
    }
}