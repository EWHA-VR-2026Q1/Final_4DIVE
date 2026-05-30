using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// EDEN/Setup: Setup Scene2_B
/// Scene2_B.unity를 열고 필요한 컴포넌트를 자동 배치합니다.
/// </summary>
public static class Scene2BSetup
{
    [MenuItem("EDEN/Setup: Scene2_B")]
    public static void Setup()
    {
        string scenePath = "Assets/Final/Scenes/Scene2_B.unity";

        if (!System.IO.File.Exists(scenePath))
        {
            Debug.LogError("[Scene2_B] Scene2_B.unity 파일이 없습니다.");
            return;
        }

        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        // ── 1. Manager2B ──────────────────────────────────────────
        var mgrGo = GameObject.Find("Scene2B_Manager");
        if (mgrGo == null)
        {
            mgrGo = new GameObject("Scene2B_Manager");
            Debug.Log("[Scene2_B] Scene2B_Manager 생성");
        }

        var mgr = mgrGo.GetComponent<HW09.Manager2B>();
        if (mgr == null)
        {
            mgr = mgrGo.AddComponent<HW09.Manager2B>();
            Debug.Log("[Scene2_B] Manager2B 추가");
        }

        // AI AudioSource (없으면 추가)
        if (mgr.aiVoice == null)
        {
            var aiGo = GameObject.Find("AIVoice2B") ?? new GameObject("AIVoice2B");
            aiGo.transform.SetParent(mgrGo.transform);
            var src = aiGo.GetComponent<AudioSource>() ?? aiGo.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.spatialBlend = 0f;
            src.ignoreListenerVolume = true;   // AudioListener.volume=0 이어도 들려야 함
            mgr.aiVoice = src;
            EditorUtility.SetDirty(aiGo);
            Debug.Log("[Scene2_B] AIVoice2B AudioSource 생성");
        }

        // ── 3. 라디오 오브젝트 ────────────────────────────────────
        var radioGo = GameObject.Find("Radio_2B");
        if (radioGo == null)
        {
            radioGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            radioGo.name = "Radio_2B";
            radioGo.transform.position = new Vector3(0f, 0.5f, 3f);
            radioGo.transform.localScale = new Vector3(0.3f, 0.2f, 0.15f);

            // 라디오 머티리얼 (어두운 회색 = 낡은 라디오)
            var mat = new Material(Shader.Find("Unlit/Color")) { color = new Color(0.2f, 0.18f, 0.15f) };
            radioGo.GetComponent<MeshRenderer>().material = mat;

            Debug.Log("[Scene2_B] Radio_2B 플레이스홀더 생성 (원하는 라디오 프리팹으로 교체 가능)");
        }

        var radio2B = radioGo.GetComponent<HW09.Radio2B>();
        if (radio2B == null)
        {
            radio2B = radioGo.AddComponent<HW09.Radio2B>();
            Debug.Log("[Scene2_B] Radio2B 컴포넌트 추가");
        }

        // 라디오 AudioSource
        if (radio2B.radioSource == null)
        {
            var src = radioGo.GetComponent<AudioSource>() ?? radioGo.AddComponent<AudioSource>();
            src.playOnAwake        = false;
            src.spatialBlend       = 1f;   // 3D 사운드
            src.minDistance        = 0.5f;
            src.maxDistance        = 8f;
            src.ignoreListenerVolume = true;
            radio2B.radioSource    = src;
            EditorUtility.SetDirty(radioGo);
        }

        // 라디오 포인트 라이트
        if (radio2B.radioLight == null)
        {
            var lightGo = new GameObject("RadioLight");
            lightGo.transform.SetParent(radioGo.transform);
            lightGo.transform.localPosition = new Vector3(0f, 0.3f, 0f);
            var lt = lightGo.AddComponent<Light>();
            lt.type      = LightType.Point;
            lt.color     = new Color(0.9f, 0.7f, 0.3f);
            lt.intensity = 1.2f;
            lt.range     = 3f;
            radio2B.radioLight = lt;
            EditorUtility.SetDirty(lightGo);
            Debug.Log("[Scene2_B] RadioLight 추가");
        }

        // ── 4. 출구 트리거 ────────────────────────────────────────
        var exitGo = GameObject.Find("ExitTrigger2B");
        if (exitGo == null)
        {
            exitGo = new GameObject("ExitTrigger2B");
            exitGo.transform.position = new Vector3(0f, 1f, -8f);
            var col = exitGo.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size      = new Vector3(4f, 3f, 1f);
            col.enabled   = false;   // Manager2B가 활성화함
            exitGo.AddComponent<HW09.ExitTrigger2B>();
            Debug.Log("[Scene2_B] ExitTrigger2B 생성");
        }

        // Manager2B에 exitTrigger 연결
        if (mgr.exitTrigger == null)
        {
            var col = exitGo.GetComponent<Collider>();
            if (col != null)
            {
                mgr.exitTrigger = col;
                Debug.Log("[Scene2_B] Manager2B.exitTrigger 연결");
            }
        }

        // ── 5. Manager2A / ExitTrigger2A / Radio2A 제거 ──────────
        // Scene2_B는 Scene2_A 복제본 → 2A 컴포넌트 완전 삭제 (비활성화만으로는 루프 발생)
        foreach (var c in Object.FindObjectsOfType<HW09.Manager2A>())
        {
            EditorUtility.SetDirty(c.gameObject);
            Object.DestroyImmediate(c);
            Debug.Log("[Scene2_B] Manager2A 제거 완료");
        }
        foreach (var c in Object.FindObjectsOfType<HW09.ExitTrigger2A>())
        {
            EditorUtility.SetDirty(c.gameObject);
            Object.DestroyImmediate(c);
            Debug.Log("[Scene2_B] ExitTrigger2A 제거 완료");
        }
        foreach (var c in Object.FindObjectsOfType<HW09.Radio2A>())
        {
            EditorUtility.SetDirty(c.gameObject);
            Object.DestroyImmediate(c);
            Debug.Log("[Scene2_B] Radio2A 제거 완료");
        }

        // ── 저장 ─────────────────────────────────────────────────
        EditorUtility.SetDirty(mgrGo);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("=== [Scene2_B] 설정 완료 ===");
        Debug.Log("남은 작업 (Inspector에서):");
        Debug.Log("  1. Manager2B → hackerClip, aiClip_Final 오디오 클립 연결");
        Debug.Log("  2. Radio_2B → 원하는 라디오 프리팹으로 교체 (선택)");
        Debug.Log("  3. Build Settings에 Scene2_B 추가");
    }
}
