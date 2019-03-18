using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControls : MonoBehaviour
{
    public float speed;
    public float rotSpeed;
    private void OnValidate()
    {
        if (speed <= 0)
        {
            speed = .0001f;
        }
        if (rotSpeed <= 0)
        {
            rotSpeed = .0001f;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        float tempSpeed = speed;
        float tempRotSpeed = rotSpeed;
        if(Input.GetKey(KeyCode.Space))
        {
            tempSpeed *= 5;
        }
        Vector3 motion = Vector3.ProjectOnPlane(this.transform.right * Input.GetAxis("Horizontal") + this.transform.forward * Input.GetAxis("Depth"), new Vector3(0, 1, 0)).normalized;
        transform.Translate(new Vector3(0, -Input.GetAxis("Mouse ScrollWheel"), 0) * tempSpeed, Space.World);
        if (Input.GetMouseButton(2))
        {
            transform.Translate(
                Vector3.ProjectOnPlane(this.transform.right * Input.GetAxis("Mouse X") + this.transform.forward * Input.GetAxis("Mouse Y"), new Vector3(0, 1, 0)).normalized,
                Space.World
            );
        }
        transform.Translate(motion * tempSpeed * Time.deltaTime, Space.World);
        transform.Translate(new Vector3(0, Input.GetAxis("Vertical"), 0) * tempSpeed * Time.deltaTime, Space.World);
        transform.Rotate(new Vector3(0, Input.GetAxis("Rotational Vertical") * Time.deltaTime, 0) * rotSpeed, Space.World);
        transform.Rotate(new Vector3(Input.GetAxis("Rotational Horizontal") * Time.deltaTime, 0, 0) * rotSpeed);
    }
}
