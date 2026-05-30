using System.Collections;
using UnityEngine;

namespace HW09
{
    /// <summary>
    /// WaterPuddle 오브젝트에 부착.
    /// Scene1과 동일한 Custom/waterWave 머티리얼로 복도 바닥에 물웅덩이 생성.
    /// </summary>
    [RequireComponent(typeof(MeshRenderer))]
    public class WaterPuddleEffect : MonoBehaviour
    {
        [Header("물 머티리얼 (A_WaterWave.mat 드래그)")]
        public Material waterMaterial;

        [Header("waterMaterial 없을 때 사용")]
        public Color waterColor = new Color(0.12f, 0.28f, 0.42f, 0.90f);
        public Color deepColor  = new Color(0.02f, 0.06f, 0.14f, 1.00f);

        [Header("웅덩이 배치 (Z위치, 가로, 세로)")]
        static readonly (float z, float w, float d)[] Puddles =
        {
            ( 8f, 2.5f, 1.2f), (14f, 1.6f, 0.8f), (21f, 3.0f, 1.5f),
            (28f, 1.8f, 1.0f), (35f, 2.2f, 1.2f), (42f, 3.5f, 1.8f),
            (48f, 2.0f, 1.0f),
        };

        [Header("물방울 파티클")]
        public bool enableDrips = true;

        void OnEnable() => StartCoroutine(Build());

        IEnumerator Build()
        {
            yield return null;

            // 원본 Quad → 첫 번째 웅덩이로 재활용
            SetupPuddleMesh(gameObject, 0f, 26f, 3.0f, 1.5f);

            yield return new WaitForSeconds(0.2f);

            foreach (var (z, w, d) in Puddles)
            {
                SpawnPuddle(z, w, d);
                yield return new WaitForSeconds(0.15f);
            }

            if (enableDrips) SpawnDrips();
        }

        // ── 머티리얼 생성 ─────────────────────────────────────────
        Material MakeWaterMat()
        {
            if (waterMaterial != null)
                return new Material(waterMaterial);

            var shader = Shader.Find("Custom/waterWave");
            if (shader != null)
            {
                var mat = new Material(shader);
                mat.SetColor("_Color",          waterColor);
                mat.SetColor("_DeepColor",      deepColor);
                mat.SetFloat("_WaveHeight",     0f);
                mat.SetFloat("_WaveSpeed",      1.2f);
                mat.SetFloat("_WaveFreq",       2.0f);
                mat.SetFloat("_Glossiness",     0.97f);
                mat.SetFloat("_NormalStrength", 1.5f);
                mat.SetFloat("_FresnelPower",   2.5f);
                mat.SetColor("_SpecularColor",  new Color(0.9f, 0.95f, 1f, 1f));
                mat.SetFloat("_SpecularPower",  128f);
                return mat;
            }

            // 폴백: Unlit/Color (Quest 항상 동작)
            var s  = Shader.Find("Unlit/Color") ?? Shader.Find("Mobile/Diffuse");
            var fb = new Material(s);
            fb.color = new Color(0.05f, 0.18f, 0.38f);
            return fb;
        }

        // ── 웅덩이 생성 ──────────────────────────────────────────
        void SpawnPuddle(float z, float width, float depth)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = $"Puddle_z{z:0}";
            Destroy(go.GetComponent<MeshCollider>());
            SetupPuddleMesh(go, 0f, z, width, depth);
        }

        void SetupPuddleMesh(GameObject go, float x, float z, float w, float d)
        {
            go.transform.position    = new Vector3(x, 0.015f, z);
            go.transform.eulerAngles = new Vector3(90f, 0f, 0f);
            go.transform.localScale  = new Vector3(w, d, 1f);
            go.GetComponent<MeshRenderer>().material = MakeWaterMat();
            SpawnRipples(go.transform.position + Vector3.up * 0.005f, w, d);
        }

        // ── 잔물결 파티클 ────────────────────────────────────────
        void SpawnRipples(Vector3 center, float w, float d)
        {
            var go  = new GameObject("Ripples");
            go.transform.position = center;
            var ps  = go.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.loop            = true;
            main.duration        = 3f;
            main.startLifetime   = new ParticleSystem.MinMaxCurve(1.8f, 3.0f);
            main.startSpeed      = 0f;
            main.startSize       = new ParticleSystem.MinMaxCurve(0.06f, 0.28f);
            main.startColor      = new ParticleSystem.MinMaxGradient(
                new Color(0.5f, 0.75f, 1.0f, 0.55f),
                new Color(0.2f, 0.45f, 0.85f, 0.25f));
            main.gravityModifier = 0f;
            main.maxParticles    = 25;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startRotation   = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);

            var em = ps.emission;
            em.rateOverTime = 4f;

            var sh = ps.shape;
            sh.shapeType = ParticleSystemShapeType.Rectangle;
            sh.scale     = new Vector3(w * 0.65f, d * 0.65f, 0.01f);
            sh.rotation  = new Vector3(90f, 0f, 0f);

            var sov = ps.sizeOverLifetime;
            sov.enabled = true;
            sov.size = new ParticleSystem.MinMaxCurve(1f,
                new AnimationCurve(new Keyframe(0f, 0.05f), new Keyframe(0.4f, 0.8f), new Keyframe(1f, 2.0f)));

            var cov = ps.colorOverLifetime;
            cov.enabled = true;
            var g = new Gradient();
            g.SetKeys(
                new[] { new GradientColorKey(new Color(0.6f, 0.85f, 1f), 0f),
                        new GradientColorKey(new Color(0.3f, 0.6f, 1f), 1f) },
                new[] { new GradientAlphaKey(0.6f, 0f),
                        new GradientAlphaKey(0.3f, 0.5f),
                        new GradientAlphaKey(0f,   1f) });
            cov.color = new ParticleSystem.MinMaxGradient(g);

            var ren = go.GetComponent<ParticleSystemRenderer>();
            ren.renderMode = ParticleSystemRenderMode.Billboard;
            ren.material   = new Material(Shader.Find("Sprites/Default"))
                             { color = new Color(0.5f, 0.8f, 1f, 0.6f) };
        }

        // ── 천장 물방울 ───────────────────────────────────────────
        void SpawnDrips()
        {
            float[] dripZ = { 10f, 22f, 35f, 48f };
            foreach (float z in dripZ)
            {
                float xOff = Random.Range(-0.8f, 0.8f);
                var go = new GameObject($"Drip_z{z:0}");
                go.transform.position = new Vector3(xOff, 3.75f, z);

                var ps   = go.AddComponent<ParticleSystem>();
                var main = ps.main;
                main.loop            = true;
                main.duration        = 3f;
                main.startLifetime   = new ParticleSystem.MinMaxCurve(0.55f, 1.1f);
                main.startSpeed      = new ParticleSystem.MinMaxCurve(1.8f, 3.5f);
                main.startSize       = new ParticleSystem.MinMaxCurve(0.012f, 0.035f);
                main.startColor      = new ParticleSystem.MinMaxGradient(
                    new Color(0.45f, 0.65f, 0.95f, 0.90f),
                    new Color(0.25f, 0.45f, 0.85f, 0.65f));
                main.gravityModifier = 1.8f;
                main.maxParticles    = 30;
                main.simulationSpace = ParticleSystemSimulationSpace.World;

                var em = ps.emission;
                em.rateOverTime = new ParticleSystem.MinMaxCurve(3f, 7f);

                var sh = ps.shape;
                sh.shapeType = ParticleSystemShapeType.Rectangle;
                sh.scale     = new Vector3(0.25f, 0.08f, 0.01f);

                var cov = ps.colorOverLifetime;
                cov.enabled = true;
                var g = new Gradient();
                g.SetKeys(
                    new[] { new GradientColorKey(new Color(0.6f, 0.8f, 1f), 0f),
                            new GradientColorKey(new Color(0.3f, 0.5f, 1f), 1f) },
                    new[] { new GradientAlphaKey(0.9f, 0f), new GradientAlphaKey(0.1f, 1f) });
                cov.color = new ParticleSystem.MinMaxGradient(g);

                var ren = go.GetComponent<ParticleSystemRenderer>();
                ren.material = new Material(Shader.Find("Sprites/Default"))
                               { color = new Color(0.4f, 0.65f, 1f, 0.8f) };
            }
        }
    }
}
