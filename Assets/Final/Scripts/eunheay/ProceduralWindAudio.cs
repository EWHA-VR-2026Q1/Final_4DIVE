using UnityEngine;

namespace HW09
{
    /// <summary>
    /// OnAudioFilterRead 를 이용해 바람 소리를 실시간으로 생성합니다.
    /// AudioSource 가 필요하지만 clip 은 필요 없습니다.
    /// Manager2A.FadeOutAmbient() 에서 volume 을 0으로 내려 페이드아웃합니다.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class ProceduralWindAudio : MonoBehaviour
    {
        [Range(0f, 1f)]
        public float volume = 0.35f;

        // Paul Kellet's pink noise filter state
        private float _b0, _b1, _b2, _b3, _b4, _b5, _b6;
        private float _lfoPhase;
        private float _sampleRate = 44100f;   // Start()에서 캐싱, 오디오 스레드에서 직접 조회 불가

        void Start()
        {
            // outputSampleRate 는 메인 스레드에서만 호출 가능 → 여기서 캐싱
            _sampleRate = AudioSettings.outputSampleRate;

            // AudioSource 는 loop=true, clip=침묵 으로 OnAudioFilterRead 를 계속 호출
            var src = GetComponent<AudioSource>();
            var silent = AudioClip.Create("_wind_silent", 4096, 1, (int)_sampleRate, false);
            src.clip       = silent;
            src.loop       = true;
            src.volume     = 1f;
            src.spatialBlend = 0f;
            src.Play();
        }

        void OnAudioFilterRead(float[] data, int channels)
        {
            if (volume <= 0f) return;

            float sampleRate = _sampleRate;
            const float lfoSpeed = 0.18f;   // 바람 세기 변동 주기 (느릿하게)

            for (int i = 0; i < data.Length; i += channels)
            {
                float w = Random.Range(-1f, 1f);

                // Paul Kellet 핑크 노이즈 알고리즘 (바람 특성 주파수 분포)
                _b0 = 0.99886f * _b0 + w * 0.0555179f;
                _b1 = 0.99332f * _b1 + w * 0.0750759f;
                _b2 = 0.96900f * _b2 + w * 0.1538520f;
                _b3 = 0.86650f * _b3 + w * 0.3104856f;
                _b4 = 0.55000f * _b4 + w * 0.5329522f;
                _b5 = -0.7616f * _b5 - w * 0.0168980f;
                float pink = (_b0 + _b1 + _b2 + _b3 + _b4 + _b5 + _b6 + w * 0.5362f) * 0.11f;
                _b6 = w * 0.115926f;

                // 느린 LFO 로 바람 세기 자연스럽게 변동
                _lfoPhase += lfoSpeed / sampleRate;
                if (_lfoPhase > 1f) _lfoPhase -= 1f;
                float lfo = 0.65f + 0.35f * Mathf.Sin(2f * Mathf.PI * _lfoPhase);

                float sample = pink * volume * lfo;

                for (int c = 0; c < channels; c++)
                    data[i + c] += sample;
            }
        }
    }
}
