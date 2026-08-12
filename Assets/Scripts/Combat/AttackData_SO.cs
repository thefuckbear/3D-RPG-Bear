using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Attack", menuName = "Attack/Attack Data")]
public class AttackData_SO : ScriptableObject
{
    public float attackRange; //攻击距离
    public float skillRange; //远程距离
    public float coolDown; //冷却时间
    public int minDamage; //最小伤害
    public int maxDamage; //最大伤害

    public float criticalMultiplier; //暴击伤害倍数
    public float criticalChance; //暴击几率
}
