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
        Quaternion invRot = Quaternion.Euler(-transform.eulerAngles.x, 0, -transform.eulerAngles.z);
        transform.Translate(invRot * new Vector3(0, -Input.GetAxis("Mouse ScrollWheel"), 0) * tempSpeed);
        if (Input.GetMouseButton(2))
        {
            transform.Translate(invRot * new Vector3(Input.GetAxis("Mouse X"), 0, Input.GetAxis("Mouse Y")));
        }
        transform.Translate(invRot * new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Depth")) * tempSpeed * Time.deltaTime);
        transform.Translate(new Vector3(0, Input.GetAxis("Vertical"), 0) * tempSpeed * Time.deltaTime, Space.World);
        transform.Rotate(new Vector3(0, Input.GetAxis("Rotational Vertical") * Time.deltaTime, 0) * rotSpeed, Space.World);
        transform.Rotate(new Vector3(Input.GetAxis("Rotational Horizontal") * Time.deltaTime, 0, 0) * rotSpeed);
    }
}
