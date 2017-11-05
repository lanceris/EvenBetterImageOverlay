using ICities;
using UnityEngine;
using System.IO;
using ColossalFramework.UI;

namespace EvenBetterImageOverlay
{
    public class EvenBetterImageOverlay : IUserMod
    {
        private UISlider slider;
        public string helperText = @"Toggle overlay:                                                  Shift + Enter or keypad Enter
Cycle through images:                                      Shift + R
Lock:                                                                     Shift + V or keypad 5
Auto fit image to 1x1...9x9 tiles:                      Shift +T
Reset to default position and rotation:        Shift + B

Move:                                                                    Shift + arrows or keypad arrows

Rotate:                                                                  Shift + Q and E or keypad 7 and 9
Rotate by 90°:                                                     Shift + { or }

Raise:                                                                     Shift + X or keypad period
Lower:                                                                   Shift + Z or keypad 0

Enlarge:                                                                 Shift + plus(+) or keypad 3
Reduce:                                                                  Shift + minus(-) or keypad 1

Precise movement:                                             Hold Ctrl";

        public string Description
        {
            get { return "Overlays an images located at [Steam Skylines Folder]/Files/*.png"; }
        }

        public string Name
        {
            get { return "EvenBetterImageOverlay"; }
        }

        public void OnSettingsUI(UIHelperBase helper)
        {
            helper.AddSpace(20);
            slider = (UISlider)helper.AddSlider("Overlay Alpha", 0f, 255f, 1f, Config.overlayAlpha, (f) =>
            {
                Config.overlayAlpha = f;
                Config.ins.SaveConfig();
                slider.tooltip = $"{Mathf.Floor((Config.overlayAlpha / 255f)*100)}%\n\nSet opacity level for overlay.";
                slider.RefreshTooltip();
            }
            );
            slider.width = 510f;
            slider.height = 10f;
            slider.color = Color.cyan;
            slider.scrollWheelAmount = 1f;
            slider.tooltip = $"{Mathf.Floor((Config.overlayAlpha / 255f) * 100)}%\n\nSet opacity level for overlay.";
            helper.AddSpace(20);
            helper.AddButton("Apply", MainLoad.ApplyOpacity);
            helper.AddSpace(20);
            helper.AddGroup(helperText);
        }
    }

    public class LoadingExtension : LoadingExtensionBase
    {
        string[] deffile = MainLoad.TextureLoad();
        public static GameObject go;
        public Config config;
        public static Texture2D tex;
        public static bool levelLoaded;
        
        
        public override void OnLevelLoaded(LoadMode mode)
        {
            levelLoaded = true;
            go = GameObject.CreatePrimitive(PrimitiveType.Plane);
            tex = new Texture2D(1, 1);
            try
            {
                byte[] bytes = File.ReadAllBytes(deffile[0]);
                tex.LoadImage(bytes);
            }
            catch
            {
                Debug.Log("[EvenBetterImageOverlay]Error while loading image! Are you sure there is images in Files?");
                go.SetActive(false);
                return;
            }
            go.AddComponent<ShaderLoad>();
            ShaderLoad.shader.SetTexture("_MainTex", tex);

            go.GetComponent<Renderer>().material = ShaderLoad.shader;
            go.AddComponent<MainLoad>();
            go.AddComponent<Config>();
            MainLoad.UpdateOpacity();

            //go.transform.SetParent(Camera.main.transform.parent);
        }

        public override void OnLevelUnloading()
        {
            //save
            levelLoaded = false;
            go.GetComponent<Config>();
            Config.ins.SaveConfig();
            MainLoad.Unload(go);
        }
    }

    public class MainLoad : MonoBehaviour
    {
        //load files
        public static string[] TextureLoad()
        {
            string[] fileList = Directory.GetFiles("Files/", "*.png");
            return fileList;
        }
        string[] fl = TextureLoad();
        float srtSlowSpeedFactor = 0.1f;
        public static Vector3 ps, rt, sc;
        bool isMovable = true;
        int count = -1;
        int c = 1;

        public static void ApplyOpacity()
        {
            if (LoadingExtension.levelLoaded)
            {
                UpdateOpacity();
            }
        }

        public static void UpdateOpacity()
        {
            Color[] oldColors = LoadingExtension.tex.GetPixels();

            for (int i = 0; i < oldColors.Length; i++)
            {
                if (oldColors[i].a != 0f)
                {
                    Color newColor = new Color(oldColors[i].r, oldColors[i].g, oldColors[i].b, Config.overlayAlpha / 255f);
                    oldColors[i] = newColor;
                }
            }
            LoadingExtension.tex.SetPixels(oldColors);
            LoadingExtension.tex.Apply();
        }

        public static void Unload(GameObject go)
        {
            Destroy(go);
        }

        //listening for inputs
        void Update()
        {
            //track overlay location
            ps = transform.position;
            rt = transform.eulerAngles;
            sc = transform.localScale;

            bool controlDown = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            bool isShiftKeyDown = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            float speedModifier = controlDown ? srtSlowSpeedFactor : 1.0f;

            float positionDelta = 400f * speedModifier * Time.deltaTime;
            Vector3 rotationDelta = new Vector3(0f, 1f, 0f) * speedModifier;
            Vector3 scaleDelta = new Vector3(2.5f, 0f, 2.5f) * speedModifier;

            //fit to tiles
            if (isMovable && (isShiftKeyDown && Input.GetKeyDown(KeyCode.T)))
            {
                switch (c)
                {
                    case 1:
                        //1x1
                        LoadingExtension.go.transform.localScale = new Vector3(193f, 1f, 193f);
                        c += 1;
                        break;
                    case 2:
                        //3x3
                        LoadingExtension.go.transform.localScale = new Vector3(577f, 1f, 577f);
                        c += 1;
                        break;
                    case 3:
                        //5x5
                        LoadingExtension.go.transform.localScale = new Vector3(965f, 1f, 965f);
                        c += 1;
                        break;
                    case 4:
                        //9x9
                        LoadingExtension.go.transform.localScale = new Vector3(1727f, 1f, 1727f);
                        c = 1;
                        break;
                }
            }
            //cycle through images
            if (isMovable && (isShiftKeyDown && Input.GetKeyDown(KeyCode.R)))
            {
                string[] files = TextureLoad();
                count += 1;

                byte[] bytes = File.ReadAllBytes(files[count]);
                LoadingExtension.tex.LoadImage(bytes);
                ShaderLoad.shader.SetTexture("_MainTex", LoadingExtension.tex);
                LoadingExtension.go.GetComponent<Renderer>().material = ShaderLoad.shader;

                if (count==files.Length-1)
                {
                    count = -1;
                }

                //Set opacity
                UpdateOpacity();
            }

            //Position
            if (isMovable && (Input.GetKey(KeyCode.Keypad8) || isShiftKeyDown && Input.GetKey(KeyCode.UpArrow))) // UP
            {
                transform.position += new Vector3(0f, 0f, positionDelta);
            }
            else if (isMovable && (Input.GetKey(KeyCode.Keypad2) || isShiftKeyDown && Input.GetKey(KeyCode.DownArrow))) // DOWN
            {
                transform.position += new Vector3(0f, 0f, -positionDelta);
            }
            if (isMovable && (Input.GetKey(KeyCode.Keypad4) || isShiftKeyDown && Input.GetKey(KeyCode.LeftArrow))) // LEFT
            {
                transform.position += new Vector3(-positionDelta, 0f, 0f);
            }
            else if (isMovable && (Input.GetKey(KeyCode.Keypad6) || isShiftKeyDown && Input.GetKey(KeyCode.RightArrow))) // RIGHT
            {
                transform.position += new Vector3(positionDelta, 0f, 0f);
            }

            //Scale
            if (isMovable && (Input.GetKey(KeyCode.Keypad3) || isShiftKeyDown && Input.GetKey(KeyCode.Equals)))
            {
                transform.localScale += scaleDelta * speedModifier;
            }
            else if (isMovable && (Input.GetKey(KeyCode.Keypad1) || isShiftKeyDown && Input.GetKey(KeyCode.Minus)))
            {
                transform.localScale -= scaleDelta * speedModifier;
            }

            //Rotation
            if (isMovable && (Input.GetKey(KeyCode.Keypad7) || isShiftKeyDown && Input.GetKey(KeyCode.Q)))
            {
                transform.eulerAngles -= rotationDelta;
            }
            else if (isMovable && (Input.GetKey(KeyCode.Keypad9) || isShiftKeyDown && Input.GetKey(KeyCode.E)))
            {
                transform.eulerAngles += rotationDelta;
            }

            //90° rotation
            else if (isMovable && (isShiftKeyDown && Input.GetKeyDown(KeyCode.LeftBracket)))
            {
                transform.eulerAngles -= rotationDelta * 90;
            }
            else if (isMovable && (isShiftKeyDown && Input.GetKeyDown(KeyCode.RightBracket)))
            {
                transform.eulerAngles += rotationDelta * 90;
            }

            //Toggle
            if (Input.GetKeyDown(KeyCode.KeypadEnter) || isShiftKeyDown && Input.GetKeyDown(KeyCode.Return))
            {
                gameObject.GetComponent<Renderer>().enabled = !gameObject.GetComponent<Renderer>().enabled;
            }

            //Height
            if (Input.GetKey(KeyCode.KeypadPeriod) || isShiftKeyDown && Input.GetKey(KeyCode.X))
            {
                transform.position += new Vector3(0f, 400f * speedModifier * Time.deltaTime, 0f);
            }
            else if (Input.GetKey(KeyCode.Keypad0) || isShiftKeyDown && Input.GetKey(KeyCode.Z))
            {
                transform.position -= new Vector3(0f, 400f * speedModifier * Time.deltaTime, 0f);
            }

            //Lock
            if (Input.GetKeyDown(KeyCode.Keypad5) || isShiftKeyDown && Input.GetKeyDown(KeyCode.V))
            {
                isMovable = !isMovable;
            }

            //Reset rotation and position to default
            if (isMovable && (isShiftKeyDown && Input.GetKey(KeyCode.B)))
            {
                transform.eulerAngles = new Vector3(0f, 180f, 0f);
                transform.position = new Vector3(0f, 200f, 0f);
            }
        }
    }
}
