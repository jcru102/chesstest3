using UnityEngine;
using UnityEngine.UI;

public class SwapCamera : MonoBehaviour
{
    [SerializeField] private Camera whiteCamera;
    [SerializeField] private Camera blackCamera;
    [SerializeField] private Button switchCameraButton;

    private bool isWhiteCameraActive = true;

    private void Start()
    {
        
        SetCameraState(true); 

       
        if (switchCameraButton != null)
        {
            switchCameraButton.onClick.AddListener(SwitchCamera);
        }
    }

    public void SwitchCamera()
    {
        
        isWhiteCameraActive = !isWhiteCameraActive;
        SetCameraState(isWhiteCameraActive);
    }

    private void SetCameraState(bool whiteActive)
    {
        if (whiteCamera != null && blackCamera != null)
        {
            whiteCamera.enabled = whiteActive;
            blackCamera.enabled = !whiteActive;

            
            if (switchCameraButton != null)
            {
                Text buttonText = switchCameraButton.GetComponentInChildren<Text>();
                if (buttonText != null)
                {
                    buttonText.text = whiteActive ? "Switch to Black View" : "Switch to White View";
                }
            }
        }
    }

    
    public void SetWhiteCameraActive()
    {
        isWhiteCameraActive = true;
        SetCameraState(true);
    }

    public void SetBlackCameraActive()
    {
        isWhiteCameraActive = false;
        SetCameraState(false);
    }
}
