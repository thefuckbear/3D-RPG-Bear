using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    public CharacterData_SO characterData;

    public AttackData_SO attackData;

    [HideInInspector]
    public bool isCritical;

    #region Read from Data_SO
    public int MaxHealth
    {
        get { return characterData != null ? characterData.maxHealth : 0; }
        set { if (characterData != null) characterData.maxHealth = value; }
    }

    public int CurrentHealth
    {
        get { return characterData != null ? characterData.currentHealth : 0; }
        set { if (characterData != null) characterData.currentHealth = value; }
    }

    public int BaseDefence 
    {
        get { return characterData != null ? characterData.baseDefence : 0; }
        set { if (characterData != null) characterData.baseDefence = value; }
    }

    public int CurrentDefence
    {
        get { return characterData != null ? characterData.currentDefence : 0; }
        set { if (characterData != null) characterData.currentDefence = value; }
    }
    #endregion

    #region Character Combat
    // 计算实际伤害并扣血
    public void TakeDamage(CharacterStats attacker, CharacterStats defener)
    {
        int damage = Mathf.Max(attacker.CurrentDamage() - defener.CurrentDefence, 0);
        CurrentHealth = Mathf.Max(CurrentHealth - damage, 0);

        //被暴击则播放受击动画
        if (attacker.isCritical)
        {
            defener.GetComponent<Animator>().SetTrigger("Hit");
        }
        //TODO：Update UI
        //TODO：经验update
    }

    // 计算当前伤害
    private int CurrentDamage()
    {
        float coreDamage = UnityEngine.Random.Range(attackData.minDamage, attackData.maxDamage);

        //暴击判断
        if (isCritical)
        {
            coreDamage *= attackData.criticalMultiplier;
            Debug.Log("暴击：" + coreDamage);
        }

        return (int)coreDamage;
    }

    #endregion
}
