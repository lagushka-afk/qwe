using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Meteor : MonoBehaviour
{
    public float speed1;
    Rigidbody2D rb1;
    public HealBar health1;
    public int collisionDamage1;
    void Start()
    {
        rb1 = GetComponent<Rigidbody2D>();
        rb1.linearVelocity = transform.right * speed1;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        health1 = collision.gameObject.GetComponent<HealBar>();
        health1.TakeDamage(collisionDamage1);
        Destroy(gameObject);
    }

    void Update()
    {

    }
}
