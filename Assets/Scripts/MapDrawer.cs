using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class MapDrawer : MonoBehaviour
{

    public float lineSmoothness = 0.1f;
    public InputActionAsset actions;

    public Texture2D mapTexture;

    public GameObject mapObject;

    public FirstPersonController controller;
    public StarterAssetsInputs inputs;

    public Texture2D cursorTexture;

    bool mapOpen = false;

    (bool,Vector2) lastHit = new (false, Vector2.zero);
    // Start is called before the first frame update
    void Awake()
    {
        actions.FindActionMap("Player").Enable();
        actions.FindAction("ToggleMap").performed += OnOpenMap;
        mapTexture = new Texture2D(256, 256);
}

    // Update is called once per frame
    void FixedUpdate()
    {
        if(mapOpen)
        {


            // 
            if (Mouse.current.leftButton.isPressed){


                if (Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), out var hit, 100, LayerMask.GetMask("Map")))
                {

                    Renderer rend = hit.transform.GetComponent<Renderer>();
                    Texture2D original = rend.material.mainTexture as Texture2D;
                    Texture2D tex = new Texture2D(original.width, original.height);
                    Graphics.CopyTexture(original, tex);
                    Vector2 pixelUV = hit.textureCoord;
                    pixelUV.x *= tex.width;
                    pixelUV.y *= tex.height;
                    if (lastHit.Item1)
                    {
                       

                        Vector2 pixelUV2 = lastHit.Item2;
                        pixelUV2.x *= tex.width;
                        pixelUV2.y *= tex.height;

                       for(float i = 0; i < 1; i+= lineSmoothness)
                        {
                            Vector2 l = Vector2.Lerp(pixelUV, pixelUV2, i);
                            tex.SetPixel((int)l.x, (int)l.y, Color.black);
                           

                        }
                        tex.Apply();

                        rend.material.mainTexture = tex;
                        lastHit = new(true, hit.textureCoord);
                    }
                    else if(lastHit.Item2 != Vector2.zero) { }
                    {
                        tex.SetPixel((int)pixelUV.x, (int)pixelUV.y, Color.black);
                        tex.Apply();
                        rend.material.mainTexture = tex;
                        lastHit = new(true, hit.textureCoord);
                    }
                    //GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    //sphere.transform.localScale = Vector3.one * 0.01f;
                    //sphere.transform.position = hit.point;
                    //sphere.transform.SetParent(mapObject.transform);
                   

                }
                else
                {
                    lastHit = new (false, Vector2.zero);


                }
            }
            else
            {
                lastHit = new(false, Vector2.zero);
            }
          
          

        }


    }
    private void OnDrawGizmos()
    {
     // Gizmos.DrawRay(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()));
    }
    public void OnOpenMap(InputAction.CallbackContext context)
    {
        if (!mapOpen)
        {
            
            mapObject.SetActive(true);

            //controller.enabled = false;
           
            inputs.cursorLocked = false;
            inputs.cursorInputForLook = false;

          
          
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.lockState = CursorLockMode.None;
            Cursor.SetCursor(cursorTexture, new Vector2(0, cursorTexture.height), CursorMode.Auto);

            mapOpen = true;
        }
        else
        {
            mapObject.SetActive(false);
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            //controller.enabled = true;
            Cursor.visible = false;
            //Cursor.lockState = CursorLockMode.Confined;
            inputs.cursorInputForLook = true;
            inputs.cursorLocked = true;
            mapOpen = false;
        }
       


    }
}
