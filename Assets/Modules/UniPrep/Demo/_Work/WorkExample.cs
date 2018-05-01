using UnityEngine;
using UniPrep.Utils;
using System.Collections;

public class WorkExample : MonoBehaviour {
    void Start () {
        var work1 = new Work<string>(DelayedMessage());
        work1.Begin(result1 => {
            Debug.Log(result1);
        });
    }
	
	IEnumerator DelayedMessage() {
        yield return new WaitForSeconds(Random.value * 10);
        yield return "That's awesome";
    }

    void Update() {
        if (!Input.GetKeyDown(KeyCode.Escape))
            return;
        var work2 = new Work<string>(DelayedMessage());
        work2.Begin(result2 => {
            Debug.Log(result2);
        });
    }
}
