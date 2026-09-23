using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    public GameObject healthUIPrefab; //血条预制体
    public Transform healthPoint;

    public bool alwaysVisible;

    public float visibleTime;

    private float timeLeft;

    Image healthSlider;
    Transform UIbar; //血条的transform
    Transform cam; //相机

    CharacterStats currentStats;

    void Awake()
    {
        currentStats = GetComponent<CharacterStats>();

        currentStats.UpdateHealthBarOnAttack += UpdateHealthBar;
    }

    //人物生成时生成血条
    void OnEnable()
    {
        if (visibleTime <= 0)
            visibleTime = 3f;

        Camera mainCamera = Camera.main;
        if (mainCamera == null)
            return;

        cam = mainCamera.transform;

        foreach (Canvas canvas in FindObjectsOfType<Canvas>())
        {
            if (canvas.renderMode == RenderMode.WorldSpace)
            {
                //如果是WorldSpace，就实例化血条
                UIbar = Instantiate(healthUIPrefab, canvas.transform).transform;
                healthSlider = UIbar.GetChild(0).GetComponent<Image>(); //拿到血条
                UIbar.gameObject.SetActive(alwaysVisible);
                break;
            }
        }
    }

    private void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        if (UIbar == null)
            return;

        if (currentHealth <= 0)
        {
            Destroy(UIbar.gameObject);
            return;
        }
        UIbar.gameObject.SetActive(true);
        timeLeft = visibleTime;

        float sliderPercent = (float)currentHealth / maxHealth;
        healthSlider.fillAmount = sliderPercent;
    }

    void OnDestroy()
    {
        if (currentStats != null)
            currentStats.UpdateHealthBarOnAttack -= UpdateHealthBar;
    }

    //LateUpdate在Update之后执行，防止相机移动后血条位置不对
    void LateUpdate()
    {
        if (UIbar != null)
        {
            UIbar.position = healthPoint.position;
            UIbar.forward = -cam.forward; //血条朝向相机

            //血条显示时间
            if (timeLeft <= 0 && !alwaysVisible)
            {
                UIbar.gameObject.SetActive(false);
            }
            else
            {
                timeLeft -= Time.deltaTime;
            }
        }
    }
}
