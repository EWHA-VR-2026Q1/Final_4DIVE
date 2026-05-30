using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneFlow : MonoBehaviour
{
    [Header("씬 이름")]
    public string nextScene = "gurin_Scene01_A";

    [Header("트리거 범위")]
    public float triggerDistance = 2f;
    public Transform player;

    private bool triggered = false;

    void Update()
    {
        if (triggered) return;
        if (player == null) return;

        float dist = Vector3.Distance(player.position, transform.position);
        if (dist < triggerDistance)
        {
            triggered = true;
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextScene);
        }
    }
}
