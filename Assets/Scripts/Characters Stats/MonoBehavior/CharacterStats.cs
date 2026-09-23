using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    //用于更新血条UI的事件
    public event Action<int, int> UpdateHealthBarOnAttack;

    public CharacterData_SO templateData;

    public CharacterData_SO characterData;

    public AttackData_SO attackData;

    [HideInInspector]
    public bool isCritical;

    private void Awake()
    {
        if (templateData != null)
        {
            characterData = Instantiate(templateData);
        }
    }

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
        bool wasAlive = CurrentHealth > 0;
        int damage = Mathf.Max(attacker.CurrentDamage() - defener.CurrentDefence, 0);
        CurrentHealth = Mathf.Max(CurrentHealth - damage, 0);
        Debug.Log(defener.name + " 受到 " + damage + " 点伤害，剩余生命值：" + defener.CurrentHealth);

        if (damage > 0)
        {
            PlayerController playerController = defener.GetComponent<PlayerController>();
            if (playerController != null)
                playerController.InterruptAttack();
        }

        //被暴击则播放受击动画
        if (attacker.isCritical)
        {
            //依据人物类型在控制台通报暴击
            Debug.Log("CRITICAL! " + attacker.name + " 对 " + defener.name + " 造成暴击共" + damage + "点伤害");
            defener.GetComponent<Animator>().SetTrigger("Hit");
        }
        //TODO：Update UI
        UpdateHealthBarOnAttack?.Invoke(CurrentHealth, MaxHealth);
        //TODO：经验update
        if (wasAlive && CurrentHealth <= 0)
        {
            attacker.characterData.UpdateExp(defener.characterData.killPoint);
        }
    }

    //计算固定攻击伤害（石头专用）
    public void TakeDamage(int damage, CharacterStats defener)
    {
        bool wasAlive = CurrentHealth > 0;
        int currentDamage = Mathf.Max(damage - defener.CurrentDefence, 0);

        defener.CurrentHealth = Mathf.Max(CurrentHealth - currentDamage, 0);

        if (currentDamage > 0)
        {
            PlayerController playerController = defener.GetComponent<PlayerController>();
            if (playerController != null)
                playerController.InterruptAttack();
        }

        UpdateHealthBarOnAttack?.Invoke(CurrentHealth, MaxHealth);

        if (wasAlive && CurrentHealth <= 0 && !defener.CompareTag("Player") &&
            GameManager.IsInitialized && GameManager.Instance.playerStats != null)
        {
            GameManager.Instance.playerStats.characterData.UpdateExp(defener.characterData.killPoint);
        }

        Debug.Log(defener.name + " 受到 " + currentDamage + " 点伤害，剩余生命值：" + defener.CurrentHealth);
    }

    // 计算当前伤害
    private int CurrentDamage()
    {
        float coreDamage = UnityEngine.Random.Range(attackData.minDamage, attackData.maxDamage);

        //暴击判断
        if (isCritical)
        {
            coreDamage *= attackData.criticalMultiplier;
        }

        return (int)coreDamage;
    }

    #endregion
}
