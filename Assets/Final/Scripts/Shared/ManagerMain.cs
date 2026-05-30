using UnityEngine;
using UnityEngine.SceneManagement;

namespace HW09
{
    public class ManagerMain : MonoBehaviour
    {
        public static ManagerMain Instance;

        [Header("클리어 상태")]
        public bool[] setClear = new bool[4];

        [Header("허브 환경 오브젝트 (MainScene 재로드 시 자동 탐색)")]
        public GameObject lightOff;
        public GameObject wallCrack;
        public GameObject waterPuddle;
        public GameObject structureTwist;

        [Header("엔딩 문")]
        [Tooltip("모든 Set 클리어 전 막혀있는 패널 (이름: EndingDoorClosed)")]
        public GameObject endingDoorClosed;
        [Tooltip("모든 Set 클리어 후 활성화되는 트리거 (이름: EndingDoorOpen)")]
        public GameObject endingDoorOpen;
        public string     endingSceneName = "EndingScene";

        [Header("복귀 음성 (인덱스 0~3 = 씬1~4 클리어 후)")]
        public AudioSource hubAudio;
        public AudioClip[] clearClips = new AudioClip[4];

        [Header("Player 프리팹 (없으면 자동 생성)")]
        public GameObject playerPrefab;

        private int _pendingClearIndex = -1;

        // ── Singleton ────────────────────────────────────────────
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                if (hubAudio == null)
                    hubAudio = gameObject.AddComponent<AudioSource>();
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        void OnEnable()  => SceneManager.sceneLoaded += OnSceneLoaded;
        void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

        // ── Scene Loaded ─────────────────────────────────────────
        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "MainScene")
            {
                RefindHubObjects();
                UpdateHubEnvironment();
                PlayClearClip();
                return;
            }

            // 서브씬: Player가 없으면 프리팹 생성
            var player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                if (playerPrefab != null)
                {
                    Instantiate(playerPrefab, Vector3.up * 2f, Quaternion.identity);
                    Debug.Log($"[GameManager] '{scene.name}' — Player 없어서 프리팹 생성");
                }
                else
                {
                    Debug.LogWarning($"[GameManager] ⚠️ '{scene.name}'에 Player 태그 오브젝트가 없습니다.");
                }
            }
        }

        // ── Hub Objects 재탐색 ───────────────────────────────────
        /// <summary>
        /// MainScene 재로드 후 DontDestroyOnLoad 오브젝트의 씬 참조가
        /// 무효화되므로 이름으로 다시 찾아서 재연결한다.
        /// </summary>
        void RefindHubObjects()
        {
            Debug.Log("[ManagerMain] RefindHubObjects() 시작");

            // Deterioration 부모는 active → GameObject.Find 가능
            // transform.Find 는 inactive 자식도 탐색 가능
            var det = GameObject.Find("Deterioration");
            if (det != null)
            {
                Debug.Log($"[ManagerMain] Deterioration 발견, childCount={det.transform.childCount}");
                for (int i = 0; i < det.transform.childCount; i++)
                    Debug.Log($"[ManagerMain]   child[{i}]='{det.transform.GetChild(i).name}'");

                lightOff       = Child(det, "LightOff")       ?? lightOff;
                wallCrack      = Child(det, "WallCrack")      ?? wallCrack;
                waterPuddle    = Child(det, "WaterPuddle")    ?? waterPuddle;
                structureTwist = Child(det, "StructureTwist") ?? structureTwist;

                Debug.Log($"[ManagerMain] 연결 결과: lightOff={lightOff?.name ?? "null"}, wallCrack={wallCrack?.name ?? "null"}, waterPuddle={waterPuddle?.name ?? "null"}, structureTwist={structureTwist?.name ?? "null"}");
            }
            else Debug.LogWarning("[ManagerMain] ✗ 'Deterioration' 오브젝트를 찾을 수 없음");

            // EndingDoor 는 Structure 하위
            var structure = GameObject.Find("Structure");
            if (structure != null)
            {
                endingDoorClosed = Child(structure, "EndingDoorClosed") ?? endingDoorClosed;
                endingDoorOpen   = Child(structure, "EndingDoorOpen")   ?? endingDoorOpen;
            }
        }

        static GameObject Child(GameObject parent, string name)
        {
            var t = parent.transform.Find(name);
            return t != null ? t.gameObject : null;
        }

        // ── Clear & Deterioration ────────────────────────────────
        public void ClearSet(int index)
        {
            if (index < 0 || index >= setClear.Length) return;
            setClear[index] = true;
            _pendingClearIndex = index;
            Debug.Log($"[EDEN] Set {index + 1} 클리어! 복도 붕괴 단계 {GetClearCount()}/4");
            UpdateHubEnvironment();
        }

        void PlayClearClip()
        {
            if (_pendingClearIndex < 0) return;
            if (hubAudio == null) return;
            AudioClip clip = (_pendingClearIndex < clearClips.Length)
                ? clearClips[_pendingClearIndex] : null;
            _pendingClearIndex = -1;
            if (clip == null) return;
            hubAudio.clip = clip;
            hubAudio.Play();
        }

        void UpdateHubEnvironment()
        {
            Debug.Log($"[ManagerMain] UpdateHubEnvironment setClear=[{(setClear[0]?1:0)},{(setClear[1]?1:0)},{(setClear[2]?1:0)},{(setClear[3]?1:0)}]");
            Debug.Log($"[ManagerMain]   refs: lightOff={lightOff?.name ?? "null"}, wallCrack={wallCrack?.name ?? "null"}, waterPuddle={waterPuddle?.name ?? "null"}, structureTwist={structureTwist?.name ?? "null"}");

            if (setClear[0]) SetActive(lightOff,       true);   // 조명 꺼짐
            if (setClear[1]) SetActive(wallCrack,      true);   // 벽 균열
            if (setClear[2]) SetActive(waterPuddle,    true);   // 바닥 물 고임
            if (setClear[3]) SetActive(structureTwist, true);   // 복도 뒤틀림

            bool allClear = setClear[0] && setClear[1] && setClear[2] && setClear[3];
            if (allClear)
            {
                SetActive(endingDoorClosed, false);  // 막힌 패널 제거
                SetActive(endingDoorOpen,   true);   // 엔딩 트리거 활성화
            }
        }

        static void SetActive(GameObject go, bool v)
        {
            if (go != null) go.SetActive(v);
        }

        // ── Helpers ───────────────────────────────────────────────
        public void LoadScene(string sceneName)   => SceneManager.LoadScene(sceneName);
        public void ReturnToHub()                 => SceneManager.LoadScene("MainScene");

        public int GetClearCount()
        {
            int count = 0;
            foreach (bool b in setClear) if (b) count++;
            return count;
        }
    }
}
