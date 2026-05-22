using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class ClickOnArea : MonoBehaviour
{
    [SerializeField] GameObject modelButtons;
    [SerializeField] GameObject areaButtons;

    [SerializeField] Button controlsExitButton;
    [SerializeField] Button resetButton;
    [SerializeField] Button areasExitButton;
    [SerializeField] Button backButton;
    [SerializeField] Button showButton;
    [SerializeField] Button findButton;

    [SerializeField] Image areasPanel;

    [SerializeField] TextMeshProUGUI panelListText;
    [SerializeField] TextMeshProUGUI areaTitleText;
    [SerializeField] TextMeshProUGUI areaDescriptionText;

    [SerializeField] Texture2D mask;
    [SerializeField] Texture2D[] emissionMaps;
    [SerializeField] TextAsset maskTable;

    [SerializeField] float flashPeriod;

    Material material;

    Dictionary<string, string[]> areas = new Dictionary<string, string[]>();
    Dictionary<string, Texture2D> maps = new Dictionary<string, Texture2D>();

    byte emissionIntensity = 0xBF;
    int bitPosition;            // starting from the most significant bit in bit string
    float elapsedTime;
    string bitString;
    bool inShowMode;

    void Start()
    {
        material = GetComponent<Renderer>().material;
        material.EnableKeyword("_EMISSION");

        SetUpEmissionMaps();

        List<Button> buttonList = new List<Button> {controlsExitButton, resetButton,
            areasExitButton, backButton, showButton, findButton};
        buttonList.AddRange(modelButtons.GetComponentsInChildren<Button>());
        foreach (Button button in buttonList)
        {
            button.onClick.AddListener(() => { SetEmissionColor(0, true); });
        }

        showButton.GetComponent<Button>().onClick.AddListener(ActivateAreaButtons);
        foreach (Button button in areaButtons.GetComponentsInChildren<Button>(true))
        {
            button.onClick.AddListener(() => { AreaButtonFunctionality(button); });
        }

        bitString = "";
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && areasPanel.gameObject.activeSelf &&
            !findButton.gameObject.activeSelf && !inShowMode)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            bool isNotOverUI = !EventSystem.current.IsPointerOverGameObject();
            if (isNotOverUI && Physics.Raycast(ray, out RaycastHit hit))
            {
                Color32 pixelColour = mask.GetPixel((int) (hit.textureCoord.x * mask.width),
                    (int) (hit.textureCoord.y * mask.height));

                     // Convert.ToString() removes leading zeroes
                     string rawBitString = Convert.ToString(pixelColour.g, 2);

                bitString = "";
                for (int bit = 0; bit < rawBitString.Length; bit++)
                {
                    int bitPosition = rawBitString.Length - bit;

                    if (bitPosition > maps.Count)
                    {
                        bitString += "0";
                    }
                    else
                    {
                        if (maps[(1 << bitPosition - 1).ToString()] == null ||
                            !areas.ContainsKey((1 << bitPosition - 1).ToString()))
                        {
                            bitString += "0";
                        }
                        else
                        {
                            bitString += rawBitString[bit];
                        }
                    }
                }

                if (bitString.Split('1').Length == 1)   // no emission map
                {
                    SetEmissionColor(0);

                    GetComponentInParent<UIManager>().ResetAreasPanel(true);
                }
                else
                {
                    string areasKey = (1 << bitString.Length - 1).ToString();

                    material.SetTexture("_EmissionMap", maps[areasKey]);
                    SetEmissionColor(emissionIntensity);

                    areaTitleText.text = areas[areasKey][0];
                    areaDescriptionText.text = areas[areasKey][1];

                    GetComponentInParent<UIManager>().ResetAreasPanel(false);
                }
            }
        }

        if (bitString.Split('1').Length > 2)    // area has parent area
        {
            for (int bit = bitPosition; bit < bitString.Length; bit++)
            {
                if (bitString[bit] == '1')
                {
                    string areasKey = (1 << bitString.Length - 1).ToString();
                    string mapKey = (1 << bitString.Length - 1 - bit).ToString();

                    material.SetTexture("_EmissionMap", maps[mapKey]);
                    if (bit == 0)
                    {
                        SetEmissionColor((byte) emissionIntensity);

                        areaDescriptionText.text = areas[areasKey][1];
                    }
                    else
                    {
                        SetEmissionColor((byte) 0x7F);

                        areaDescriptionText.text = string.Concat(areas[areasKey][1],
                            "\n\nParent region: ", areas[areasKey][2].ToLower());
                    }

                    bitPosition = bit;
                    break;
                }
                bitPosition = 0;    // reset as least significant bit is '0' and
            }                       // elapsed time is less than flash period

            if ((elapsedTime += Time.deltaTime) >= flashPeriod)
            {
                elapsedTime = 0;
                if (++bitPosition >= bitString.Length)
                {
                    bitPosition = 0;
                }
            }
        }
    }

    void SetUpEmissionMaps()
    {
        areas.Clear();
        maps.Clear();

        panelListText.text = "";
        foreach (string line in maskTable.text.Split("\n"))
        {
            string[] fields = line.Split("\t");
            
            if (line.Length > 0 && Char.IsDigit(fields[0][0]))  // ignore EOF, header
            {
                if (Char.IsLetter(fields[2][0]))    // ignore blank fields
                {
                    areas.Add(fields[0],            // value
                        new string[] {fields[2],    // name
                                      fields[3],    // description
                                      fields[4]});  // parent region
                    panelListText.text += string.Concat(fields[2], "\n\n");
                }

                maps.Add(fields[0], emissionMaps[maps.Count]);
            }
        }
    }

    void SetEmissionColor(byte intensity, bool resetBitString = false)
    {
        if (resetBitString)
        {
            bitString = "";
        }
        material.SetColor("_EmissionColor", new Color32(intensity, 0, 0, 0));

        inShowMode = false;
    }

    void ActivateAreaButtons()
    {
        Button[] buttonArray = areaButtons.GetComponentsInChildren<Button>(true);

        int i = 0;
        foreach (string area in panelListText.text.Remove(
            panelListText.text.Length - 2).Split("\n\n"))
        {
            buttonArray[i].GetComponentInChildren<TextMeshProUGUI>().text =
                panelListText.text.Split("\n\n")[i];
            buttonArray[i].GetComponentInChildren<TextMeshProUGUI>().fontSize =
                buttonArray[i].GetComponentInChildren<TextMeshProUGUI>().text.Length > 15 ?
                14.4f : 18;

            buttonArray[i++].gameObject.SetActive(true);
        }
    }

    void AreaButtonFunctionality(Button clicked)
    {
        foreach (Button button in areaButtons.GetComponentsInChildren<Button>())
        {
            button.interactable = clicked.name != button.name;
        }

        foreach (string areasKey in areas.Keys)
        {//Debug.Log((areas.Count, areas.Keys.Count, areasKey));
            if (areas[areasKey][0] == clicked.GetComponentInChildren<TextMeshProUGUI>().text)
            {//Debug.Log(clicked.GetComponentInChildren<TextMeshProUGUI>().text);
                material.SetTexture("_EmissionMap", maps[areasKey]);
                SetEmissionColor(emissionIntensity);

                areaTitleText.text = areas[areasKey][0];
                areaDescriptionText.text = areas[areasKey][2] == "N/A" ? areas[areasKey][1] :
                    string.Concat(areas[areasKey][1], "\n\nParent region: ",
                    areas[areasKey][2].ToLower());

                GetComponentInParent<UIManager>().ResetAreasPanel(false);

                inShowMode = true;      // set to false in SetEmissionColor
                break;
            }
        }
    }
}
