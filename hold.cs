using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hold : MonoBehaviour {
    bool carying;
    GameObject CarriedGameObject;
    public GameObject grabber;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	    if (Input.GetAxis("drop")==1 & CarriedGameObject != null )
       {
            CarriedGameObject.transform.parent = null;
            Debug.Log("drop");
        }
	}

    void OnTriggerEnter(Collider col)
    { Behaviour script;
        Debug.Log("hit");
        if (col.gameObject.tag == "Pick Up" & !carying)
        { Debug.Log("collected");

            col.transform.position = grabber.transform.position + new Vector3(0f, 0f, 0f);// + col.transform.localScale.magnitude);
            col.transform.rotation = grabber.transform.rotation;
            col.transform.SetParent(grabber.transform);
            script = GetComponent<Behaviour>("Rotator");
            script.enabled = false;
            CarriedGameObject = col.gameObject;
        }
    }
}
