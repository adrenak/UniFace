using UnityEngine;
using UniPrep.Utils;

public class GraduallyExample : MonoBehaviour {
    // Disable this to check for GC in the profile (Debug.Log creates garbage)
    public bool debug;

	void Start() {
        // Spread a For loop from x to y over n frames
        int x = 0;
        int y = 100000000;
        int n = 50;
        Gradually.InExactSteps(x, y, n,
            i => {
                float value = i * Random.value;
                Log(value);
            },
            j => { },
            () => {
                Debug.Log("Done 1 after " + Time.frameCount + " frames");
            }
        );

        int counter = 0;
        Gradually.InTimedSteps(x, y, 50,
            i => {
                // Some complex math
                var result = Mathf.Pow((float)i / 99.9f, (i + 1) / 3.3f);
            },
            j => {
                counter++;
                Log("Loop on hold at index [" + j + "]");
            },
            () => {
                Debug.Log("Done 2 in " + counter + " steps.");
            }
        );
    }

    void Log(object msg) {
        if (debug)
            Debug.Log(msg);
    }
}
