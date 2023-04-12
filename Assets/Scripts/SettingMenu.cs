using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingMenu : MonoBehaviour
{
    /// <summary>
    /// This thing is broken
    /// </summary>
    public GameObject menu;

    bool settingEnabled = false;

    public TMPro.TMP_Dropdown resolutionDropdown;

    public InputActionAsset actions;

    Resolution[] resolutions;
    // Start is called before the first frame update
    void Awake()
    {
        actions.FindActionMap("UI").Enable();
        actions.FindAction("Esc").performed += ToggleMenu;
    }

    private void Start()
    {
        resolutions = Screen.resolutions;

        resolutionDropdown.ClearOptions();

        int currentInedx = 0;
        List<string> options = new List<string>();

        for(int i = 0; i < resolutions.Length; i++)
        {

            options.Add($"{resolutions[i].width} x {resolutions[i].height}");

            if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].width == Screen.currentResolution.width)
            {
                currentInedx = i;
            }

        }

        resolutionDropdown.AddOptions(options);

    }

    public void ChangeRess(int i)
    {
        Screen.SetResolution(resolutions[i].width, resolutions[i].height, Screen.fullScreen);


    }
    public void ToggleMenu(InputAction.CallbackContext context)
    {
        settingEnabled = !settingEnabled;
        menu.SetActive(settingEnabled);
        if (settingEnabled)
        {
            
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
           
            Cursor.lockState = CursorLockMode.Locked;
        }

    }

    public void SetFullscreen(bool value)
    {

        Screen.fullScreen = value; 
    }

    public void Quit()
    {

        Application.Quit();
    }
   
}
