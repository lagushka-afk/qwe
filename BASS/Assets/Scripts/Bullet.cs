using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Bullet : MonoBehaviour
{
    public float speed;
    Rigidbody2D rb;
    public HealBar health;
    public int collisionDamage;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = transform.right * speed;
    }

    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        health = collision.gameObject.GetComponent<HealBar>();
        health.TakeDamage(collisionDamage);
        Destroy(gameObject);
    }

    void Update()
    {
        
    }
}
