using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class DoorManager : MonoBehaviour
{
    int doorKey = AttributeManager.MAGIC | AttributeManager.INVISIBLE;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnCollisionEnter(Collision collision) {
        int attributes = collision.gameObject.GetComponent<AttributeManager>().attributes;
        Debug.Log(Convert.ToString(attributes, 2));
        Debug.Log(Convert.ToString(doorKey, 2));
        if ((attributes & doorKey) == doorKey) {
            this.GetComponent<BoxCollider>().isTrigger =true;
            Debug.Log("DOOR OPENS");
        } 
    }

    private void OnTriggerExit(Collider other) {
        Debug.Log("DOOR CLOSES");
        this.GetComponent<BoxCollider>().isTrigger = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
