using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonDetector : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public static bool isPointerOverButton;

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOverButton = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOverButton = false;
    }

    public void SetIsPointerOverButtonFalse()
    {
        isPointerOverButton = false;
    }
}
