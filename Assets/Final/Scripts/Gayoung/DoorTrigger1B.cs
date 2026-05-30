using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorTrigger : MonoBehaviour
{
    public string targetScene = "MainScene";

    void OnTriggerEnter(Collider other)
    {
        // 태그 대신 CharacterController로 플레이어 판별
        if (other.GetComponent<CharacterController>() != null)
        {
            SceneManager.LoadScene(targetScene);
        }
    }
}
