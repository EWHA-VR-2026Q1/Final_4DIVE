
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorOpen1A : MonoBehaviour
{
    public Transform doorLeft;
    public Transform doorRight;
    public float openDistance = 50f;
    public float speed = 2f;
    public float triggerDistance = 3f;
    public Transform player;

    private Vector3 leftClosedPos;
    private Vector3 rightClosedPos;
    private Vector3 leftOpenPos;
    private Vector3 rightOpenPos;
    private bool isOpen = false;
    private Transform _eyeAnchor;

    void Start()
    {
        leftClosedPos = doorLeft.localPosition;
        rightClosedPos = doorRight.localPosition;
        leftOpenPos = leftClosedPos + Vector3.left * openDistance;
        rightOpenPos = rightClosedPos + Vector3.right * openDistance;

        // VR Quest: OVRCameraRig의 실제 눈 위치를 우선 사용
        var rig = FindFirstObjectByType<OVRCameraRig>();
        if (rig != null) _eyeAnchor = rig.centerEyeAnchor;
    }

    void Update()
    {
        Transform checkTarget = _eyeAnchor != null ? _eyeAnchor : player;
        if (checkTarget == null) return;

        // Y 무시, 수평 거리만 체크 (스케일된 씬에서 높이 차이로 오작동 방지)
        Vector3 doorXZ   = new Vector3(transform.position.x,    0, transform.position.z);
        Vector3 playerXZ = new Vector3(checkTarget.position.x,  0, checkTarget.position.z);
        float dist = Vector3.Distance(doorXZ, playerXZ);
        isOpen = dist < triggerDistance;

        if (isOpen)
        {
            doorLeft.localPosition = Vector3.Lerp(doorLeft.localPosition, leftOpenPos, Time.deltaTime * speed);
            doorRight.localPosition = Vector3.Lerp(doorRight.localPosition, rightOpenPos, Time.deltaTime * speed);
        }
        else
        {
            doorLeft.localPosition = Vector3.Lerp(doorLeft.localPosition, leftClosedPos, Time.deltaTime * speed);
            doorRight.localPosition = Vector3.Lerp(doorRight.localPosition, rightClosedPos, Time.deltaTime * speed);
        }
    }
}
