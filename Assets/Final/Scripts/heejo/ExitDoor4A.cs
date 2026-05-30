using UnityEngine;
using UnityEngine.SceneManagement;

namespace HW09.heejo
{
    public class ExitDoor4A : MonoBehaviour
    {
        [Header("Highlight")]
        public Color glowColor = new Color(1f, 0.85f, 0.2f);
        public float glowIntensity = 2f;

        [Header("Proximity")]
        public float proximityDistance = 2f;

        private bool _active = false;
        private bool _transitioning = false;

        void Start()
        {
            SetEmission(false);

            BoxCollider col = gameObject.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size   = new Vector3(2f, 3f, 1f);
            col.center = new Vector3(0f, 1.5f, 0.5f);
        }

        public void Activate()
        {
            _active = true;
            SetEmission(true);
            Debug.Log("[ExitDoor4A] Door activated and glowing.");
        }

        void Update()
        {
            if (!_active || _transitioning) return;

            Transform cam = GetCameraTransform();
            if (cam == null) return;

            float dist = Vector3.Distance(cam.position, transform.position);
            if (dist <= proximityDistance)
            {
                Debug.Log($"[ExitDoor4A] Camera proximity ({dist:F2}m) → loading Scene4_B");
                LoadNextScene();
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (!_active || _transitioning) return;
            if (!IsPlayer(other)) return;
            Debug.Log($"[ExitDoor4A] Trigger entered by '{other.gameObject.name}' → loading Scene4_B");
            LoadNextScene();
        }

        void LoadNextScene()
        {
            _transitioning = true;
            SceneManager.LoadScene("Scene4_B");
        }

        Transform GetCameraTransform()
        {
            if (Camera.main != null) return Camera.main.transform;
            GameObject camObj = GameObject.Find("CenterEyeAnchor");
            if (camObj != null) return camObj.transform;
            return null;
        }

        bool IsPlayer(Collider other)
        {
            if (other.CompareTag("Player")) return true;
            if (other.CompareTag("MainCamera")) return true;

            string n = other.gameObject.name;
            if (n == "Main Camera") return true;
            if (n.Contains("OVRCameraRig") || n.Contains("CenterEyeAnchor") ||
                n.Contains("Player_OVRInput") || n.Contains("XR Origin")) return true;

            Transform t = other.transform.parent;
            int depth = 0;
            while (t != null && depth < 5)
            {
                if (t.CompareTag("Player")) return true;
                if (t.gameObject.name.Contains("Player_OVRInput")) return true;
                t = t.parent; depth++;
            }
            return false;
        }

        void SetEmission(bool active)
        {
            foreach (Renderer rend in GetComponentsInChildren<Renderer>())
            {
                foreach (Material mat in rend.materials)
                {
                    if (active)
                    {
                        mat.EnableKeyword("_EMISSION");
                        mat.SetColor("_EmissionColor", glowColor * glowIntensity);
                    }
                    else
                    {
                        mat.SetColor("_EmissionColor", Color.black);
                        mat.DisableKeyword("_EMISSION");
                        DynamicGI.SetEmissive(rend, Color.black);
                    }
                }
            }
        }
    }
}
