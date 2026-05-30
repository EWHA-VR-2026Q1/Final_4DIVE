using System.Collections;
using UnityEngine;

namespace HW09
{
    /// <summary>
    /// Scene2_B 낡은 라디오 — 레이 클릭 / OVR 그랩 모두 처리
    /// ISampleInteractable : OVR 그랩
    /// IRayInteractable    : 레이 포인터 클릭
    /// </summary>
    public class Radio2B : MonoBehaviour, ISampleInteractable, IRayInteractable
    {
        [Header("오디오")]
        public AudioClip hackerClip;          // "소리가 없으면 공간도 죽어버려..."
        public AudioSource radioSource;

        [Header("호버 연출")]
        public Light radioLight;              // 라디오 주변 포인트 라이트 (선택)
        public float hoverIntensityBoost = 1.5f;

        private Manager2B _manager;
        private bool _used = false;
        private float _baseIntensity;

        // ── Lifecycle ────────────────────────────────────────────
        void Start()
        {
            _manager = FindObjectOfType<Manager2B>();
            if (radioLight != null)
            {
                _baseIntensity = radioLight.intensity;
                radioLight.enabled = true;
            }

            // 라디오는 AudioListener.volume 0 에 영향 안 받게
            if (radioSource != null)
                radioSource.ignoreListenerVolume = true;
        }

        // ── ISampleInteractable (OVR 그랩) ───────────────────────
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

            if (hackerClip != null)
            {
                radioSource.clip   = hackerClip;
                radioSource.volume = 1f;
                radioSource.Play();
                float len = hackerClip.length;
                if (_manager != null) _manager.OnRadioActivated(len);
            }
            else
            {
                Debug.LogWarning("[Radio2B] hackerClip이 비어 있습니다!");
                if (_manager != null) _manager.OnRadioActivated(3f);
            }
        }

        IEnumerator FadeLightOut()
        {
            if (radioLight == null) yield break;
            float t = 0f;
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
