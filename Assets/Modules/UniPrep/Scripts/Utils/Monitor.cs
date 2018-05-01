using System;
using UnityEngine;

namespace UniPrep.Utils {
    [DisallowMultipleComponent]
    public class Monitor : MonoBehaviour {
        // OnTriggerEnter
        Action<Collider> m_OnTriggerEnter;
        public void HandleTriggerEnter(Action<Collider> callback) {
            m_OnTriggerEnter = callback;
        }
        void OnTriggerEnter(Collider collider) {
            if(m_OnTriggerEnter!= null )m_OnTriggerEnter(collider);
        }

        // OnTriggerExit
        Action<Collider> m_OnTriggerExit;
        public void HandleTriggerExit(Action<Collider> callback) {
            m_OnTriggerExit = callback;
        }
        void OnTriggerExit(Collider collider) {
            if (m_OnTriggerExit != null) m_OnTriggerExit(collider);
        }

        // OnTriggerStay
        Action<Collider> m_OnTriggerStay;
        public void HandleTriggerStay(Action<Collider> callback) {
            m_OnTriggerStay = callback;
        }
        void OnTriggerStay(Collider collider) {
            if (m_OnTriggerStay != null) m_OnTriggerStay(collider);
        }

        // OnCollisionEnter
        Action<Collision> m_OnCollisionEnter;
        public void HandleCollisionEnter(Action<Collision> callback) {
            m_OnCollisionEnter = callback;
        }
        void OnCollisionEnter(Collision collision) {
            if (m_OnCollisionEnter != null) m_OnCollisionEnter(collision);
        }

        // OnCollisionExit
        Action<Collision> m_OnCollisionExit;
        public void HandleCollisionExit(Action<Collision> callback) {
            m_OnCollisionExit = callback;
        }
        void OnCollisionExit(Collision collision) {
            if (m_OnCollisionExit != null) m_OnCollisionExit(collision);
        }

        // OnCollisionStay
        Action<Collision> m_OnCollisionStay;
        public void HandleCollisionStay(Action<Collision> callback) {
            m_OnCollisionStay = callback;
        }
        void OnCollisionStay(Collision collision) {
            if (m_OnCollisionStay != null) m_OnCollisionStay(collision);
        }
    }

    public static class MonitorExtensions {
        public static Monitor AddMonitor(this GameObject gameObject) {
            Monitor monitor = gameObject.GetComponent<Monitor>();
            if (monitor == null)
                monitor = gameObject.AddComponent<Monitor>();
            return monitor;
        }
    }
}
