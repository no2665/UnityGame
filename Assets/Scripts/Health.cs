using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour {

    public Slider healthSlider;

    public float maxHealth = 100;
    public float startHealth = 100;
    public float currentHealth = 100;

    void Start()
    {
        UpdateUI(); 
    }

    private void OnEnable()
    {
        currentHealth = startHealth;
        UpdateUI();
    }

    public void TakeHealth(float amount)
    {
        currentHealth -= amount;
        if ( currentHealth < 0 )
        {
            currentHealth = 0;
        }
        UpdateUI();
    }

    public void AddHealth(float amount)
    {
        currentHealth += amount;
        if ( currentHealth >= maxHealth )
        {
            currentHealth = maxHealth;
        }
        UpdateUI();
    }

    private void UpdateUI()
    {
        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;
    }
}
