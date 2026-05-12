using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public Transform shotpos;
    public GameObject Bullet;
    public Transform shotpos1;
    public GameObject Bullet1;


    private void Start()
    {

    }


    void Update()
    {


    }

    public void Shoot()
    {
        Instantiate(Bullet, shotpos);
    }
    public void Meteor()
    {
        Instantiate(Bullet1, shotpos1);
    }







}
