using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class UIManager : MonoBehaviour
{
    [SerializeField] GameObject modelButtonsParent;

    [SerializeField] Button controlsButton;
    [SerializeField] Button controlsExitButton;
    [SerializeField] Button resetButton;
    [SerializeField] Button areasButton;
    [SerializeField] Button backButton;

    [SerializeField] Image controlsPanel;
    [SerializeField] Image areasPanel;
    [SerializeField] Image fimbriaFornixPanel;
    [SerializeField] Image sharedInputOutputPanel;

    [SerializeField] TextMeshProUGUI mouseFunctionText;
    [SerializeField] TextMeshProUGUI touchFunctionText;
    [SerializeField] TextMeshProUGUI panelTitleText;
    [SerializeField] TextMeshProUGUI panelListText;
    [SerializeField] TextMeshProUGUI areaTitleText;
    [SerializeField] TextMeshProUGUI areaDescriptionText;

    List<GameObject> models = new List<GameObject>();
    Button[] modelButtons;

    Quaternion startRotation;

    Vector3 startPosition;
    Vector3 startCameraPosition;

    float startOrthographicSize;

    void Start()
    {
        GetComponent<MouseControl>().enabled = false;

        backButton.GetComponent<Button>().onClick.AddListener(() =>
            { ResetAreasPanel(true); });

        foreach (Transform child in transform)
        {
            models.Add(child.gameObject);
        }

        modelButtons = modelButtonsParent.GetComponentsInChildren<Button>();

        foreach (Button button in modelButtons)
        {
            button.onClick.AddListener(() => { ActivateModel(button, true); });
        }
        // on exit, activate WholeBrain model, make WholeBrain button non-interactable
        controlsExitButton.GetComponent<Button>().onClick.AddListener(() =>
            { ActivateModel(modelButtons[0], false); });

        controlsExitButton.GetComponent<Button>().onClick.AddListener(ResetModels);
        resetButton.GetComponent<Button>().onClick.AddListener(ResetModels);

        startPosition = transform.position;
        startRotation = transform.rotation;

        startCameraPosition = Camera.main.transform.position;
        startOrthographicSize = Camera.main.orthographicSize;
    }

    void ActivateModel(Button clickedButton, bool keepPanelsOpen)
    {
        string clickedName = clickedButton.name.Split()[0];

        foreach (Button button in modelButtons)
        {
            bool notClicked = clickedName != button.name.Split()[0];

            button.gameObject.SetActive(notClicked || !clickedName.Contains("Boolean"));
            button.interactable = notClicked;
        }

        foreach (GameObject model in models)
        {
            model.SetActive(model.name == clickedName);
        }

        fimbriaFornixPanel.gameObject.SetActive(false);
        sharedInputOutputPanel.gameObject.SetActive(false);

        ClosePanels(keepPanelsOpen);
    }

    public void ClosePanels(bool active)
    {
        controlsPanel.gameObject.SetActive(active);
        controlsButton.gameObject.SetActive(!active);

        areasPanel.gameObject.SetActive(false);
        foreach (Transform child in transform)
        {
           if (child.gameObject.activeSelf)
           {
               areasButton.gameObject.SetActive(
                   active && !child.gameObject.name.Contains("Boolean"));
               break;
           }
        }

        ResetAreasPanel(true);
    }

    public void ResetAreasPanel(bool active)
    {
        panelTitleText.gameObject.SetActive(active);
        panelListText.gameObject.SetActive(active);

        areaTitleText.gameObject.SetActive(!active);
        areaDescriptionText.gameObject.SetActive(!active);

        backButton.gameObject.SetActive(!active);
    }

    public void ResetModels()
    {
        transform.position = startPosition;
        transform.rotation = startRotation;

        Camera.main.transform.position = startCameraPosition;
        Camera.main.orthographicSize = startOrthographicSize;
    }

    public void MouseInput(bool mouseDetected)
    {
        GetComponent<MouseControl>().enabled = mouseDetected;
        GetComponent<TouchControl>().enabled = !mouseDetected;

        mouseFunctionText.gameObject.SetActive(mouseDetected);
        touchFunctionText.gameObject.SetActive(!mouseDetected);

        panelTitleText.text = string.Concat(mouseDetected ? "Click" : "Tap",
            " on brain to find the following areas:");
    }

    public void Exit()
    {
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }
}
