using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;


public class TimpsShotMain : MonoBehaviour
{
// the shader used for silhouettes
    public Shader silhoutteMat;
    //reference to the Object holder
    GameObject objectHolder;
    private GameObject crosshairs;
    //A count of how many children it has. Calculated on start()
    int modelChildCount;
    //Current position in the list of models to cycle through
    int currentChildListPosition;
    //reference to the currently selected model. Used to apply scale and rotation
    GameObject currentChildModel;
    //Reference to the camera
    private Camera iconCamera;
    private Camera silhouetteCamera;

    

    private RenderTexture myRenderTexture;
    //reference to the meshrenderer on the current selected child. Used to find the centre of the object for the initial setup phase
    Renderer mMeshRenderer;
    Vector3 mCenter;
    Vector3 mWorldCenter;
    //The new empty object created for each child in the setup phase
    GameObject newParentEmpty;
    //the 512px item frame and it's centre
    RectTransform itemframeReference;
    Vector2 itemframeCenter;

    //Reference to the master Canvas
    GameObject timpsShotCanvas;

    //UI Components
    //The sliders
    private Slider sizeSlider;
    private Slider ccSlider;
    private Slider panSlider;
    private Slider tumbleSlider;
    private Slider vOffset;
    private Slider hOffset;
    private InputField dimensions;
    private InputField filePrefix;
    private InputField fileSuffix;
    private Toggle prefixToggle;
    private Toggle suffixToggle;
    private Toggle silhouetteToggle;
    private Toggle iconSizeToggle;
    private Toggle randomToggle;
    private InputField saveFolderText;
    private Text fileNameSample;
    

    //Light Groups
    GameObject lightGroup1;
    GameObject lightGroup2;
    GameObject lightGroup3;
    GameObject lightGroup4;

    
    private string filenamePrefix;
    private string filenameSuffix;
    private string filenameSample;

    // Start is called before the first frame update
    void Start()
    {
        //First we find and store references to the things we need
        objectHolder = GameObject.Find("ObjectHolder");
        crosshairs = GameObject.Find("TargetingLines");
        sizeSlider = GameObject.Find("SizeSlider").GetComponent<Slider>();
        ccSlider = GameObject.Find("CCSlider").GetComponent<Slider>();
        tumbleSlider = GameObject.Find("TumbleSlider").GetComponent<Slider>();
        panSlider = GameObject.Find("PanSlider").GetComponent<Slider>();
        vOffset = GameObject.Find("VOffset").GetComponent<Slider>();
        hOffset = GameObject.Find("HOffset").GetComponent<Slider>();
        timpsShotCanvas = GameObject.Find("TimpsShotCanvas");
        iconCamera = GameObject.Find("ShotCam").GetComponent<Camera>();
        if (iconCamera != null) iconCamera.transform.gameObject.SetActive(false);
        silhouetteCamera = GameObject.Find("SilhouetteCamera").GetComponent<Camera>();
        silhouetteCamera.SetReplacementShader (silhoutteMat, "");
        if (silhouetteCamera != null) silhouetteCamera.transform.gameObject.SetActive(false);
        dimensions = GameObject.Find("Dimensions").GetComponent<InputField>();
        filePrefix = GameObject.Find("Prefixfield").GetComponent<InputField>();
        fileSuffix = GameObject.Find("SuffixField").GetComponent<InputField>();
        saveFolderText = GameObject.Find("OutputPath").GetComponent<InputField>();
        prefixToggle = GameObject.Find("PrefixToggle").GetComponent<Toggle>();
        suffixToggle = GameObject.Find("SuffixToggle").GetComponent<Toggle>();
        silhouetteToggle = GameObject.Find("SilhouetteToggle").GetComponent<Toggle>();
        iconSizeToggle = GameObject.Find("IconSizeToggle").GetComponent<Toggle>();
        randomToggle = GameObject.Find("RandomStringToggle").GetComponent<Toggle>();
        fileNameSample = GameObject.Find("NameSample").GetComponent<Text>();
        

        lightGroup1 = GameObject.Find("LightGroup1");
        lightGroup2 = GameObject.Find("LightGroup2");
        lightGroup3 = GameObject.Find("LightGroup3");
        lightGroup4 = GameObject.Find("LightGroup4");

        //turn on primary lights
        lightGroup1.SetActive(true);
        lightGroup2.SetActive(false);
        lightGroup3.SetActive(false);
        lightGroup4.SetActive(false);




        itemframeReference = GameObject.Find("itemframe").GetComponent<RectTransform>();
        

        //And then we calculate the corner point of the frame and count the children
        itemframeCenter = itemframeReference.anchorMin;
        modelChildCount = objectHolder.transform.childCount;
        if (modelChildCount == 0)
        {
            Debug.Log("No GameObjects attached to the Object Holder. Objects need to be attached before entering play mode. Adding a cube for you");
            GameObject empty = new GameObject();
            empty.name = "0";
            empty.transform.parent = objectHolder.transform;
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.parent = empty.transform;
            
        }
        
        //for each child object we find the centre of the object bounds, create an empty GameObject and attach the object to it.
        //This moves the pivot point of each object dead centre for the clockwise and counterclockwise rotations
        for (int i = 0; i < modelChildCount; i++)
        {
            mMeshRenderer = objectHolder.transform.GetChild(i).GetComponent<Renderer>();
            if (mMeshRenderer == null)
            {
                Renderer foundRenderer = FindRendererInChildren(objectHolder.transform.GetChild(i));
                if (foundRenderer == null)
                {
                    DestroyImmediate(objectHolder.transform.GetChild(i).gameObject);
                    return;
                }
                else
                {
                    mMeshRenderer = foundRenderer;
                }

            }
            mWorldCenter = mMeshRenderer.bounds.center;
            newParentEmpty = new GameObject();
            newParentEmpty.name = i.ToString();
            newParentEmpty.transform.position = mWorldCenter;
            objectHolder.transform.GetChild(i).transform.SetParent(newParentEmpty.transform);
            newParentEmpty.transform.SetParent(objectHolder.transform);
            newParentEmpty.transform.SetSiblingIndex(i);
            mWorldCenter = transform.TransformPoint(mCenter);
            newParentEmpty.transform.position = new Vector3(0, 0, 0);                     
            newParentEmpty.SetActive(false);
        }

        objectHolder.transform.GetChild(0).gameObject.SetActive(true);
        currentChildModel = objectHolder.transform.GetChild(0).gameObject;
        }

    private Renderer FindRendererInChildren(Transform parent)
    {
        Renderer childRenderer = null;

        foreach (Transform child in parent)
        {
            childRenderer = child.GetComponent<Renderer>();

            if (childRenderer != null && child.gameObject.activeSelf)
            {
                // Renderer found in an active gameObject
                return childRenderer;
            }

            // Recursively search children
            childRenderer = FindRendererInChildren(child);

            if (childRenderer != null)
            {
                // Renderer found in a child's subtree
                return childRenderer;
            }
        }

        // No active gameObject with a renderer found
        return null;
    }

    private void BuildSampleName()
    {
        filenameSample =
            (prefixToggle.isOn ? "Prefix_" : "") +
            "Model" +
            (iconSizeToggle.isOn ? "256" : "") +
            (suffixToggle.isOn ? "_Suffix" : "") +
            (randomToggle.isOn ? "9876" : "");
        fileNameSample.text = filenameSample;
    }

    public void ToggleCrosshairs()
    {
        if (crosshairs != null) 
        {
            crosshairs.SetActive(!crosshairs.activeSelf);
        }
    }
    public void Light1()
    {
        lightGroup1.SetActive(true);
        lightGroup2.SetActive(false);
        lightGroup3.SetActive(false);
        lightGroup4.SetActive(false);
    }

    public void Light2()
    {
        lightGroup1.SetActive(false);
        lightGroup2.SetActive(true);
        lightGroup3.SetActive(false);
        lightGroup4.SetActive(false);
    }
    public void Light3()
    {
        lightGroup1.SetActive(false);
        lightGroup2.SetActive(false);
        lightGroup3.SetActive(true);
        lightGroup4.SetActive(false);
    }
    public void Light4()
    {
        lightGroup1.SetActive(false);
        lightGroup2.SetActive(false);
        lightGroup3.SetActive(false);
        lightGroup4.SetActive(true);
    }
    private void OnGUI()
    {
        AdjustObject();
        BuildSampleName();
    }

    private void AdjustObject()
    {
        currentChildModel.transform.localScale = new Vector3(sizeSlider.value * 8, sizeSlider.value * 8, sizeSlider.value * 8);
        currentChildModel.transform.rotation = Quaternion.Euler(tumbleSlider.value, panSlider.value, ccSlider.value);
        currentChildModel.transform.position = new Vector3(hOffset.value * 5, vOffset.value * 5, 0);
    }
    
    

    
    //method to disable the current child object and set the next as active
    public void NextModel()
    {
        objectHolder.transform.GetChild(currentChildListPosition).gameObject.SetActive(false);
        currentChildListPosition++;
        
        //if function checks if the next object exceeds the child count and restarts at 0
        if (currentChildListPosition > modelChildCount-1)
        {
            currentChildListPosition = 0;
        }
        objectHolder.transform.GetChild(currentChildListPosition).gameObject.SetActive(true);
        currentChildModel = objectHolder.transform.GetChild(currentChildListPosition).gameObject;
        AdjustObject();

    }

    //method to disable the current child object and set the previous as active
    public void PrevModel()
    {
        objectHolder.transform.GetChild(currentChildListPosition).gameObject.SetActive(false);
        currentChildListPosition--;

        //if function checks if the next object is below zeroand if so sets it to the max child count
        if (currentChildListPosition < 0)
        {
            currentChildListPosition = modelChildCount-1;
        }
        objectHolder.transform.GetChild(currentChildListPosition).gameObject.SetActive(true);
        currentChildModel = objectHolder.transform.GetChild(currentChildListPosition).gameObject;
        AdjustObject();
    }

    
    //Method to call the single screenshot save routine. We use a coroutine so we can wait til WaitForEndOfFrame() to capture the image
    public void SaveButtonTrigger()
    {if (int.TryParse(dimensions.text, out int dimensionValue))
        {
            // Create the RenderTexture with the parsed dimension value
            myRenderTexture = new RenderTexture(dimensionValue, dimensionValue, 32);
       
        }
        else
        {
            Debug.LogError("Failed to parse the dimension value from the InputField.");
            return;
        }
        StartCoroutine(TakeAndSaveScreenshot(dimensionValue,dimensionValue, myRenderTexture));
    }

    //Method to call the Save all screenshot routine. We use a coroutine so we can wait til WaitForEndOfFrame() to capture the image
    public void SaveAllButtonTrigger()
    {
        if (int.TryParse(dimensions.text, out int dimensionValue))
        {
            // Create the RenderTexture with the parsed dimension value
            myRenderTexture = new RenderTexture(dimensionValue, dimensionValue, 32);
        }
        else
        {
            Debug.LogError("Failed to parse the dimension value from the InputField.");
            return;
        }
     StartCoroutine(SaveAllScreenshots(dimensionValue, myRenderTexture));

    }

    //Coroutine method to save all screenshots. It loops through the total count of children saving a screenshot, then loading the next model
    IEnumerator SaveAllScreenshots(int dimensionValue, RenderTexture createdTexture)
    {
        
        currentChildListPosition = 0;

        for (int i = 0; i < modelChildCount; i++)
        {
            
            StartCoroutine(TakeAndSaveScreenshot(dimensionValue,dimensionValue, createdTexture));
            NextModel();
            yield return new WaitForSeconds(0.01f);
        }
        

    }

    //Coroutine to save the screenshot
IEnumerator TakeAndSaveScreenshot(int width, int height, RenderTexture activeTexture)
{
    // Disable the canvas
    timpsShotCanvas.SetActive(false);
    // Create a new 2D texture of the right size
    var texture = new Texture2D(width, height, TextureFormat.RGBA32, false );
    if (!silhouetteToggle.isOn)
    {
        iconCamera.targetTexture = activeTexture;
        iconCamera.aspect = 1;
        iconCamera.Render();
    }
    else
    {
        silhouetteCamera.targetTexture = activeTexture;
        silhouetteCamera.aspect = 1;
        silhouetteCamera.Render();
        }

    // Waits til the end of the frame. This is the last graphics call made. After GUI and Post Processing have been called
    yield return new WaitForEndOfFrame();
    
    // Read the pixels from the render texture
 
    
 


    

    var outputPath = $"{saveFolderText.text}";
    //Check for the output folder and create it if necessary
    if (!Directory.Exists(outputPath))
    {
        Directory.CreateDirectory(outputPath);
    }

    
    
    string spriteName =
        (prefixToggle.isOn ? filePrefix.text : "") +
        currentChildModel.transform.GetChild(0).name +
        (iconSizeToggle.isOn ? dimensions.text : "") +
        (suffixToggle.isOn ? fileSuffix.text : "") +
        "_" + 
        (randomToggle.isOn ? Random.Range(1111, 9999).ToString() : "");
    
    
    RenderTexture.active = activeTexture;
    texture.ReadPixels(new Rect(0, 0, activeTexture.width, activeTexture.height), 0, 0);
    texture.Apply();
    //we create the PNG data
    var bytes = texture.EncodeToPNG();
    //We create the full path of folder and file name
    var iconPath = $"{outputPath}/{spriteName}.png";
    //write the actual file
    File.WriteAllBytes(iconPath, bytes);
    //Debug the details to the console to confirm to the user
    Debug.Log($"Icon saved to path '{iconPath}'");
    timpsShotCanvas.SetActive(true);
    yield return null;
}



}
