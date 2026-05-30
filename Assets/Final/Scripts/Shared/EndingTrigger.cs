using UnityEngine;

namespace HW09
{
    /// <summary>
    /// 엔딩 문 통과 시 EndingScene 으로 전환.
    /// ManagerMain.UpdateHubEnvironment() 에서 활성화된 뒤에만 동작.
    /// </summary>
    public class EndingTrigger : MonoBehaviour
    {
        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            if (ManagerMain.Instance != null)
                ManagerMain.Instance.LoadScene(ManagerMain.Instance.endingSceneName);
            else
                UnityEngine.SceneManagement.SceneManager.LoadScene("OutroScene");
        }
    }
}
