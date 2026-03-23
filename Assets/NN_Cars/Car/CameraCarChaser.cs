using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCarChaser : MonoBehaviour
{
    [SerializeField]Rigidbody target;

    [SerializeField] float cameraLookDistance;
    [SerializeField] float cameraCarDistance;
    [SerializeField] float cameraElevation;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        Vector3 lookPosition;
        if (target.linearVelocity.magnitude < 8)
        {
            Vector3 offset = Vector3.ProjectOnPlane((transform.position-target.position),Vector3.up).normalized*cameraCarDistance+Vector3.up*cameraElevation;
            transform.position=Vector3.Lerp(transform.position, target.position + offset,Time.fixedDeltaTime*6f);
            lookPosition =target.position - transform.position;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookPosition), Time.deltaTime);
        }
        else
        {
            Vector3 offset = Vector3.ProjectOnPlane((transform.position - target.position), Vector3.up).normalized * cameraCarDistance + Vector3.up * cameraElevation;
            transform.position = Vector3.Lerp(transform.position, target.position + offset, Time.fixedDeltaTime * 6f);
            lookPosition = target.position+target.linearVelocity.normalized*cameraLookDistance- transform.position;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookPosition), Time.deltaTime*4);
        }

        
    }
}
