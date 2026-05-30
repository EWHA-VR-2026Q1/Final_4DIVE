using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HW09
{
    /// <summary>
    /// Scene2_A 흐름:
    ///  1) 숲 + 바람 환경음 재생
    ///  2) AI 진입 음성
    ///  3) AI 탐험 유도 음성
    ///  4) 10초 후 Scene2_B 자동 전환
    /// </summary>
    public class Manager2A : MonoBehaviour
    {
        [Header("오디오 — 환경")]
        public AudioSource ambientSound;
        [Tooltip("비워두면 'WindSound' 이름으로 자동 탐색")]
        public AudioSource windAudio;
        public AudioSource aiVoice;
        public AudioClip   aiClip_Enter;
        public AudioClip   aiClip_Explore;

        [Header("탐험 유도 음성 종료 후 대기 (초)")]
        public float delayBeforeScene2B = 10f;

        void Start()
        {
            if (windAudio == null)
            {
                // 이름으로 탐색 (다양한 이름 대응)
                string[] windNames = { "WindSound", "Wind", "WindAmbient", "wind", "WindAudio" };
                foreach (var n in windNames)
                {
                    var ws = GameObject.Find(n);
                    if (ws != null) { windAudio = ws.GetComponent<AudioSource>(); break; }
                }
            }
            StartCoroutine(SceneFlow());
        }

        IEnumerator SceneFlow()
        {
            // ① 환경음 재생
            if (ambientSound != null) ambientSound.Play();
            if (windAudio    != null) windAudio.Play();

            yield return new WaitForSeconds(3f);

            // ② AI 진입 음성
            if (aiClip_Enter != null && aiVoice != null)
            {
                aiVoice.clip = aiClip_Enter;
                aiVoice.Play();
                yield return new WaitForSeconds(aiClip_Enter.length + 2f);
            }

            // ③ AI 탐험 유도 음성
            if (aiClip_Explore != null && aiVoice != null)
            {
                aiVoice.clip = aiClip_Explore;
                aiVoice.Play();
                yield return new WaitForSeconds(aiClip_Explore.length);
            }

            // ④ 10초 후 Scene2_B 전환
            yield return new WaitForSeconds(delayBeforeScene2B);
            SceneManager.LoadScene("Scene2_B");
        }
    }
}
