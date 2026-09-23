using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Rock : MonoBehaviour
{
    public enum RockStates { HitNothing, HitEnemy, HitPlayer }

    public RockStates rockStates;

    private Rigidbody rb;

    [Header("Basic Settings")]
    public float force;
    public int damage;
    public GameObject target;
    private Vector3 direction;

    public GameObject breakEffect;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = Vector3.one; //为了让FixedUpdate中的判断生效，先给一个初始速度
        rockStates = RockStates.HitPlayer;
        FlyToTarget();
    }

    //FixedUpdate：用于物理更新
    void FixedUpdate()
    {
        if (rb.velocity.sqrMagnitude < 1f)
        {
            rockStates = RockStates.HitNothing;
        }
    }

    public void FlyToTarget()
    {
        //当目标丢失为空时，石头依旧会寻找玩家
        if (target == null)
            target = FindObjectOfType<PlayerController>().gameObject;
        direction = (target.transform.position - transform.position + Vector3.up).normalized;
        rb.AddForce(direction * force, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision other)
    {
        switch (rockStates)
        {
            case RockStates.HitPlayer:
                if (other.gameObject.CompareTag("Player"))
                {
                    other.gameObject.GetComponent<NavMeshAgent>().isStopped = true; //停止移动
                    other.gameObject.GetComponent<NavMeshAgent>().velocity = direction * force; // 施加力

                    other.gameObject.GetComponent<Animator>().SetTrigger("Dizzy"); 
                    other.gameObject.GetComponent<CharacterStats>().TakeDamage(damage, other.gameObject.GetComponent<CharacterStats>());

                    rockStates = RockStates.HitNothing;
                }
                break;
            case RockStates.HitEnemy:
                if (other.gameObject.GetComponent<Gloem>())
                {
                    var otherStats = other.gameObject.GetComponent<CharacterStats>();
                    otherStats.TakeDamage(damage, otherStats);

                    Instantiate(breakEffect, transform.position, Quaternion.identity);
                    Destroy(this.gameObject);
                }
                break;
            case RockStates.HitNothing:
                break;
            default:
                break;
        }
    }
}
