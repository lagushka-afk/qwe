using System;
using UnityEngine;
using UnityEngine.UI;

public class HealBar : MonoBehaviour
{
    public Image healthbar;
    public float maxHealth = 10f;
    public float HP;
    public int collisionDamage = 1;

    void Start()
    {
     
        HP = maxHealth;
    }
    
    public void TakeDamage(int damage)
    {
        HP -= damage;
        healthbar.fillAmount = HP / maxHealth;
        if (HP <= 0)
        {
            HP = 0;
            Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }

    void Update()
    {
        healthbar.fillAmount = HP / maxHealth;
    }
   
}