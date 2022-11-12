using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityExtensions;

public class CameraControls : MonoBehaviour
{

    public Vector2 cameraFieldOfViewLimits;
    public float slideSpeed=1;
    public float zoomSpeed=1;
    public float dragActivationPixels = 30;
    public string state = "None";
    public Vector2 position;
    
    void Start()
    {
        InputPanel.Instance.OnPointerDownFullInfo.AddListener(Down);
        InputPanel.Instance.OnDragFullInfo.AddListener(Drag);
        InputPanel.Instance.OnPointerUpFullInfo.AddListener(Up);
    }


    void Down(PointerEventData data)
    {
        if (state == "None")
        {
            state = "Listening";
            position = data.position;
        }
    }

    void Drag(PointerEventData data)
    {
        if (state == "Listening")
        {
            if (dragActivationPixels < Mathf.Abs(position.x - data.position.x))
            {
                state = "Horizontal";
                UIM.Instance.TriggerCamera();
            }

            else if (dragActivationPixels < Mathf.Abs(position.y - data.position.y))
            {
                UIM.Instance.TriggerCamera();
                state = "Vertical";
            }   
        }
        
        if (state == "Horizontal")
            transform.parent.localEulerAngles += new Vector3(0, data.delta.x* slideSpeed, 0);
        else if (state == "Vertical")
            GetComponent<Camera>().fieldOfView = Mathf.Clamp(GetComponent<Camera>().fieldOfView + data.delta.y * zoomSpeed,
                cameraFieldOfViewLimits.x, cameraFieldOfViewLimits.y);
    }

    void Up(PointerEventData data)
    {
        state = "None";
    }
    
    
}