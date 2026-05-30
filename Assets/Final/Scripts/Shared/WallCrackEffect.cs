using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HW09
{
    /// <summary>
    /// LineRenderer 기반 에너지 균열 효과. Quest에서 완전 작동.
    /// 균열이 벽 위(천장)에서 아래(바닥)로 이어지는 수직 형태.
    /// isMaster=true 이면 복도 전체(양쪽 벽)에 14개 균열 자동 스폰.
    /// </summary>
    [RequireComponent(typeof(MeshRenderer))]
    public class WallCrackEffect : MonoBehaviour
    {
        [Header("균열 색상")]
        public Color coreColor  = new Color(1.0f, 0.95f, 0.7f, 1.0f);  // 중심: 밝은 황백
        public Color glowColor  = new Color(1.0f, 0.40f, 0.05f, 0.85f); // 중간: 오렌지
        public Color outerColor = new Color(0.8f, 0.10f, 0.00f, 0.35f); // 외곽: 붉은 후광

        [Header("균열 크기")]
        [Range(1.5f, 5.0f)] public float crackLength = 3.5f; // 천장~바닥 커버
        [Range(2, 6)]       public int   branches    = 3;    // 메인 + 좌우 가지

        [Header("포인트 라이트")]
        public bool  addLight       = true;
        public Color lightColor     = new Color(1f, 0.28f, 0.04f);
        public float lightIntensity = 2.5f;
        public float lightRange     = 4.0f;

        [Header("등장")]
        [Range(0f, 1.5f)] public float fadeInTime = 0.6f;

        [Header("복도 전체 분산")]
        public bool isMaster = true;
        [HideInInspector] public int crackSeed = 42;

        // 복도 균열 배치: (Y시작높이=천장, Z위치, 왼쪽벽여부, 스케일)
        // Y를 천장 근처(3.5~3.9)로 설정 → 균열이 위에서 아래로 뻗음
        static readonly (float y, float z, bool left, float s)[] Slots =
        {
            (3.8f,  5f, false, 0.9f), (3.6f,  9f, true,  1.0f),
            (3.9f, 13f, false, 1.1f), (3.7f, 17f, true,  0.9f),
            (3.5f, 21f, false, 1.0f), (3.8f, 25f, true,  1.2f),
            (3.6f, 29f, false, 0.85f),(3.9f, 33f, true,  1.1f),
            (3.7f, 37f, false, 1.2f), (3.8f, 41f, true,  1.0f),
            (3.5f, 44f, false, 0.9f), (3.9f, 47f, true,  1.1f),
            (3.6f, 50f, false, 1.0f), (3.8f, 52f, true,  0.8f),
        };

        void OnEnable()
        {
            GetComponent<MeshRenderer>().enabled = false;
            StartCoroutine(Run());
        }

        IEnumerator Run()
        {
            yield return null;
            // 마스터 오브젝트의 균열: 천장 위치에서 시작
            Vector3 masterPos = new Vector3(transform.position.x,
                                            3.8f,
                                            transform.position.z);
            BuildCrack(masterPos, true, 1f, crackSeed);

            if (!isMaster) yield break;

            yield return new WaitForSeconds(0.4f);
            var rng = new System.Random(7777);
            foreach (var (y, z, left, s) in Slots)
            {
                float wx = left ? -2.95f : 2.95f;
                float wy = y + (float)(rng.NextDouble() - 0.5f) * 0.15f; // 천장 근처 미세 변화
                BuildCrack(new Vector3(wx, wy, z), left, s, rng.Next(10, 9999));
                yield return new WaitForSeconds(0.12f);
            }
        }

        void BuildCrack(Vector3 origin, bool leftWall, float scale, int seed)
        {
            var rng  = new System.Random(seed);
            var root = new GameObject($"CrackGroup_{seed}");

            // 벽 로컬 축
            Vector3 normal = leftWall ? Vector3.right : Vector3.left;
            Vector3 right  = leftWall ? Vector3.forward : Vector3.back;
            Vector3 up     = Vector3.up;
            Vector3 base3D = origin + normal * 0.025f;

            var allLines = new List<List<Vector2>>();

            // branches 개 가지: 모두 -90°(아래) 방향 중심으로 펼침
            // spread를 좁게 → 수직에 가까운 균열
            for (int b = 0; b < branches; b++)
            {
                float spread = (branches - 1) * 18f; // 좌우 18°씩 퍼짐 (좁게)
                float baseAngle = -90f; // 아래 방향
                float angle = baseAngle
                    + (branches > 1 ? -spread * 0.5f + b * spread / (branches - 1) : 0f)
                    + Rand(rng, -8f, 8f);

                var pts = new List<Vector2> { Vector2.zero };
                Grow(rng, pts, allLines, Vector2.zero, angle,
                     crackLength * scale * Rand(rng, 0.85f, 1.15f), 3);
                allLines.Add(pts);
            }

            // 3단 레이어 렌더링
            foreach (var line in allLines)
            {
                if (line.Count < 2) continue;
                Vector3[] w = To3D(line, base3D, right, up);
                AddLineRenderer(root, w, outerColor, 0.10f * scale, 0.025f * scale, seed);
                AddLineRenderer(root, w, glowColor,  0.045f * scale, 0.012f * scale, seed + 1);
                AddLineRenderer(root, w, coreColor,  0.018f * scale, 0.005f * scale, seed + 2);
            }

            // 조명 (천장 균열 발광 효과)
            if (addLight && scale >= 0.85f)
            {
                var lg = new GameObject("Light");
                lg.transform.SetParent(root.transform);
                // 균열 중간 높이에 빛 배치
                lg.transform.position = origin + normal * 0.4f + Vector3.down * (crackLength * scale * 0.4f);
                var l = lg.AddComponent<Light>();
                l.type      = LightType.Point;
                l.color     = lightColor;
                l.intensity = lightIntensity * scale;
                l.range     = lightRange * scale;
            }

            StartCoroutine(FadeIn(root, fadeInTime));
        }

        void Grow(System.Random rng, List<Vector2> line, List<List<Vector2>> all,
                  Vector2 cur, float angleDeg, float len, int depth)
        {
            int   steps  = Mathf.Max(5, Mathf.RoundToInt(len / 0.08f));
            float segLen = len / steps;
            float angle  = angleDeg;
            bool  split  = false;

            for (int i = 0; i < steps; i++)
            {
                // 수직 방향 유지를 위해 변화폭 줄임 (±15°)
                angle += Rand(rng, -15f, 15f);
                // 방향이 너무 수평으로 빠지지 않도록 -90° 방향으로 당김
                angle = Mathf.Lerp(angle, -90f, 0.15f);

                float rad = angle * Mathf.Deg2Rad;
                cur += new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * segLen;
                // 벽 범위 클램프: 수평 ±1.5m, 수직 0~4m
                cur.x = Mathf.Clamp(cur.x, -1.5f, 1.5f);
                cur.y = Mathf.Clamp(cur.y, -3.8f, 0.1f); // 시작이 천장(y=0)이므로 아래로 최대 3.8m
                line.Add(cur);

                // 가지치기: 균열의 1/3~2/3 지점에서 사선으로 갈라짐
                if (!split && depth > 0 && i > steps / 4 && i < steps * 3 / 4
                    && rng.NextDouble() > 0.4f)
                {
                    split = true;
                    // 사선 방향: -60°~-120° (여전히 아래 방향 유지)
                    float sa = angle + (rng.NextDouble() > 0.5 ? 40f : -40f)
                               + Rand(rng, -10f, 10f);
                    var sub = new List<Vector2> { cur };
                    Grow(rng, sub, all, cur, sa, len * Rand(rng, 0.35f, 0.55f), depth - 1);
                    all.Add(sub);
                }
            }
        }

        static Vector3[] To3D(List<Vector2> pts2D, Vector3 origin, Vector3 right, Vector3 up)
        {
            var r = new Vector3[pts2D.Count];
            for (int i = 0; i < pts2D.Count; i++)
                r[i] = origin + right * pts2D[i].x + up * pts2D[i].y;
            return r;
        }

        void AddLineRenderer(GameObject root, Vector3[] pts,
                             Color color, float startW, float endW, int order)
        {
            var go = new GameObject("L");
            go.transform.SetParent(root.transform);

            var lr  = go.AddComponent<LineRenderer>();
            var mat = new Material(Shader.Find("Sprites/Default")) { color = color };
            lr.material          = mat;
            lr.useWorldSpace     = true;
            lr.positionCount     = pts.Length;
            lr.SetPositions(pts);
            lr.startWidth        = startW;
            lr.endWidth          = endW;
            lr.numCapVertices    = 5;
            lr.numCornerVertices = 5;
            lr.sortingOrder      = order;

            var g = new Gradient();
            g.SetKeys(
                new[] { new GradientColorKey(color, 0f), new GradientColorKey(color, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(color.a, 1f) });
            lr.colorGradient = g;
        }

        IEnumerator FadeIn(GameObject root, float duration)
        {
            var lrs     = root.GetComponentsInChildren<LineRenderer>(true);
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                foreach (var lr in lrs)
                {
                    var g  = lr.colorGradient;
                    var ak = g.alphaKeys;
                    if (ak.Length >= 2)
                    {
                        Color c  = lr.material.color;
                        float ta = c.a * t;
                        var ng   = new Gradient();
                        ng.SetKeys(g.colorKeys,
                            new[] { new GradientAlphaKey(0f, 0f),
                                    new GradientAlphaKey(ta, 1f) });
                        lr.colorGradient = ng;
                    }
                }
                yield return null;
            }
        }

        static float Rand(System.Random r, float mn, float mx)
            => mn + (float)r.NextDouble() * (mx - mn);
    }
}
