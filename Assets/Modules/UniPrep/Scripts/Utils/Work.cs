using System;
using UnityEngine;
using System.Text;
using System.Collections;

namespace UniPrep.Utils {
    public class Work<T> {
        // ================================================
        // FIELDS
        // ================================================
        const string k_ExecutorNamePrefix = "WORK_";
        WorkExecutor m_Executor;
        string m_ID;

        /// <summary>
        /// Returns if the <see cref="Work"/> is currently running
        /// </summary>
        public bool Running {
            get { return m_Executor.IsRunning(); }
        }

        /// <summary>
        /// Returns it the <see cref="Work"/> is paused
        /// </summary>
        public bool Paused {
            get { return m_Executor.IsPaused(); }
        }

        // ================================================
        // CONSTRUCTORS AND PREFABS
        // ================================================
        /// <summary>
        /// Constructs a <see cref="Work"/> obejct with the coroutine method
        /// </summary>
        /// <param name="method">The coroutine method to run</param>
        public Work(IEnumerator method) {
            m_ID = Guid.NewGuid().ToString();

            m_Executor = WorkExecutor.pool.Get();
            m_Executor.name = k_ExecutorNamePrefix + m_ID;
            m_Executor.SetCoroutineMethod(method);
        }
        
        /// <summary>
        /// Runs the given action after a delay
        /// </summary>
        /// <param name="delay">The delay before the action is executed</param>
        /// <param name="onDone">The Action to execute</param>
        public static void StartDelayed(float delay, Action onDone) {
            Work delayWork = new Work(DelayCo(delay));
            delayWork.Begin(() => {
                onDone();
            });
        }
        
        private static IEnumerator DelayCo(float delayDuration) {
            yield return new WaitForSeconds(delayDuration);
            yield return null;
        }

        public new string ToString() {
            return new StringBuilder()
                .Append("Work ID : ").Append(m_ID.ToString())
                .Append("Running : ").Append(Running.ToString())
                .Append("Paused : ").Append(Paused.ToString())
                .ToString();
        }

        // ================================================
        // EXTERNAL INVOKES
        // ================================================
        /// <summary>
        /// Begins the <see cref="Work"/> execution and gives back a callback
        /// </summary>
        /// <param name="onDone">The callback invoked when the execution ends along with the result</param>
        public void Begin(Action<T> onDone) {
            m_Executor.Begin((result) => {
                if(onDone != null)
                    onDone((T)result);
            });
        }

        /// <summary>
        /// Begins the <see cref="Work"/> execution and gives back a callback
        /// </summary>
        /// <param name="onDone">The callback invoked when the execution ends</param>
        public void Begin(Action onDone) {
            m_Executor.Begin((result) => {
                if (onDone != null)
                    onDone();
            });
        }

        /// <summary>
        /// Begins the <see cref="Work"/> execution
        /// </summary>
        public void Begin() {
            m_Executor.Begin((result) => { });
        }

        /// <summary>
        /// Pauses the <see cref="Work"/>
        /// </summary>
        public void Pause() {
            m_Executor.Pause();
        }

        /// <summary>
        /// Resumes the <see cref="Work"/>
        /// </summary>
        public void Resume() {
            m_Executor.Resume();
        }

        /// <summary>
        /// Stops the <see cref="Work"/> 
        /// </summary>
        public void End() {
            m_Executor.End();
        }
    }

    public class Work : Work<object> {
        public Work(IEnumerator method) : base(method) { }
    }

    public class WorkExecutor : MonoBehaviour {
        // ================================================
        // FIELDS
        // ================================================
        public static ComponentPool<WorkExecutor> pool = new ComponentPool<WorkExecutor>();
        IEnumerator m_Routine;
        Action<object> m_OnDone;
        object m_Result;
        bool m_Running;
        bool m_Paused;

        // Make sure the executor persists across scenes and don't show it
        void Awake() {
            DontDestroyOnLoad(gameObject);
            gameObject.hideFlags = HideFlags.HideAndDontSave;
        }

        // When the object is destroyed (ie. when the game closes)
        void OnDestroy() {
            m_Running = false;
        }

        /// <summary>
        /// Sets the routine to be execution
        /// </summary>
        /// <param name="method">The routine to be executed</param>
        public void SetCoroutineMethod(IEnumerator method) {
            m_Routine = method;
        }

        /// <summary>
        /// Begins execution of the routine
        /// </summary>
        /// <param name="onDone">When the execution completes</param>
        public void Begin(Action<object> onDone) {
            m_OnDone = onDone;
            m_Running = true;
            StartCoroutine(ExecuteInternalCoroutine());
        }

        IEnumerator ExecuteInternalCoroutine() {
            while (m_Running) {
                if (m_Paused)
                    yield return null;

                if (m_Routine != null && m_Routine.MoveNext())
                    yield return m_Routine.Current;
                else {
                    m_Result = m_Routine.Current;
                    m_Running = false;
                }
            }
            Terminate();
            yield break;
        }

        /// <summary>
        /// Terminates the coroutine and the gameobject it is running on
        /// </summary>
        public void Terminate() {
            StopCoroutine(ExecuteInternalCoroutine());
            m_Routine = null;
            m_OnDone(m_Result);
            pool.Free(this);
        }

        public void Pause() {
            m_Paused = true;
        }

        public void Resume() {
            m_Paused = false;
        }

        public void End() {
            m_Running = false;
        }

        public bool IsRunning() {
            return m_Running;
        }

        public bool IsPaused() {
            return m_Paused;
        }
    }
}