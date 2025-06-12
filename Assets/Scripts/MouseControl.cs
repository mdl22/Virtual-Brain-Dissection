using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MouseControl : MonoBehaviour
{
    [SerializeField] GameObject controlsPanel;

    [SerializeField] float minOrthographicSize;
    [SerializeField] float scale;

    Vector3 lastMousePosition;

    float startOrthographicSize;
    float lastSeparation;

    void Start()
    {
        startOrthographicSize = Camera.main.orthographicSize;
    }

    void Update()
    {
        if (Input.touchCount > 0)
        {
            GetComponent<UIManager>().MouseInput(false);
        }

        // mouse control only available when contol panel open and mouse not over a button
        if (controlsPanel.gameObject.activeSelf && !ButtonDetector.isPointerOverButton)
        {
            Vector3 dist = Input.mousePosition - lastMousePosition;
            lastMousePosition = Input.mousePosition;

            if (Input.GetMouseButton(0))                // left button for rotating
            {
                transform.Rotate(dist.y * 360f/Screen.height, -dist.x * 360f/Screen.width, 0,
                    Space.World);
            }
            else if (Input.GetMouseButton(2))           // middle button for zooming
            {
                Vector3 centreOfScreen = new Vector3(Screen.width/2, Screen.height/2, 0);

                if (Input.GetMouseButtonDown(2))
                {
                    lastSeparation = (centreOfScreen - Input.mousePosition).magnitude;
                }
                else
                {
                    float separation = (centreOfScreen - Input.mousePosition).magnitude;

                    TestZoomBoundaries((separation - lastSeparation) * scale/Screen.height);

                    lastSeparation = separation;
                }
            }
            else if (Input.mouseScrollDelta.y != 0)     // scroll wheel for zooming
            {            
                    TestZoomBoundaries(-Mathf.Sign(Input.mouseScrollDelta.y) * scale/50);
            }
            else if (Input.GetMouseButton(1))           // right button for positioning
            {            
                // adjust scaling by the degree of zooming
                Camera.main.transform.position -= dist * scale/Screen.height *
                    Camera.main.orthographicSize/startOrthographicSize;
            }
        }
    }

    void TestZoomBoundaries(float deltaSize)
    {
        if ((Camera.main.orthographicSize -= deltaSize) < minOrthographicSize)
        {
            Camera.main.orthographicSize = minOrthographicSize;
        }
        else if (Camera.main.orthographicSize > startOrthographicSize)
        {
            Camera.main.orthographicSize = startOrthographicSize;
        }
    }
}
