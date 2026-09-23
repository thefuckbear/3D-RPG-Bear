using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyStates { GUARD, PATROL, CHASE, DEAD }

// 若没有NavMeshAgent组件，则自动添加一个
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(CharacterStats))]
public class EnemyController : MonoBehaviour, IEndGameObserve
{
    private EnemyStates enemyStates;

    private NavMeshAgent agent;

    private Animator anim;

    private Collider coll; //获得碰撞体组件，用于在死亡时禁用碰撞体

    protected CharacterStats characterStats;

    [Header("Basic Settings")]
    public float sightRadius;
    
    public bool isGuard;

    private float speed;

    protected GameObject attackTarget;

    public float lookAtTime;
    private float remainLookAtTime;

    private float lastAttackTime; //攻击冷却时间

    private quaternion guardRotation;

    [Header("Patrol State")]
    public float patrolRange;
    private Vector3 wayPoint;
    private Vector3 guardPos;

    //bool配合动画
    bool isWalk;
    bool isChase;
    bool isFollow;
    bool isDeath;
    bool isPlayerDead;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        characterStats = GetComponent<CharacterStats>();
        coll = GetComponent<Collider>();

        speed = agent.speed;
        guardPos = transform.position;
        guardRotation = transform.rotation;
        remainLookAtTime = lookAtTime;
        //characterStats.CurrentHealth = characterStats.MaxHealth;
    }

    void Start()
    {
        //根据isGuard来决定初始状态是守卫还是巡逻
        if (isGuard)
        {
            enemyStates = EnemyStates.GUARD;
        }
        else
        {
            enemyStates = EnemyStates.PATROL;
            GetNewWayPoint();
        }
        GameManager.Instance.AddObserver(this);
    }

    //切换场景时启用
    //void OnEnable()
    //{
    //    GameManager.Instance.AddObserver(this);
    //}

    void OnDisable()
    {
        if (!GameManager.IsInitialized) return;
        GameManager.Instance.RemoveObserver(this);
    }

    void Update()
    {
        if (characterStats.CurrentHealth <= 0)
        {
            isDeath = true;
        }
        if (!isPlayerDead)
        {
            SwitchStates();
            
            lastAttackTime -= Time.deltaTime;
        }
        SwitchAnimation();
    }

    void SwitchAnimation()
    {
        anim.SetBool("Walk", isWalk);
        anim.SetBool("Chase", isChase);
        anim.SetBool("Follow", isFollow);
        anim.SetBool("Critical", characterStats.isCritical);
        anim.SetBool("Death", isDeath);
    }

    void SwitchStates()
    {
        if (isDeath)
        {
            enemyStates = EnemyStates.DEAD;
        }
        //如果发现Player，则切换到CHASE(追击)状态
        else if (FoundPlayer())
        {
            enemyStates = EnemyStates.CHASE;
        }

        switch (enemyStates)
        {
            case EnemyStates.GUARD:
                isChase = false;
                agent.isStopped = false;
                agent.updateRotation = true;

                if (transform.position != guardPos)
                {
                    isWalk = true;
                    agent.destination = guardPos;

                    //sqrMagnitude是向量的平方长度，避免了开方运算，性能更好
                    if (Vector3.SqrMagnitude(guardPos - transform.position) <= agent.stoppingDistance)
                    {
                         isWalk = false;
                         transform.rotation = Quaternion.Lerp(transform.rotation, guardRotation, 0.05f);
                    }
                }
                break;
            case EnemyStates.PATROL:
                isChase = false;
                agent.isStopped = false;
                agent.updateRotation = true;
                agent.speed = speed * 0.5f; //巡逻为原本速度的一半

                //判断是否到达巡逻点，如果到达则获取新的巡逻点，否则继续移动
                if (Vector3.Distance(transform.position, wayPoint) <= agent.stoppingDistance)
                {
                    isWalk = false;
                    //在巡逻点停留一段时间后再获取新的巡逻点
                    if (remainLookAtTime > 0)
                    {
                        remainLookAtTime -= Time.deltaTime;
                        transform.LookAt(wayPoint);
                    }
                    else
                    {
                        GetNewWayPoint();
                    }
                }
                else
                {
                    isWalk = true;
                    agent.destination = wayPoint;
                }

                break;
            case EnemyStates.CHASE:
                isWalk = false;
                isChase = true;

                agent.speed = speed; //追击为原本的速度2.5
                //如果追击时丢失了Player，则回到原来的状态
                if (!FoundPlayer())
                {
                    isFollow = false;
                    agent.isStopped = false;
                    agent.updateRotation = true;
                    if (remainLookAtTime > 0)
                    {
                        agent.destination = transform.position;
                        remainLookAtTime -= Time.deltaTime;          
                    }
                    else
                    {
                        if (isGuard)
                        {
                            enemyStates = EnemyStates.GUARD;
                        }
                        else
                        {
                            enemyStates = EnemyStates.PATROL;
                            GetNewWayPoint();
                        }
                    }
                }
                else
                {
                    isFollow = true;
                    agent.isStopped = false;
                    agent.updateRotation = true;
                    agent.destination = attackTarget.transform.position;
                }
                //在攻击范围内则停止移动并攻击
                if (TargetInAttackRange() || TargetInSkillRange())
                {
                    isFollow = false;
                    agent.ResetPath();
                    agent.isStopped = true;
                    agent.updateRotation = false;
                    FaceAttackTarget();

                    if (lastAttackTime < 0)
                    {
                        lastAttackTime = characterStats.attackData.coolDown;

                        //暴击判断
                        characterStats.isCritical = UnityEngine.Random.value < characterStats.attackData.criticalChance;
                        //攻击
                        Attack();
                    }
                }
                break;
            case EnemyStates.DEAD:
                coll.enabled = false;
                //agent.enabled = false;
                agent.radius = 0; //让死亡后禁用agent的碰撞体，防止阻挡玩家移动
                isChase = false;
                isWalk = false;
                isFollow = false;
                Destroy(gameObject, 2f);
                break;
        }
    }

    void Attack()
    {
        if (attackTarget == null)
        {
            enemyStates = isGuard ? EnemyStates.GUARD : EnemyStates.PATROL;
            return;
        }
        if (TargetInAttackRange())
        {
            //近战攻击
            anim.SetTrigger("Attack");
        }
        else if (TargetInSkillRange())
        {
            // 技能攻击
            anim.SetTrigger("Skill");
        }
    }

    void FaceAttackTarget()
    {
        if (attackTarget == null)
            return;

        Vector3 direction = attackTarget.transform.position - transform.position;
        direction.y = 0;
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                agent.angularSpeed * Time.deltaTime);
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

    bool TargetInAttackRange()
    {
        if (attackTarget != null)
            return Vector3.Distance(transform.position, attackTarget.transform.position) <= characterStats.attackData.attackRange;
        return false;
    }
    bool TargetInSkillRange()
    {
        if (attackTarget != null)
            return Vector3.Distance(transform.position, attackTarget.transform.position) <= characterStats.attackData.skillRange;
        return false;
    }

    void GetNewWayPoint()
    {
        remainLookAtTime = lookAtTime;

        float randomX = UnityEngine.Random.Range(-patrolRange, patrolRange);
        float randomZ = UnityEngine.Random.Range(-patrolRange, patrolRange);

        Vector3 randomPoint = new Vector3(guardPos.x + randomX, transform.position.y, guardPos.z + randomZ);

        //使用NavMesh.SamplePosition来确保随机点在可行走(Walkable)的导航网格上
        NavMeshHit hit;
        wayPoint = NavMesh.SamplePosition(randomPoint, out hit, patrolRange, 1) ? hit.position : transform.position;
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, sightRadius);
    }

    //Animation Event
    void Hit()
    {
        //攻击目标是否在自身前方的扇区内，如果不在则不会受到伤害
        if (attackTarget != null && transform.IsFacingTarget(attackTarget.transform))
        {
            var targetStats = attackTarget.GetComponent<CharacterStats>();
            if (targetStats != null)
            {
                targetStats.TakeDamage(characterStats, targetStats);
            }
        }
    }

    public void EndNotify()
    {
        //获胜动画
        //停止移动
        //停止agent
        anim.SetBool("Win", true);
        isPlayerDead = true;
        isChase = false;
        isWalk = false;
        attackTarget = null;
    }

    
}
