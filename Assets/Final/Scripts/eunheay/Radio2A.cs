using System.Collections;
using UnityEngine;

namespace HW09
{
    /// <summary>
    /// Scene2_A 라디오 — 아래 3가지 방식으로 상호작용 가능
    ///  1) IRayInteractable  : EX_OVRInput_RayForInteraction 이 씬에 있을 때
    ///  2) ISampleInteractable : EditorHandSimulator 에디터 테스트용
    ///  3) 자체 Update 감지 : 카메라 레이캐스트 + 근접(proximityDistance) — 실기기 fallback
    /// </summary>
    public class Radio2A : MonoBehaviour, ISampleInteractable, IRayInteractable
    {
        [Header("호버 연출 (선택)")]
        public Light  radioLight;
        public float  hoverIntensityBoost = 1.5f;

        [Header("자체 입력 감지")]
        [Tooltip("카메라 레이캐스트 최대 거리 (m)")]
        public float rayDetectDistance  = 8f;
        [Tooltip("이 거리 이내에서 버튼을 누르면 무조건 작동 (라디오가 작아서 레이 빗나갈 때 대비)")]
        public float proximityDistance  = 1.5f;

        private Manager2A _manager;
        private bool      _used          = false;
        private float     _baseIntensity = 1f;

        // ── Lifecycle ────────────────────────────────────────────
        void Start()
        {
            _manager = FindObjectOfType<Manager2A>();

            if (radioLight != null)
            {
                _baseIntensity = radioLight.intensity;
                radioLight.enabled = true;
            }
        }

        void Update()
        {
            if (_used) return;

            // 인덱스 트리거 또는 핸드 트리거 (양손)
            bool pressed =
                OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger) ||
                OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger) ||
                OVRInput.GetDown(OVRInput.RawButton.LHandTrigger)  ||
                OVRInput.GetDown(OVRInput.RawButton.RHandTrigger);

            if (!pressed) return;

            Camera cam = Camera.main;
            if (cam == null) return;

            // ① 카메라 레이캐스트로 라디오 히트 확인
            if (Physics.Raycast(cam.transform.position, cam.transform.forward,
                                out RaycastHit hit, rayDetectDistance))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    Interact();
                    return;
                }
            }

            // ② 근거리 fallback — 라디오가 카메라 범위 안에 있을 때
            if (Vector3.Distance(transform.position, cam.transform.position) < proximityDistance)
                Interact();
        }

        // ── ISampleInteractable (OVR 그랩 / 에디터 시뮬레이터) ───
        public void OnGrab() => Interact();

        // ── IRayInteractable (레이 포인터) ───────────────────────
        public void OnRayEnter()
        {
            if (_used || radioLight == null) return;
            radioLight.intensity = _baseIntensity * hoverIntensityBoost;
        }
        public void OnRayStay() { }
        public void OnRayExit()
        {
            if (_used || radioLight == null) return;
            radioLight.intensity = _baseIntensity;
        }
        public void OnRayClick() => Interact();

        // ── Core ─────────────────────────────────────────────────
        void Interact()
        {
            if (_used) return;
            _used = true;

            if (radioLight != null)
                StartCoroutine(FadeLightOut());

            // Scene2_A는 자동 전환 방식 — 라디오 상호작용 콜백 없음
        }

        IEnumerator FadeLightOut()
        {
            if (radioLight == null) yield break;
            float t     = 0f;
            float start = radioLight.intensity;
            while (t < 1f)
            {
                t += Time.deltaTime * 2f;
                radioLight.intensity = Mathf.Lerp(start, 0f, t);
                yield return null;
            }
            radioLight.enabled = false;
        }
    }
}
