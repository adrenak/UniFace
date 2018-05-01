using UnityEngine;
using UniPrep.Utils;

public class MonitorDemo : MonoBehaviour {
    public GameObject toMonitor;
	
	void Start () {
        toMonitor.AddMonitor().HandleCollisionEnter(collision => {
            Debug.Log("Object Collided with " + collision.collider.name);
        });
	}
}
