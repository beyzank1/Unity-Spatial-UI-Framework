using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChairManager : MonoBehaviour
{
    public GameObject chairObject;
    public GameObject chairCanvas;    // First canvas prefab already under this chair
    public float minWorldSeparationDistance = 1.0f;

    //3D Layout Settings
    public float riseOffset = 1.0f;    // How high to lift labels above the chair's center
    public float forwardOffset = 0.2f; // How far to push labels toward the camera

    // List of all label canvases
    public List<GameObject> myTextPanels = new List<GameObject>();

    // Baseline character metrics (computed once from first canvas)
    int numOfChars;
    float charWidth;
    
    // Reference to the main camera (for VR)
    private Camera mainCamera;


    void Start()
    {
        // Get the main camera. Tag VR camera as "MainCamera"
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("ChairManager: No camera found with 'MainCamera' tag. Billboarding will not work.", this);
        }

        chairObject = this.transform.gameObject;

        // first canvas already in hierarchy 
        myTextPanels.Add(chairCanvas);

        // Setup no-wrap on the first text
        TMP_Text firstTMP = chairCanvas.transform.GetChild(0).GetComponent<TMP_Text>();
        firstTMP.textWrappingMode = TextWrappingModes.NoWrap;
        firstTMP.overflowMode = TextOverflowModes.Overflow;

        // Baseline metrics from the first text
        // prevent divide-by-zero if text is empty
        numOfChars = Mathf.Max(1, firstTMP.text.TrimEnd('\r', '\n', ' ').Length);

        Vector3 leftPos = chairCanvas.transform.GetChild(1).position;
        Vector3 rightPos = chairCanvas.transform.GetChild(2).position;
        charWidth = Vector3.Distance(leftPos, rightPos) / numOfChars;

        // Spawn a SECOND canvas 
        GameObject canvasTwo = Instantiate(chairCanvas, chairCanvas.transform.position, chairCanvas.transform.rotation);
        canvasTwo.transform.SetParent(this.transform);
        myTextPanels.Add(canvasTwo);
        canvasTwo.transform.GetChild(0).GetComponent<TMP_Text>().text = "Black";
        var tmp2 = canvasTwo.transform.GetChild(0).GetComponent<TMP_Text>();
        tmp2.textWrappingMode = TextWrappingModes.NoWrap;
        tmp2.overflowMode = TextOverflowModes.Overflow;

        // Spawn a THIRD canvas 
        GameObject canvasThree = Instantiate(chairCanvas, chairCanvas.transform.position, chairCanvas.transform.rotation);
        canvasThree.transform.SetParent(this.transform);
        myTextPanels.Add(canvasThree);
        canvasThree.transform.GetChild(0).GetComponent<TMP_Text>().text = "Furniture";
        var tmp3 = canvasThree.transform.GetChild(0).GetComponent<TMP_Text>();
        tmp3.textWrappingMode = TextWrappingModes.NoWrap;
        tmp3.overflowMode = TextOverflowModes.Overflow;
    }

    // Switched to LateUpdate for camera-relative logic to prevent jitter
    void LateUpdate()
    {
        
        if (mainCamera == null) return; // Don't run if no camera

        // Recompute bounds & position all canvases every frame
        CalculateAllPositions3D(myTextPanels);

        // After positioning, make them all face the camera
        FaceCamera(myTextPanels);
    }

    // Update check-spheres for ONE canvas using baseline character width
    void ReturnTextBounds3D(GameObject inputCanvas)
    {
        TMP_Text tmp = inputCanvas.transform.GetChild(0).GetComponent<TMP_Text>();
        Transform leftSphere = inputCanvas.transform.GetChild(1);
        Transform rightSphere = inputCanvas.transform.GetChild(2);
        Transform checkLeft = inputCanvas.transform.GetChild(3);
        Transform checkRight = inputCanvas.transform.GetChild(4);

        int currentChars = Mathf.Max(1, tmp.text.TrimEnd('\r', '\n', ' ').Length);

        // Reset check spheres to original anchor positions
        checkLeft.position = leftSphere.position;
        checkRight.position = rightSphere.position;

        // Expand outward relative to baseline character count
        float delta = (currentChars - numOfChars) * charWidth * 0.5f;

      
        // expand along the camera's right vector
        Vector3 rightAxis = mainCamera.transform.right;

        checkLeft.position -= rightAxis * delta;
        checkRight.position += rightAxis * delta;
    }

    // lay out in 3D space relative to the camera
    private void CalculateAllPositions3D(List<GameObject> canvasList)
    {
        if (canvasList == null || canvasList.Count == 0 || chairObject == null) return;

        int count = canvasList.Count;

        // Update bounding check-spheres
        for (int i = 0; i < count; i++)
            ReturnTextBounds3D(canvasList[i]);


        // lay out along the camera's right vector
        //3D ruler that is always horizontal in user's view
        Vector3 rightAxis = mainCamera.transform.right;

        // Measure widths
        float[] widths = new float[count];
        float totalWidth = 0f;

        for (int i = 0; i < count; i++)
        {
            Transform checkLeft = canvasList[i].transform.GetChild(3);
            Transform checkRight = canvasList[i].transform.GetChild(4);

            // Measure the perceived width along the camera's right axis
            //This gives you the "perceived width" (how wide it looks to the user) instead of its simple 3D distance.
            float w = AxisDistance(checkLeft.position, checkRight.position, rightAxis);
            widths[i] = w;
            totalWidth += w;
        }

        // Add gaps
        totalWidth += minWorldSeparationDistance * (count - 1);

        // Center the group on the chair in 3D
        // Start at the chair's position
        Vector3 center = chairObject.transform.position;
        // Lift it up
        center += Vector3.up * riseOffset;
        center += Vector3.up * 1.5f;

        // Push it slightly toward the camera (away from the chair)
        center += (mainCamera.transform.position - center).normalized * forwardOffset;

        // Find the start position (leftmost edge in camera view)
        Vector3 startPos = center - rightAxis * (totalWidth * 0.5f);

        // Position canvases left-to-right (in camera view)
        float cursor = 0f;
        for (int i = 0; i < count; i++)
        {
            float half = widths[i] * 0.5f;

            // Calculate 3D target position
            Vector3 targetPos = startPos + rightAxis * (cursor + half);

            RectTransform rt = canvasList[i].GetComponent<RectTransform>();
            rt.position = targetPos;

            cursor += widths[i] + minWorldSeparationDistance;
        }
    }
   
    // Helper function to measure distance between two points projected onto an axis.
    // This finds the perceived width from the camera's angle.
    float AxisDistance(Vector3 a, Vector3 b, Vector3 axis)
    {
        Vector3 ab = b - a;
        return Mathf.Abs(Vector3.Dot(ab, axis.normalized));
    }
    
    // Makes all canvases face the camera as a single, flat plane.
    // This prevents the text from looking distorted (that happened with lookat).
    private void FaceCamera(List<GameObject> canvasList)
    {
        // Calculate ONE rotation that is parallel to the camera's view
        Vector3 forward = mainCamera.transform.forward; // Look toward the camera
        Vector3 up = mainCamera.transform.up;
        Quaternion viewPlaneRotation = Quaternion.LookRotation(forward, up);

        // Apply same rotation to all canvases
        for (int i = 0; i < canvasList.Count; i++)
        {
            canvasList[i].transform.rotation = viewPlaneRotation;
        }
    }
}