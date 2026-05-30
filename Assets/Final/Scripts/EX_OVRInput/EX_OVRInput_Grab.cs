using UnityEngine;

/// <summary>
/// EX_OVRInput_Grab
///
/// 잡기 방식 2가지:
/// 1. 근접 잡기  : 손을 오브젝트에 직접 가져다 대고 LHandTrigger 누르기
/// 2. 레이 잡기  : LIndexTrigger(선 발사) + LHandTrigger 동시에 누르면 레이가 맞은 오브젝트를 잡음
///
/// 주의: 잡힐 오브젝트에 Rigidbody + Collider 가 반드시 있어야 합니다.
///       GrabLayer 에 해당 오브젝트 레이어가 포함되어야 합니다.
///       GrabLayer 가 Nothing(0) 이면 자동으로 모든 레이어를 사용합니다.
///
/// [Scene3 연동] 오브젝트에 CandleGrabTrigger 컴포넌트가 있으면 grab 즉시 TriggerGrab() 자동 호출
/// </summary>
public class EX_OVRInput_Grab : MonoBehaviour
{
    [Header("Hand")]
    public Transform LeftHand;

    [Header("Grab (근접)")]
    public float grabRadius = 0.2f;
    public LayerMask GrabLayer;

    [Header("Ray Grab (원거리 레이 잡기)")]
    [Tooltip("true : LIndexTrigger(선) + LHandTrigger 동시에 누르면 레이로 잡음")]
    public bool enableRayGrab = true;
    [Tooltip("레이 잡기 최대 거리")]
    public float rayGrabDistance = 10f;
    [Tooltip("레이로 잡으면 손 앞 이 거리에 오브젝트를 가져다 놓음 (0 = 원위치 유지)")]
    public float rayGrabPullToHandOffset = 0.4f;

    // ── 내부 상태 ────────────────────────────────
    Rigidbody    GrabbedRB;
    Collider     PlayerCollider;
    Collider     ObjectCollider;

    /// <summary>현재 잡고 있는 Rigidbody (없으면 null). CandleGrabTrigger 등 외부에서 폴링 가능.</summary>
    public Rigidbody CurrentGrabbedObject => GrabbedRB;

    Vector3    PosOffset;
    Quaternion RotOffset;

    bool _prevGrab = false;

    // ─────────────────────────────────────────────
    void Start()
    {
        // CharacterController 또는 Capsule Collider 찾기
        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null)
            PlayerCollider = cc;
        else
            PlayerCollider = GetComponent<Collider>();

        // GrabLayer 가 Nothing(0) 이면 모든 레이어 사용
        if (GrabLayer.value == 0)
        {
            GrabLayer = ~0;
            Debug.LogWarning("[EX_OVRInput_Grab] GrabLayer 가 Nothing 으로 설정되어 모든 레이어를 사용합니다. 필요 시 Inspector 에서 특정 레이어를 선택하세요.");
        }
    }

    // ─────────────────────────────────────────────
    void Update()
    {
        bool grab  = OVRInput.Get(OVRInput.RawButton.LHandTrigger);
        bool index = OVRInput.Get(OVRInput.RawButton.LIndexTrigger);
        bool grabDown = grab && !_prevGrab; // 이번 프레임에 새로 눌림
        _prevGrab = grab;

        if (grab)
        {
            if (GrabbedRB == null)
            {
                // ── 레이 잡기 (인덱스 트리거로 선 발사 중일 때, 새로 누른 순간) ──
                if (enableRayGrab && index && grabDown)
                    TryRayGrab();

                // ── 근접 잡기 ──
                if (GrabbedRB == null)
                    TryGrab();
            }
            else
            {
                UpdateGrab();
            }
        }
        else
        {
            if (GrabbedRB != null)
                Release();
        }
    }

    // ─────────────────────────────────────────────
    // 근접 잡기
    // ─────────────────────────────────────────────
    void TryGrab()
    {
        if (LeftHand == null) return;

        Collider[] hits = Physics.OverlapSphere(LeftHand.position, grabRadius, GrabLayer);
        if (hits.Length == 0) return;

        float    minDist = float.MaxValue;
        Rigidbody closest = null;

        foreach (Collider c in hits)
        {
            Rigidbody rb = c.attachedRigidbody;
            if (rb == null) continue;

            float d = Vector3.Distance(LeftHand.position, rb.position);
            if (d < minDist)
            {
                minDist  = d;
                closest = rb;
            }
        }

        if (closest != null)
        {
            AttachGrab(closest, useHandOffset: true);
        }
    }

    // ─────────────────────────────────────────────
    // 레이 잡기
    // ─────────────────────────────────────────────
    void TryRayGrab()
    {
        if (LeftHand == null) return;

        Vector3 start = LeftHand.position + LeftHand.forward * 0.04f;
        Vector3 dir   = LeftHand.forward;

        if (!Physics.Raycast(start, dir, out RaycastHit hit, rayGrabDistance, GrabLayer))
        {
            Debug.Log("[EX_OVRInput_Grab] 레이 잡기: 레이에 맞는 오브젝트가 없습니다.");
            return;
        }

        Rigidbody rb = hit.collider.attachedRigidbody;
        if (rb == null)
        {
            Debug.LogWarning($"[EX_OVRInput_Grab] 레이 잡기: {hit.collider.name} 에 Rigidbody가 없습니다. Rigidbody를 추가해야 잡을 수 있습니다.");
            return;
        }

        Debug.Log($"[EX_OVRInput_Grab] 레이 잡기 성공: {rb.name}");

        // 먼저 Attach (isKinematic 설정 포함)
        GrabbedRB      = rb;
        ObjectCollider = GrabbedRB.GetComponent<Collider>();
        GrabbedRB.isKinematic = true;

        if (ObjectCollider != null && PlayerCollider != null)
            Physics.IgnoreCollision(ObjectCollider, PlayerCollider, true);

        // 목표 위치 결정 (손 앞 당기기 or 원위치)
        Vector3 targetPos = (rayGrabPullToHandOffset > 0f)
            ? LeftHand.position + LeftHand.forward * rayGrabPullToHandOffset
            : rb.position;

        // ★ 목표 위치 기준으로 offset 계산 (이 순서여야 손이 움직일 때 캔들이 올바르게 따라옴)
        PosOffset = Quaternion.Inverse(LeftHand.rotation) * (targetPos - LeftHand.position);
        RotOffset = Quaternion.Inverse(LeftHand.rotation) * GrabbedRB.rotation;

        // 목표 위치로 이동
        GrabbedRB.MovePosition(targetPos);

        // Scene3 캔들 등 CandleGrabTrigger가 붙어 있으면 이벤트 전달
    }

    // ─────────────────────────────────────────────
    // 공통 Attach 로직 (근접 잡기용)
    // ─────────────────────────────────────────────
    void AttachGrab(Rigidbody rb, bool useHandOffset)
    {
        GrabbedRB        = rb;
        ObjectCollider   = GrabbedRB.GetComponent<Collider>();
        GrabbedRB.isKinematic = true;

        if (ObjectCollider != null && PlayerCollider != null)
            Physics.IgnoreCollision(ObjectCollider, PlayerCollider, true);

        PosOffset = Quaternion.Inverse(LeftHand.rotation) * (GrabbedRB.position - LeftHand.position);
        RotOffset = Quaternion.Inverse(LeftHand.rotation) * GrabbedRB.rotation;
    }

    // ─────────────────────────────────────────────
    // Grab 유지 (매 프레임 오브젝트를 손에 붙여놓음)
    // ─────────────────────────────────────────────
    void UpdateGrab()
    {
        if (LeftHand == null || GrabbedRB == null) return;

        Vector3    targetPos = LeftHand.position + LeftHand.rotation * PosOffset;
        Quaternion targetRot = LeftHand.rotation * RotOffset;

        GrabbedRB.MovePosition(targetPos);
        GrabbedRB.MoveRotation(targetRot);
    }

    // ─────────────────────────────────────────────
    // 놓기
    // ─────────────────────────────────────────────
    void Release()
    {
        GrabbedRB.isKinematic = false;

        // 던지기 속도 전달
        GrabbedRB.velocity        = OVRInput.GetLocalControllerVelocity(OVRInput.Controller.LTouch);
        GrabbedRB.angularVelocity = OVRInput.GetLocalControllerAngularVelocity(OVRInput.Controller.LTouch);

        if (ObjectCollider != null && PlayerCollider != null)
            Physics.IgnoreCollision(ObjectCollider, PlayerCollider, false);

        GrabbedRB      = null;
        ObjectCollider = null;
    }


    // ─────────────────────────────────────────────
    // Gizmos
    // ─────────────────────────────────────────────
    void OnDrawGizmos()
    {
        if (LeftHand == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(LeftHand.position, grabRadius);

        if (enableRayGrab)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(LeftHand.position, LeftHand.forward * rayGrabDistance);
        }
    }
}
