using UnityEngine;

namespace HW09
{
    public class DeteriorationManager : MonoBehaviour
    {
        void Start()
        {
            Debug.Log($"[DETER] Start() on '{gameObject.name}', childCount={transform.childCount}");

            for (int i = 0; i < transform.childCount; i++)
            {
                Transform c = transform.GetChild(i);
                Debug.Log($"[DETER]   child[{i}] = '{c.name}' active={c.gameObject.activeSelf}");
            }

            if (ManagerMain.Instance == null)
            {
                Debug.LogWarning("[DETER] ManagerMain.Instance == NULL → abort");
                return;
            }

            bool[] clear = ManagerMain.Instance.setClear;
            Debug.Log($"[DETER] setClear = [{(clear[0]?1:0)},{(clear[1]?1:0)},{(clear[2]?1:0)},{(clear[3]?1:0)}]");

            Activate("LightOff",       clear.Length > 0 && clear[0]);
            Activate("WallCrack",      clear.Length > 1 && clear[1]);
            Activate("WaterPuddle",    clear.Length > 2 && clear[2]);
            Activate("StructureTwist", clear.Length > 3 && clear[3]);
        }

        void Activate(string childName, bool shouldActivate)
        {
            Debug.Log($"[DETER] Activate '{childName}' shouldActivate={shouldActivate}");
            if (!shouldActivate) return;

            Transform child = transform.Find(childName);
            if (child != null)
            {
                child.gameObject.SetActive(true);
                Debug.Log($"[DETER] ✓ '{childName}' SetActive(true)");
            }
            else
            {
                // 이름이 다를 경우를 대비해 부분 일치로 재시도
                for (int i = 0; i < transform.childCount; i++)
                {
                    Transform c = transform.GetChild(i);
                    if (c.name.Contains(childName) || childName.Contains(c.name))
                    {
                        c.gameObject.SetActive(true);
                        Debug.LogWarning($"[DETER] '{childName}' 정확 매칭 실패, '{c.name}' 부분매칭으로 활성화");
                        return;
                    }
                }
                Debug.LogWarning($"[DETER] ✗ '{childName}' 자식을 찾을 수 없음");
            }
        }
    }
}
