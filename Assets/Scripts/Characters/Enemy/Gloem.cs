using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Gloem : EnemyController
{
    [Header("Skill")]
    public float kickForce = 25;

    public GameObject rockPrefab;

    public Transform handPos;

    //Animation Event
    public void KickOff()
    {
        //攻击目标是否在自身前方的扇区内，如果不在则不会受到伤害
        if (attackTarget != null && transform.IsFacingTarget(attackTarget.transform))
        {
            var targetStats = attackTarget.GetComponent<CharacterStats>();

            if (targetStats == null)
                return;

            Vector3 direction = attackTarget.transform.position - transform.position;
            direction.Normalize();

            NavMeshAgent targetAgent = targetStats.GetComponent<NavMeshAgent>();
            if (targetAgent != null)
            {
                targetAgent.isStopped = true;
                targetAgent.velocity = direction * kickForce;
            }

            Animator targetAnimator = targetStats.GetComponent<Animator>();
            if (targetAnimator != null)
                targetAnimator.SetTrigger("Dizzy");

            targetStats.TakeDamage(characterStats, targetStats);
        }
    }

    //Animation Event
    public void ThrowRock()
    {
        if (attackTarget != null)
        {
            //生成石头并设置目标
            var rock = Instantiate(rockPrefab, handPos.position, Quaternion.identity);//旋转设置为原本
            rock.GetComponent<Rock>().target = attackTarget;
        }
    }
}
