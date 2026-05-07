using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleQueue : MonoBehaviour
{
    private List<string> queue = new List<string>();
    private bool isExecuting = false;
    public Transform shotpos;
    public GameObject Bullet;

   
    public void AddAction(string actionName)
    {
        if (!isExecuting)
            queue.Add(actionName);
    }

    public void StartBattle()
    {
        if (!isExecuting && queue.Count > 0)
            StartCoroutine(ExecuteQueue());
    }

    IEnumerator ExecuteQueue()
    {
        isExecuting = true;

        for (int i = 0; i < queue.Count; i++)
        {
            Debug.Log("Выполняю: " + queue[i]);

            
            if (queue[i] == "Атака")
            {
                Instantiate(Bullet, shotpos);
            }
            else if (queue[i] == "Магия")
            {
                
            }

            yield return new WaitForSeconds(1f); 
        }

        queue.Clear(); 
        isExecuting = false;
    }
}