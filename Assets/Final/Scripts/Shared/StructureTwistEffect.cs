using System.Collections;
using UnityEngine;

namespace HW09
{
    /// <summary>
    /// StructureTwist 오브젝트에 부착.
    /// 복도 전체에 먼지/연기, 조명 깜빡임 효과 생성.
    /// </summary>
    public class StructureTwistEffect : MonoBehaviour
    {
        [Header("조명 깜빡임")]
        public bool flickerLights = true;
        [Range(0.05f, 0.5f)] public float flickerSpeed = 0.12f;

        void OnEnable()
        {
            var mr = GetComponent<MeshRenderer>();
            if (mr != null) mr.enabled = false;
            StartCoroutine(Build());
        }

        IEnumerator Build()
        {
            yield return null;
            SpawnCorridorSmoke();
            SpawnDebrisPiles();
            if (flickerLights) StartCoroutine(FlickerLights());
        }

        // ── 바닥 잔해 더미 ────────────────────────────────────────
        void SpawnDebrisPiles()
        {
            var rng = new System.Random(9999);
            float[] zPositions = { 8f, 17f, 29f, 40f, 50f };

            foreach (float z in zPositions)
            {
                int count = 5 + rng.Next(0, 4);
                for (int i = 0; i < count; i++)
                {
                    float x   = (float)(rng.NextDouble() - 0.5) * 4f;
                    float sz  = 0.04f + (float)rng.NextDouble() * 0.18f;
                    float szY = 0.03f + (float)rng.NextDouble() * 0.08f;

                    var go = GameObject.CreatePrimitive(
                        rng.NextDouble() > 0.5 ? PrimitiveType.Cube : PrimitiveType.Cylinder);
                    go.name = "Debris";
                    Destroy(go.GetComponent<Collider>());
                    go.transform.position = new Vector3(
                        x, szY * 0.5f, z + (float)(rng.NextDouble() - 0.5) * 2f);
                    go.transform.eulerAngles = new Vector3(
                        (float)(rng.NextDouble() * 360),
                        (float)(rng.NextDouble() * 360),
                        (float)(rng.NextDouble() * 360));
                    go.transform.localScale = new Vector3(sz, szY, sz * 0.7f);

                    // Unlit/Color: Quest 빌드에서 항상 동작, 핑크 없음
                    float c = 0.15f + (float)rng.NextDouble() * 0.15f;
                    var s   = Shader.Find("Unlit/Color") ?? Shader.Find("Mobile/Diffuse");
                    var mat = new Material(s);
                    mat.color = new Color(c, c * 0.92f, c * 0.85f);
                    go.GetComponent<MeshRenderer>().material = mat;
                }
            }
        }

        // ── 전역 먼지/연기 ────────────────────────────────────────
        void SpawnCorridorSmoke()
        {
            var go  = new GameObject("CorridorSmoke");
            go.transform.position = new Vector3(0f, 2f, 26f);
            var ps  = go.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.loop            = true;
            main.duration        = 5f;
            main.startLifetime   = new ParticleSystem.MinMaxCurve(6f, 12f);
            main.startSpeed      = new ParticleSystem.MinMaxCurve(0.02f, 0.12f);
            main.startSize       = new ParticleSystem.MinMaxCurve(0.4f, 1.2f);
            main.startColor      = new ParticleSystem.MinMaxGradient(
                new Color(0.55f, 0.50f, 0.45f, 0.25f),
                new Color(0.35f, 0.32f, 0.30f, 0.12f));
            main.gravityModifier = -0.03f;
            main.maxParticles    = 120;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startRotation   = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);

            var em = ps.emission;
            em.rateOverTime = 10f;

            var sh = ps.shape;
            sh.shapeType = ParticleSystemShapeType.Box;
            sh.scale     = new Vector3(5f, 3.5f, 50f);

            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.space   = ParticleSystemSimulationSpace.World;
            vel.x       = new ParticleSystem.MinMaxCurve(-0.05f, 0.05f);
            vel.y       = new ParticleSystem.MinMaxCurve(0f, 0.04f);

            var cov = ps.colorOverLifetime;
            cov.enabled = true;
            var g = new Gradient();
            g.SetKeys(
                new[] { new GradientColorKey(new Color(0.6f, 0.55f, 0.5f), 0f),
                        new GradientColorKey(new Color(0.3f, 0.28f, 0.25f), 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.35f, 0.15f),
                        new GradientAlphaKey(0.2f, 0.7f), new GradientAlphaKey(0f, 1f) });
            cov.color = new ParticleSystem.MinMaxGradient(g);

            var sov = ps.sizeOverLifetime;
            sov.enabled = true;
            sov.size = new ParticleSystem.MinMaxCurve(1f,
                new AnimationCurve(new Keyframe(0f, 0.3f), new Keyframe(0.3f, 1f), new Keyframe(1f, 1.5f)));

            // Quest 호환 머티리얼 명시
            var ren = go.GetComponent<ParticleSystemRenderer>();
            ren.material = new Material(Shader.Find("Sprites/Default"))
                           { color = new Color(0.6f, 0.55f, 0.5f, 0.3f) };
        }

        // ── 조명 깜빡임 ───────────────────────────────────────────
        IEnumerator FlickerLights()
        {
            Light[] lights = FindObjectsOfType<Light>();
            float[] originalIntensities = new float[lights.Length];
            for (int i = 0; i < lights.Length; i++)
                originalIntensities[i] = lights[i].intensity;

            while (true)
            {
                yield return new WaitForSeconds(Random.Range(2f, 6f));

                int flicks = Random.Range(3, 6);
                for (int f = 0; f < flicks; f++)
                {
                    float intensity = Random.Range(0f, 0.3f);
                    foreach (var l in lights)
                        if (l != null) l.intensity = intensity;
                    yield return new WaitForSeconds(flickerSpeed * Random.Range(0.5f, 1.5f));

                    foreach (var l in lights)
                        if (l != null) l.intensity = originalIntensities[System.Array.IndexOf(lights, l)];
                    yield return new WaitForSeconds(flickerSpeed);
                }
            }
        }
    }
}
