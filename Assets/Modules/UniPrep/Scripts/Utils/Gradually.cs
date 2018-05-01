using System;
using UnityEngine;
using System.Collections;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace UniPrep.Utils {
    public class Gradually : MonoBehaviour {
        static Gradually instance;
        public delegate void LoopHandler(int index);

        static Gradually GetInstance() {
            if(instance == null) {
                var go = new GameObject("Gradually");
                DontDestroyOnLoad(go);
                instance = go.AddComponent<Gradually>();
            }
            return instance;
        }

        /// <summary>
        /// Runs a loop in a given number of steps
        /// </summary>
        public static void InExactSteps(int start, int end, int frames, LoopHandler loopBody, Action<int> onHold, Action onEnd) {
            var instance = GetInstance();
            if (Mathf.Abs(end - start) < frames)
                Debug.LogError("end - start should be greater than frame count");
            instance.StartCoroutine(instance.InExactStepsCo(start, end, frames, loopBody, onHold, onEnd));
        }

        IEnumerator InExactStepsCo(int start, int end, int cycles, LoopHandler loopBody, Action<int> onHold, Action onEnd) {
            int cycleLength = (end - start) / cycles;

            for (int i = start; i != end; i += (int)Mathf.Sign(end - start) * 1) {
                loopBody(i);
                if((i - start) % cycleLength == 0) {
                    onHold(i);
                    yield return null;
                }
            }
            onEnd();
            yield break;
        }

        /// <summary>
        /// Runs a loop where each step takes a particular amount of time (approx)
        /// </summary>
        public static void InTimedSteps(int start, int end, int maxRunDurationMs, LoopHandler loopBody, Action<int> onHold, Action onEnd) {
            var instance = GetInstance();
            instance.StartCoroutine(instance.AboveSpeedCo(start, end, maxRunDurationMs, loopBody, onHold, onEnd));
        }

        IEnumerator AboveSpeedCo(int start, int end, int maxFrameDurationMs, LoopHandler loopBody, Action<int> onHold, Action onEnd) {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            for(int i = start; i != end; i += (int)Mathf.Sign(end - start) * 1) {
                loopBody(i);
                if(stopwatch.ElapsedMilliseconds > maxFrameDurationMs) {
                    onHold(i);
                    yield return null;
                    stopwatch.Reset();
                    stopwatch.Start();
                }
            }
            stopwatch.Stop();
            onEnd();
            yield break;
        }
    }
}