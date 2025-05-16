using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ZoomManager : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] float minOrthographicSize;
    [SerializeField] float sizeChangeSpeed;

    float startOrthographicSize;
    bool buttonPressed;

    public void OnPointerDown(PointerEventData eventData)
    {
        buttonPressed = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        buttonPressed = false;
    }

    void Start()
    {
        startOrthographicSize = Camera.main.orthographicSize;
    }

    void Update()
    {
        if (buttonPressed)
        {
            float sizeChange = sizeChangeSpeed * Screen.width * Time.deltaTime;

            if (EventSystem.current.currentSelectedGameObject.name == "ZoomIn Button")
            {
                if ((Camera.main.orthographicSize -= sizeChange) < minOrthographicSize)
                {
                    Camera.main.orthographicSize = minOrthographicSize;
                }
            }
            else
            {
                if ((Camera.main.orthographicSize += sizeChange) > startOrthographicSize)
                {
                    Camera.main.orthographicSize = startOrthographicSize;
                }
            }
        }
    }
}
