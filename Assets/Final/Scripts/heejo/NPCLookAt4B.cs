using UnityEngine;

namespace HW09.heejo
{
    public class NPCLookAt4B : MonoBehaviour
    {
        public bool isLooking = false;

        private Transform _playerTarget;
        private const float LookSpeed = 8.0f;

        void Start()
        {
            if (Camera.main != null)
                _playerTarget = Camera.main.transform;
        }

        void Update()
        {
            if (!isLooking || _playerTarget == null) return;

            Vector3 dir = _playerTarget.position - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, LookSpeed * Time.deltaTime);
            }
        }
    }
}
