using System.Collections;
using UnityEngine;

namespace HW09.heejo
{
    public class SceneManager4A : MonoBehaviour
    {
        public float startDelay = 1f;

        void Start()
        {
            StartCoroutine(PlaySequentialAudio());
        }

        IEnumerator PlaySequentialAudio()
        {
            yield return new WaitForSeconds(startDelay);

            string[] sourceNames = { "AudioAI_4A", "AudioNPC1_4A", "AudioNPC2_4A", "AudioNPC3_4A" };

            foreach (string sourceName in sourceNames)
            {
                GameObject go = GameObject.Find(sourceName);
                if (go == null) { Debug.LogWarning($"[SceneManager4A] '{sourceName}' not found."); continue; }

                AudioSource src = go.GetComponent<AudioSource>();
                if (src == null) { Debug.LogWarning($"[SceneManager4A] No AudioSource on '{sourceName}'."); continue; }
                if (src.clip == null) { Debug.LogWarning($"[SceneManager4A] No clip assigned on '{sourceName}'."); continue; }

                Debug.Log($"[SceneManager4A] Playing {sourceName} ({src.clip.length:F1}s)");
                src.Play();
                yield return new WaitForSeconds(src.clip.length);
                Debug.Log($"[SceneManager4A] Finished {sourceName}");
            }

            Debug.Log("[SceneManager4A] All audio done → activating door glow");
            ActivateDoor();
        }

        void ActivateDoor()
        {
            GameObject door = GameObject.Find("Door");
            if (door == null) { Debug.LogWarning("[SceneManager4A] 'Door' not found in scene."); return; }

            ExitDoor4A exitDoor = door.GetComponent<ExitDoor4A>();
            if (exitDoor != null)
                exitDoor.Activate();
            else
                Debug.LogWarning("[SceneManager4A] ExitDoor4A component not found on Door.");
        }
    }
}
