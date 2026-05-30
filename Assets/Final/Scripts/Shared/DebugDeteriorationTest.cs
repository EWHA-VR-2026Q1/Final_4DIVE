using System.Collections;
using UnityEngine;

namespace HW09
{
    /// <summary>
    /// [임시 테스트용] MainScene에서 붕괴 효과를 자동으로 순차 활성화합니다.
    /// 빌드 확인 후 GameManager 오브젝트에서 이 컴포넌트를 제거하세요.
    /// </summary>
    public class DebugDeteriorationTest : MonoBehaviour
    {
        [Header("테스트 설정")]
        [Tooltip("첫 번째 붕괴 효과 활성화까지 대기 시간 (초)")]
        public float startDelay = 5f;

        [Tooltip("각 붕괴 효과 사이 간격 (초)")]
        public float interval = 3f;

        [Tooltip("false로 끄면 테스트 비활성화")]
        public bool enableTest = true;

        void Start()
        {
            if (!enableTest) return;
            if (ManagerMain.Instance == null) return;

            // 이미 클리어된 씬이 있으면 테스트 스킵
            if (ManagerMain.Instance.GetClearCount() > 0) return;

            StartCoroutine(RunTest());
        }

        IEnumerator RunTest()
        {
            Debug.Log("[DEBUG] 붕괴 테스트 시작 — " + startDelay + "초 후 순차 활성화");
            yield return new WaitForSeconds(startDelay);

            for (int i = 0; i < 4; i++)
            {
                Debug.Log($"[DEBUG] ClearSet({i}) 호출");
                ManagerMain.Instance.ClearSet(i);
                yield return new WaitForSeconds(interval);
            }

            Debug.Log("[DEBUG] 붕괴 테스트 완료 — 4개 모두 활성화됨");
        }
    }
}
