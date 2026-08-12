using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyStates { GUARD, PATROL, CHASE, DEAD }

// 若没有NavMeshAgent组件，则自动添加一个
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    private EnemyStates enemyStates;

    private NavMeshAgent agent;

    [Header("Basic Settings")]
    public float sightRadius;

    private GameObject attackTarget;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        SwitchStates();
    }

    void SwitchStates()
    {
        //如果发现Player，则切换到CHASE(追击)状态
        if (FoundPlayer())
        {
            enemyStates = EnemyStates.CHASE;
            Debug.Log("找到Player");
        }

        switch (enemyStates)
        {
            case EnemyStates.GUARD:
                break;
            case EnemyStates.PATROL:
                break;
            case EnemyStates.CHASE:

                break;
            case EnemyStates.DEAD:
                break;
        }
    }

    bool FoundPlayer()
    {
        var colliders = Physics.OverlapSphere(transform.position, sightRadius); //以自身为圆心，sightRadius为半径，获取所有碰撞体
        foreach (var target in colliders)
        {
            if (target.CompareTag("Player"))
            {
                attackTarget = target.gameObject;
                return true;
            }
        }
        attackTarget = null;
        return false;
    }
}
