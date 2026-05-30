using System.Collections;
using UnityEngine;

namespace HW09
{
    /// <summary>
    /// BurstPipe 오브젝트에 부착.
    /// 천장 파이프 파열 효과: 복도 전체에 증기 분사 + 쉿 소리.
    /// StructureTwist 연기(느린 먼지)와 달리 빠르고 밀도 높은 흰 증기.
    /// </summary>
    public class BurstPipeEffect : MonoBehaviour
    {
        [Header("증기 색상")]
        public Color steamColor     = new Color(0.93f, 0.95f, 0.97f, 0.85f);
        public Color steamColorThin = new Color(0.80f, 0.84f, 0.90f, 0.35f);

        [Header("쉿 소리 (AudioClip 드래그)")]
        public AudioClip hissClip;
        [Range(0f, 1f)] public float hissVolume = 0.5f;

        // 파이프 위치: (X, Z)  천장 고정 Y=3.65
        static readonly (float x, float z)[] Pipes =
        {
            (-1.5f,  7f), ( 1.5f, 14f),
            (-1.0f, 21f), ( 1.2f, 29f),
            (-1.8f, 36f), ( 0.8f, 43f),
            (-1.3f, 50f),
        };

        void OnEnable() => StartCoroutine(Build());

        IEnumerator Build()
        {
            yield return null;
            foreach (var (x, z) in Pipes)
            {
                SpawnPipe(x, z);
                yield return new WaitForSeconds(0.2f);
            }
        }

        void SpawnPipe(float x, float z)
        {
            var root = new GameObject($"Pipe_{z:0}");
            root.transform.position = new Vector3(x, 3.65f, z);

            // ── 파이프 비주얼 (짧은 원통) ───────────────────────
            var pipe = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pipe.name = "PipeVisual";
            Destroy(pipe.GetComponent<Collider>());
            pipe.transform.SetParent(root.transform);
            pipe.transform.localPosition = Vector3.zero;
            pipe.transform.localEulerAngles = new Vector3(0f, 0f, 90f); // 수평 파이프
            pipe.transform.localScale = new Vector3(0.06f, 0.25f, 0.06f);
            var pipeMat = new Material(Shader.Find("Unlit/Color"))
                          { color = new Color(0.25f, 0.22f, 0.20f) };
            pipe.GetComponent<MeshRenderer>().material = pipeMat;

            // 파열 끝부분 (약간 아래 방향)
            var tip = new GameObject("SteamTip");
            tip.transform.SetParent(root.transform);
            tip.transform.localPosition = new Vector3(0f, -0.05f, 0f);

            // ── 증기 파티클 ──────────────────────────────────────
            SpawnSteam(tip.transform.position, large: true);   // 굵은 메인 증기
            SpawnSteam(tip.transform.position, large: false);  // 얇은 퍼짐 증기

            // ── 쉿 소리 ──────────────────────────────────────────
            if (hissClip != null)
            {
                var src = root.AddComponent<AudioSource>();
                src.clip         = hissClip;
                src.loop         = true;
                src.spatialBlend = 1f;      // 3D
                src.volume       = hissVolume;
                src.minDistance  = 0.5f;
                src.maxDistance  = 6f;
                src.rolloffMode  = AudioRolloffMode.Linear;
                src.Play();
            }
        }

        void SpawnSteam(Vector3 pos, bool large)
        {
            var go = new GameObject(large ? "Steam_Main" : "Steam_Thin");
            go.transform.position = pos;
            var ps   = go.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.loop            = true;
            main.duration        = 2f;
            main.startLifetime   = large
                ? new ParticleSystem.MinMaxCurve(1.2f, 2.0f)
                : new ParticleSystem.MinMaxCurve(0.8f, 1.5f);
            main.startSpeed      = large
                ? new ParticleSystem.MinMaxCurve(1.5f, 3.0f)
                : new ParticleSystem.MinMaxCurve(0.4f, 1.2f);
            main.startSize       = large
                ? new ParticleSystem.MinMaxCurve(0.08f, 0.22f)
                : new ParticleSystem.MinMaxCurve(0.15f, 0.40f);
            main.startColor      = new ParticleSystem.MinMaxGradient(steamColor, steamColorThin);
            main.gravityModifier = -0.15f;   // 증기가 위로 가볍게 올라감
            main.maxParticles    = large ? 80 : 50;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startRotation   = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);

            var em = ps.emission;
            em.rateOverTime = large ? 35f : 20f;

            // 아래방향 분사 Cone
            var sh = ps.shape;
            sh.shapeType = ParticleSystemShapeType.Cone;
            sh.angle     = large ? 18f : 35f;
            sh.radius    = 0.03f;
            sh.rotation  = new Vector3(180f, 0f, 0f); // 아래로 분사

            // 속도: 퍼지면서 느려짐
            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.space   = ParticleSystemSimulationSpace.World;
            vel.x       = new ParticleSystem.MinMaxCurve(-0.4f, 0.4f);
            vel.y       = new ParticleSystem.MinMaxCurve(-0.2f, 0.3f);

            // 크기: 분사 후 부풀어 오름
            var sov = ps.sizeOverLifetime;
            sov.enabled = true;
            sov.size = new ParticleSystem.MinMaxCurve(1f,
                new AnimationCurve(
                    new Keyframe(0f, 0.15f),
                    new Keyframe(0.35f, 1f),
                    new Keyframe(1f, large ? 2.5f : 3.5f)));

            // 투명도: 빠르게 나타났다가 서서히 사라짐
            var cov = ps.colorOverLifetime;
            cov.enabled = true;
            var g = new Gradient();
            g.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f),
                        new GradientColorKey(new Color(0.85f, 0.88f, 0.92f), 1f) },
                new[] { new GradientAlphaKey(0f, 0f),
                        new GradientAlphaKey(large ? 0.85f : 0.5f, 0.1f),
                        new GradientAlphaKey(large ? 0.6f : 0.3f, 0.6f),
                        new GradientAlphaKey(0f, 1f) });
            cov.color = new ParticleSystem.MinMaxGradient(g);

            // Quest 호환 머티리얼
            var ren = go.GetComponent<ParticleSystemRenderer>();
            ren.renderMode = ParticleSystemRenderMode.Billboard;
            ren.material   = new Material(Shader.Find("Sprites/Default"))
                             { color = Color.white };
        }
    }
}
