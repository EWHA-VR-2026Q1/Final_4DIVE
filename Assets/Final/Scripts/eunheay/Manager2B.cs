using System.Collections;
using UnityEngine;

namespace HW09
{
    /// <summary>
    /// Scene2_B 흐름:
    ///  진입 즉시 음소거 → 라디오 등장 → 근접 감지
    ///  → 해커 음성 → static → AI 종료 안내 → ClearSet(1) → MainScene 복귀
    /// </summary>
    public class Manager2B : MonoBehaviour
    {
        [Header("오디오")]
        public AudioSource aiVoice;
        public AudioClip   hackerClip;    // 해커 음성
        public AudioClip   staticClip;    // static 효과음 (비우면 자동 생성)
        public AudioClip   aiClip_Final;  // AI 종료 안내

        [Header("라디오 오브젝트")]
        [Tooltip("비워두면 'Radio_Normal' 이름으로 자동 탐색")]
        public GameObject radioNormal;
        [Tooltip("비워두면 'Radio_Worn' 이름으로 자동 탐색")]
        public GameObject radioWorn;

        [Header("출구 콜라이더 (클리어 전 비활성)")]
        public Collider exitTrigger;

        [Header("플레이어")]
        [Tooltip("비워두면 자동 탐색 (CenterEyeAnchor / Player_OVRInput_Parts)")]
        public Transform player;

        [Header("타이밍")]
        public float silenceDuration        = 8f;   // 음소거 후 라디오 등장까지 대기
        public float radioProximityDistance = 3.0f; // 빌드 테스트용 넓은 범위

        bool _sequenceStarted = false;
        Transform _eyeTransform;   // 실제 카메라/눈 위치 (거리 계산용)

        void Start()
        {
            // 진입 즉시 완전 무음
            AudioListener.volume = 0f;

            // 자동 탐색
            if (radioNormal == null) radioNormal = GameObject.Find("Radio_Normal");
            if (radioWorn   == null) radioWorn   = GameObject.Find("Radio_Worn");
            if (exitTrigger == null)
            {
                var go = GameObject.Find("ExitTrigger");
                if (go != null) exitTrigger = go.GetComponent<Collider>();
            }

            // 플레이어 루트 (Inspector에서 지정 없으면 자동 탐색)
            if (player == null)
            {
                string[] playerNames = { "CenterEyeAnchor", "Player_OVRInput_Parts", "OVRPlayerController", "PlayerController" };
                foreach (var n in playerNames)
                {
                    var go = GameObject.Find(n);
                    if (go != null) { player = go.transform; break; }
                }
            }

            // ★ 실제 눈/카메라 트랜스폼 탐색 (근접 거리 계산에 사용)
            _eyeTransform = FindEyeTransform();

            if (radioNormal != null) radioNormal.SetActive(false);
            if (radioWorn   != null) radioWorn.SetActive(false);
            if (exitTrigger != null) exitTrigger.enabled = false;

            // aiVoice 는 AudioListener.volume = 0 에도 들려야 함
            if (aiVoice != null) aiVoice.ignoreListenerVolume = true;

            StartCoroutine(SceneFlow());
        }

        Transform FindEyeTransform()
        {
            // 1순위: OVR CenterEyeAnchor (Quest 표준)
            var go = GameObject.Find("CenterEyeAnchor");
            if (go != null) { Debug.Log("[2B] eye=CenterEyeAnchor"); return go.transform; }

            // 2순위: Camera.main
            if (Camera.main != null) { Debug.Log("[2B] eye=Camera.main"); return Camera.main.transform; }

            // 3순위: 씬 내 활성화된 첫 번째 Camera
            foreach (var cam in FindObjectsOfType<Camera>())
            {
                if (cam.enabled && cam.gameObject.activeInHierarchy)
                {
                    Debug.Log($"[2B] eye={cam.name}");
                    return cam.transform;
                }
            }

            // 4순위: player 루트 폴백
            Debug.LogWarning("[2B] eye 못 찾음 → player 루트 사용");
            return player;
        }

        void OnDestroy() => AudioListener.volume = 1f;

        void Update()
        {
            if (_sequenceStarted || radioNormal == null || !radioNormal.activeSelf) return;

            // _eyeTransform 런타임 재탐색 (씬 로드 직후 못 찾은 경우)
            if (_eyeTransform == null) _eyeTransform = FindEyeTransform();
            if (_eyeTransform == null) return;

            float dist = Vector3.Distance(_eyeTransform.position, radioNormal.transform.position);
            if (dist < radioProximityDistance)
                StartRadioSequence();
        }

        // Radio2B 에서 그랩으로도 트리거 가능
        public void OnRadioActivated(float _) => StartRadioSequence();

        void StartRadioSequence()
        {
            if (_sequenceStarted) return;
            _sequenceStarted = true;
            StartCoroutine(RadioSequence());
        }

        // ── 씬 흐름 ─────────────────────────────────────────────
        IEnumerator SceneFlow()
        {
            // 침묵 대기 후 라디오 등장
            yield return new WaitForSeconds(silenceDuration);

            if (radioNormal != null) radioNormal.SetActive(true);
            Debug.Log("[Scene2_B] 라디오 등장");
        }

        // ── 라디오 시퀀스 ────────────────────────────────────────
        IEnumerator RadioSequence()
        {
            // Radio_Normal → Radio_Worn 교체
            if (radioNormal != null) radioNormal.SetActive(false);
            if (radioWorn   != null) radioWorn.SetActive(true);

            // 해커 음성
            yield return PlayClip(hackerClip, 3f);

            // static 효과음
            yield return PlayClip(staticClip != null ? staticClip : CreatePopClip(), 0.5f);

            // 볼륨 복원
            AudioListener.volume = 1f;

            // AI 종료 안내
            yield return PlayClip(aiClip_Final, 2f);

            // 출구 활성화
            if (exitTrigger != null) exitTrigger.enabled = true;

            // 클리어 등록 → MainScene 복귀
            if (ManagerMain.Instance != null)
                ManagerMain.Instance.ClearSet(1);
            else
                Debug.LogWarning("[Manager2B] ManagerMain.Instance null");

            yield return new WaitForSeconds(1.5f);

            if (ManagerMain.Instance != null)
                ManagerMain.Instance.ReturnToHub();
        }

        // ── 헬퍼 ────────────────────────────────────────────────
        IEnumerator PlayClip(AudioClip clip, float fallback)
        {
            if (clip == null || aiVoice == null)
            {
                yield return new WaitForSeconds(fallback);
                yield break;
            }
            aiVoice.clip = clip;
            aiVoice.Play();
            yield return new WaitForSeconds(clip.length + 0.3f);
        }

        AudioClip CreatePopClip()
        {
            int     rate    = 44100;
            int     samples = Mathf.RoundToInt(rate * 0.12f);
            float[] data    = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                data[i] = Mathf.Sin(2f * Mathf.PI * 120f * t) * Mathf.Exp(-t * 30f) * 0.9f;
            }
            var clip = AudioClip.Create("Static", samples, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
