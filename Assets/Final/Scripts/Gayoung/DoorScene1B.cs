using UnityEngine;

public class CompassGrabTrigger : MonoBehaviour
{
    public GameObject door;
    private EX_OVRInput_Grab _grabber;
    private Rigidbody _rb;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        door.SetActive(false);
        _grabber = FindFirstObjectByType<EX_OVRInput_Grab>();
    }

    void Update()
    {
        if (_grabber != null && _grabber.CurrentGrabbedObject == _rb)
            door.SetActive(true);
    }
}
