using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaseHealth : MonoBehaviour {

    public Slider healthSlider;
    public float baseRadius = 4;

    public float maxHealth = 100;
    public float currentHealth = 100;

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
        healthSlider.value = currentHealth;
    }
}
