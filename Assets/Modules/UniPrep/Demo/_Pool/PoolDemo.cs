using System;
using UnityEngine;
using UniPrep.Utils;
using System.Collections;

public class PoolDemo : MonoBehaviour {
    GameObjectPool m_Pool;
    public int emisssionRate;
    public GameObject prefabReference;
    public Vector3 maxForce;
    public Transform parent;
    
	void Start () {
        m_Pool = new GameObjectPool(prefabReference);
        var work = new Work(StartStuff());
        work.Begin();
	}

    IEnumerator StartStuff() {
        while (true) {
            for(int i = 0; i < emisssionRate; i++) {
                var newInstance = m_Pool.Get();

                // Set a single parent to batch instances
                newInstance.transform.SetParent(parent);

                // slow down to zero, Set to origin and apply force
                var rigidbody = newInstance.GetComponent<Rigidbody>();
                rigidbody.velocity = Vector3.zero;

                // Move to position
                newInstance.transform.position = transform.position;

                // random force value
                var force = new Vector3(
                    maxForce.x * (UnityEngine.Random.value - .5F),
                    maxForce.y * (UnityEngine.Random.value - .5F),
                    maxForce.z * (UnityEngine.Random.value - .5F)
                );

                // Apply force
                rigidbody.AddForce(force, ForceMode.Impulse);

                // Create a closure to handle recycling when the prefab hits the floor
                Func<int?> freeFunc = () => {
                    // Keep a copy of newInstance so we close over it
                    var instance = newInstance;
                    instance.AddMonitor().HandleTriggerEnter(_ => {
                        if (_.name.Equals("Floor"))
                            m_Pool.Free(instance);
                    });
                    return null;
                };
                freeFunc.Invoke();
            }
            yield return null;
        }
    }
}
