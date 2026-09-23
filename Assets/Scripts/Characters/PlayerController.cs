using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    private NavMeshAgent agent;

    private Animator anim;
    private CharacterStats characterStats;

    private GameObject attackTarget;

    private Collider coll;

    private float lastAttackTime;

    private bool isDeath;

    private bool attackInterrupted;

    private bool deathNotified;

    private float stopDistance;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        characterStats = GetComponent<CharacterStats>();
        
        //characterStats.CurrentHealth = characterStats.MaxHealth;

        stopDistance = agent.stoppingDistance;
    }

    private void OnEnable()
    {
        MouseManager.Instance.OnMouseClicked += MoveToTarget;
        MouseManager.Instance.OnEnemyClicked += EventAttack;
        GameManager.Instance.RegisterPlayer(characterStats);
    }

    void Start()
    {
        SaveManager.Instance.LoadPlayerData();
    }

    void OnDisable()
    {
        if (!MouseManager.IsInitialized)
            return;

        MouseManager.Instance.OnMouseClicked -= MoveToTarget;
        MouseManager.Instance.OnEnemyClicked -= EventAttack;
    }

    void Update()
    {
        isDeath = characterStats.CurrentHealth <= 0;

        if (isDeath && !deathNotified)
        {
            deathNotified = true;
            GameManager.Instance.NotifyObservers();
        }
        SwitchAnimation();

        lastAttackTime -= Time.deltaTime;
    }

    private void SwitchAnimation()
    {
        anim.SetFloat("Speed", agent.velocity.sqrMagnitude);
        anim.SetBool("Death", isDeath);
    }

    public void MoveToTarget(Vector3 target)
    {
        StopAllCoroutines();
        if (isDeath) 
            return;
        agent.stoppingDistance = stopDistance;
        agent.isStopped = false;
        agent.SetDestination(target);
    }

    private void EventAttack(GameObject target)
    {
        if (isDeath)
            return;
        if (target != null)
        {
            StopAllCoroutines();
            attackInterrupted = false;
            attackTarget = target;
            StartCoroutine(MoveToAttackTarget());
        }
    }

    IEnumerator MoveToAttackTarget()
    {
        if (attackTarget == null) yield break;

        agent.isStopped = false;
        agent.stoppingDistance = stopDistance;

        //使用碰撞体表面最近点，而不是目标中心点，避免大型石头的中心位于不可达区域。
        while (attackTarget != null)
        {
            Collider targetCollider = attackTarget.GetComponentInChildren<Collider>();
            Vector3 targetPoint = targetCollider != null
                ? targetCollider.ClosestPoint(transform.position)
                : attackTarget.transform.position;

            targetPoint.y = transform.position.y;
            Vector3 direction = targetPoint - transform.position;

            if (direction.magnitude <= characterStats.attackData.attackRange)
                break;

            agent.SetDestination(targetPoint);
            if (direction.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(direction);

            yield return null;
        }

        if (attackTarget == null || attackInterrupted)
            yield break;

        agent.isStopped = true;
        Vector3 lookDirection = attackTarget.transform.position - transform.position;
        lookDirection.y = 0;
        if (lookDirection.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(lookDirection);
        
        //Attack
        if (lastAttackTime < 0)
        {
            characterStats.isCritical = UnityEngine.Random.value < characterStats.attackData.criticalChance;
            anim.SetBool("Critical", characterStats.isCritical);
            anim.SetTrigger("Attack");
            //重置冷却时间
            lastAttackTime = characterStats.attackData.coolDown;
        }
    }

    //Animation Event
    void Hit()
    {
        if (attackInterrupted || attackTarget == null)
            return;

        //目标如果是石头，则反击石头
        if (attackTarget.CompareTag("Attackable"))
        {
            if (attackTarget.GetComponent<Rock>())
            {
                //可以在空中攻击石头，石头会反击
                attackTarget.GetComponent<Rock>().rockStates = Rock.RockStates.HitEnemy;
                attackTarget.GetComponent<Rigidbody>().velocity = Vector3.one;
                attackTarget.GetComponent<Rigidbody>().AddForce(transform.forward * 20, ForceMode.Impulse);
            }
        }
        else
        {
            if (attackTarget != null)
            {
                var targetStats = attackTarget.GetComponent<CharacterStats>();
                if (targetStats != null && targetStats.CurrentHealth > 0)
                {
                    targetStats.TakeDamage(characterStats, targetStats);
                }
            }
        }
       
    }

    public void InterruptAttack()
    {
        attackInterrupted = true;
        StopAllCoroutines();
        attackTarget = null;
        anim.ResetTrigger("Attack");
    }
}
