using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using HW09;

public static class MissingScriptFixer
{
    [MenuItem("EDEN/Fix: Remove All Missing Scripts")]
    public static void RemoveAllMissingScripts()
    {
        string[] scenePaths = {
            "Assets/HW_09/Scenes/MainScene.unity",
            "Assets/HW_09/Scenes/Scene1_A.unity",
            "Assets/HW_09/Scenes/Scene2_A.unity",
            "Assets/HW_09/Scenes/Scene2_B.unity",
            "Assets/HW_09/Scenes/Scene3_A.unity",
        };

        int total = 0;
        foreach (var path in scenePaths)
        {
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            int removed = 0;
            foreach (var go in Object.FindObjectsOfType<GameObject>(true))
            {
                int count = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
                if (count > 0)
                {
                    removed += count;
                    EditorUtility.SetDirty(go);
                }
            }
            if (removed > 0)
            {
                EditorSceneManager.SaveScene(scene);
                Debug.Log($"[{scene.name}] Missing Script {removed}개 제거 후 저장");
            }
            else
                Debug.Log($"[{scene.name}] ✅ Missing Script 없음");
            total += removed;
        }
        Debug.Log($"총 {total}개 Missing Script 제거 완료");
    }

    [MenuItem("EDEN/Fix: Reattach MainScene Scripts")]
    public static void ReattachMainScene()
    {
        EditorSceneManager.OpenScene("Assets/HW_09/Scenes/MainScene.unity", OpenSceneMode.Single);

        // GameManager → ManagerMain
        var gm = GameObject.Find("GameManager");
        if (gm != null && gm.GetComponent<ManagerMain>() == null)
        {
            gm.AddComponent<ManagerMain>();
            Debug.Log("[MainScene] GameManager → ManagerMain 추가");
        }

        // Door_01~04 / Trigger → DoorMain
        string[] doorNames = { "Door_01", "Door_02", "Door_03", "Door_04" };
        string[] targetScenes = { "Scene1_A", "Scene2_A", "Scene3_A", "Scene4_A" };
        var hub = GameObject.Find("=== SF HUB ===");
        if (hub != null)
        {
            for (int i = 0; i < doorNames.Length; i++)
            {
                var doorTf = hub.transform.Find(doorNames[i] + "/Trigger");
                if (doorTf != null && doorTf.GetComponent<DoorMain>() == null)
                {
                    var dm = doorTf.gameObject.AddComponent<DoorMain>();
                    dm.targetScene = targetScenes[i];
                    dm.triggerRadius = 2.5f;
                    EditorUtility.SetDirty(doorTf.gameObject);
                    Debug.Log($"[MainScene] {doorNames[i]}/Trigger → DoorMain (target={targetScenes[i]}) 추가");
                }
            }
        }

        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log("[MainScene] 저장 완료");
    }

    [MenuItem("EDEN/Fix: Reattach Scene1_A Scripts")]
    public static void ReattachScene1A()
    {
        // HW09.Gayoung 스크립트가 프로젝트에 없으므로 비활성화
        Debug.LogWarning("[Scene1_A] Gayoung 스크립트가 프로젝트에 없습니다. ReattachScene1A를 건너뜁니다.");
    }

    [MenuItem("EDEN/Fix: Reattach Scene3_A Scripts")]
    public static void ReattachScene3A()
    {
        EditorSceneManager.OpenScene("Assets/HW_09/Scenes/Scene3_A.unity", OpenSceneMode.Single);

        // Scene3AManager → SceneManager3A
        var mgr = GameObject.Find("Scene3AManager");
        if (mgr != null && mgr.GetComponent<HW09.Heejin.SceneManager3A>() == null)
        {
            mgr.AddComponent<HW09.Heejin.SceneManager3A>();
            EditorUtility.SetDirty(mgr);
            Debug.Log("[Scene3_A] Scene3AManager → SceneManager3A 추가");
        }

        // candle → CandleGrabTrigger3A
        var candle = GameObject.Find("candle");
        if (candle != null && candle.GetComponent<HW09.Heejin.CandleGrabTrigger3A>() == null)
        {
            candle.AddComponent<HW09.Heejin.CandleGrabTrigger3A>();
            EditorUtility.SetDirty(candle);
            Debug.Log("[Scene3_A] candle → CandleGrabTrigger3A 추가");
        }

        // candle (1) → CandleGrabTrigger3A
        var candle1 = GameObject.Find("candle (1)");
        if (candle1 != null && candle1.GetComponent<HW09.Heejin.CandleGrabTrigger3A>() == null)
        {
            candle1.AddComponent<HW09.Heejin.CandleGrabTrigger3A>();
            EditorUtility.SetDirty(candle1);
            Debug.Log("[Scene3_A] candle (1) → CandleGrabTrigger3A 추가");
        }

        // door → DoorTrigger3A
        var door = GameObject.Find("door");
        if (door != null && door.GetComponent<HW09.Heejin.DoorTrigger3A>() == null)
        {
            door.AddComponent<HW09.Heejin.DoorTrigger3A>();
            EditorUtility.SetDirty(door);
            Debug.Log("[Scene3_A] door → DoorTrigger3A 추가");
        }

        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log("[Scene3_A] 저장 완료");
    }

    // ─────────────────────────────────────────────────────────
    // Scene1_A 완전 복구 — HW09.Gayoung 스크립트 없어서 비활성화
    // ─────────────────────────────────────────────────────────
    [MenuItem("EDEN/Fix: Full Fix Scene1_A")]
    public static void FullFixScene1A()
    {
        Debug.LogWarning("[Scene1_A] Gayoung 스크립트가 프로젝트에 없습니다. FullFixScene1A를 건너뜁니다.");
    }

    // ─────────────────────────────────────────────────────────
    // Scene3_A 참조 연결: SceneManager3A ← door/candle/light/clips
    // ─────────────────────────────────────────────────────────
    [MenuItem("EDEN/Fix: Wire Scene3_A References")]
    public static void WireScene3AReferences()
    {
        EditorSceneManager.OpenScene("Assets/HW_09/Scenes/Scene3_A.unity", OpenSceneMode.Single);

        var mgrGo = GameObject.Find("Scene3AManager");
        if (mgrGo == null) { Debug.LogError("[Scene3_A] Scene3AManager 없음"); return; }

        var mgr = mgrGo.GetComponent<HW09.Heejin.SceneManager3A>();
        if (mgr == null) { Debug.LogError("[Scene3_A] SceneManager3A 컴포넌트 없음"); return; }

        // door / candle / candle(1) 연결
        var doorGo   = GameObject.Find("door");
        var candleGo = GameObject.Find("candle");
        var candle1Go = GameObject.Find("candle (1)");

        if (mgr.door   == null && doorGo   != null) { mgr.door   = doorGo;   Debug.Log("[Scene3_A] door 연결"); }
        if (mgr.candle == null && candleGo != null) { mgr.candle = candleGo; Debug.Log("[Scene3_A] candle 연결"); }
        if (mgr.candle1 == null && candle1Go != null) { mgr.candle1 = candle1Go; Debug.Log("[Scene3_A] candle(1) 연결"); }

        // candleLight: candle/Point 에 있는 Light
        if (mgr.candleLight == null && candleGo != null)
        {
            var lt = candleGo.GetComponentInChildren<Light>();
            if (lt != null) { mgr.candleLight = lt; Debug.Log("[Scene3_A] candleLight 연결"); }
        }
        if (mgr.candleLight1 == null && candle1Go != null)
        {
            var lt = candle1Go.GetComponentInChildren<Light>();
            if (lt != null) { mgr.candleLight1 = lt; Debug.Log("[Scene3_A] candleLight1 연결"); }
        }

        // AudioSource: Scene3AManager 에 이미 있는 AudioSource 연결
        if (mgr.audioSource == null)
        {
            var src = mgrGo.GetComponent<AudioSource>();
            if (src == null) src = mgrGo.AddComponent<AudioSource>();
            mgr.audioSource = src;
            Debug.Log("[Scene3_A] AudioSource 연결");
        }

        // 오디오 클립 연결 (heejo/Audio 폴더에서 탐색)
        string[] audioGuids = AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets/HW_09/heejo/Audio" });
        var audioClips = new System.Collections.Generic.List<AudioClip>();
        foreach (var g in audioGuids)
            audioClips.Add(AssetDatabase.LoadAssetAtPath<AudioClip>(AssetDatabase.GUIDToAssetPath(g)));

        if (audioClips.Count > 0 && mgr.clip1 == null) { mgr.clip1 = audioClips[0]; Debug.Log($"[Scene3_A] clip1 = {audioClips[0].name}"); }
        if (audioClips.Count > 1 && mgr.clip2 == null) { mgr.clip2 = audioClips[1]; Debug.Log($"[Scene3_A] clip2 = {audioClips[1].name}"); }
        // clip3~5는 현재 오디오 파일 없음 — 혜조가 추가 시 직접 연결

        // DoorTrigger3A.manager 연결
        if (doorGo != null)
        {
            var dt = doorGo.GetComponent<HW09.Heejin.DoorTrigger3A>();
            if (dt != null && dt.manager == null) { dt.manager = mgr; EditorUtility.SetDirty(doorGo); Debug.Log("[Scene3_A] DoorTrigger3A.manager 연결"); }
        }

        // CandleGrabTrigger3A.manager 연결
        foreach (var go in new[] { candleGo, candle1Go })
        {
            if (go == null) continue;
            var ct = go.GetComponent<HW09.Heejin.CandleGrabTrigger3A>();
            if (ct != null && ct.manager == null) { ct.manager = mgr; EditorUtility.SetDirty(go); Debug.Log($"[Scene3_A] {go.name} CandleGrabTrigger3A.manager 연결"); }
        }

        EditorUtility.SetDirty(mgrGo);
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log("[Scene3_A] Wire References 완료 — 저장됨");
    }

    // ─────────────────────────────────────────────────────────
    // Scene4_B: ExitDoor에 ExitDoorTrigger4B 추가 + 저장
    // ─────────────────────────────────────────────────────────
    [MenuItem("EDEN/Fix: Wire Scene4_B ExitDoor")]
    public static void WireScene4BExitDoor()
    {
        EditorSceneManager.OpenScene("Assets/HW_09/Scenes/Scene4_B.unity", OpenSceneMode.Single);

        var exitDoor = GameObject.Find("ExitDoor");
        if (exitDoor == null)
        {
            Debug.LogError("[Scene4_B] ExitDoor 오브젝트를 찾을 수 없습니다.");
            return;
        }

        // ExitDoorTrigger4B 추가 (없으면)
        var trigger = exitDoor.GetComponent<HW09.heejo.ExitDoorTrigger4B>();
        if (trigger == null)
        {
            trigger = exitDoor.AddComponent<HW09.heejo.ExitDoorTrigger4B>();
            trigger.targetScene = "MainScene";
            trigger.playerTag   = "Player";
            EditorUtility.SetDirty(exitDoor);
            Debug.Log("[Scene4_B] ExitDoor → ExitDoorTrigger4B 추가 (targetScene=MainScene)");
        }
        else
        {
            Debug.Log("[Scene4_B] ExitDoorTrigger4B 이미 존재");
        }

        // BoxCollider isTrigger 확인
        var col = exitDoor.GetComponent<BoxCollider>();
        if (col != null && !col.isTrigger)
        {
            col.isTrigger = true;
            EditorUtility.SetDirty(exitDoor);
            Debug.Log("[Scene4_B] ExitDoor BoxCollider.isTrigger = true");
        }

        // SceneIntroManager4B 확인 (없으면 추가)
        var introMgr = Object.FindObjectOfType<HW09.heejo.SceneIntroManager4B>();
        if (introMgr == null)
        {
            var mgrGo = GameObject.Find("SceneIntroManager") ?? new GameObject("SceneIntroManager");
            mgrGo.AddComponent<HW09.heejo.SceneIntroManager4B>();
            EditorUtility.SetDirty(mgrGo);
            Debug.Log("[Scene4_B] SceneIntroManager4B 추가");
        }

        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log("[Scene4_B] Wire ExitDoor 완료 — 저장됨");
    }

    [MenuItem("EDEN/Fix: Run All Fixes")]
    public static void RunAllFixes()
    {
        RemoveAllMissingScripts();
        ReattachMainScene();
        ReattachScene1A();
        ReattachScene3A();
        FullFixScene1A();
        WireScene3AReferences();
        WireScene4BExitDoor();
        Debug.Log("=== 전체 Missing Script 수정 완료 ===");
    }
}
