using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TouchControl : MonoBehaviour
{
    [SerializeField] GameObject controlsPanel;

    [SerializeField] float minOrthographicSize;
    [SerializeField] float scale;

    Vector3 startCameraPosition;
    float startOrthographicSize;
    float lastSeparation;

    void Start()
    {
        startCameraPosition = Camera.main.transform.position;
        startOrthographicSize = Camera.main.orthographicSize;
    }

    void Update()
    {
        // touch control only available when contol panel is open and finger not over a button
        if (controlsPanel.gameObject.activeSelf && !ButtonDetector.isPointerOverButton)
        {
            switch (Input.touchCount)
            {
            // one finger for rotating
            case 1:
                transform.Rotate(Input.GetTouch(0).deltaPosition.y * 360/Screen.height,
                    -Input.GetTouch(0).deltaPosition.x * 360/Screen.width, 0, Space.World);
                break;
            // two-finger pinch for zooming in and out
            case 2:
                if (Input.GetTouch(0).phase == TouchPhase.Began ||
                    Input.GetTouch(1).phase == TouchPhase.Began)
                {
                    lastSeparation =
                        (Input.GetTouch(1).position - Input.GetTouch(0).position).magnitude;
                }
                else
                {
                    float separation =
                        (Input.GetTouch(1).position - Input.GetTouch(0).position).magnitude;
                    float deltaSeparation = separation - lastSeparation;
                    lastSeparation = separation;

                    if ((Camera.main.orthographicSize -=
                        deltaSeparation * 2*scale/Screen.height) < minOrthographicSize)
                    {
                        Camera.main.orthographicSize = minOrthographicSize;
                    }
                    else if (Camera.main.orthographicSize > startOrthographicSize)
                    {
                        Camera.main.orthographicSize = startOrthographicSize;
                    }
                }
                break;
            // three fingers for positioning, tracking the middle finger
            case 3:
                // adjust scaling by the degree of zooming
                Camera.main.transform.position -= (Vector3) Input.GetTouch(1).deltaPosition *
                    scale/Screen.height * Camera.main.orthographicSize/startOrthographicSize;
                break;
            }
        }
    }
}
