using UnityEngine;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;

public class PlayerCombat : MonoBehaviour
{
    public Transform shotpos;
    public GameObject Bullet;

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

    
}
