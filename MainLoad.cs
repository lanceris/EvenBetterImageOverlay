using ICities;
using UnityEngine;
using System.IO;
using ColossalFramework.UI;
using ColossalFramework.Math;

/*
             TODO:
              + Выпилить старый оверлей
              - UI для изменения управления
              - Сохранение клавиш управления в конфиг
*/

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
            });
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
                return;
            }
            //go.AddComponent<ShaderLoad>();
            //ShaderLoad.shader.SetTexture("_MainTex", tex);
            //go.GetComponent<Renderer>().material = ShaderLoad.shader;
            go.AddComponent<MainLoad>();
            go.AddComponent<Config>();
            go.GetComponent<Renderer>().enabled = false;
            MainLoad.UpdateOpacity();
            MainLoad.rotateTexture(tex, false);
            RenderOver.OnLevelLoaded();
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
        
        string[] fl = TextureLoad();
        public static Vector3 ps, rt, sc;
        public static bool isMovable = true;
        int count = -1;
        int c = 1;
        public static bool active = true;

        public static string[] TextureLoad()
        {
            string[] fileList = Directory.GetFiles("Files/", "*.png");
            return fileList;
        }

        public static void ApplyOpacity()
        {
            if (LoadingExtension.levelLoaded)
            {
                UpdateOpacity();
            }
        }

        public static void UpdateOpacity()
        {
            Texture2D texture = LoadingExtension.tex;
            Color32[] oldColors = texture.GetPixels32();
            for (int i = 0; i < oldColors.Length; i++)
            {

                if (oldColors[i].a != 0f)
                {
                    Color32 newColor = new Color32(oldColors[i].r, oldColors[i].g, oldColors[i].b, (byte)((Config.overlayAlpha / 255f) * 255));
                    oldColors[i] = newColor;
                }
                
            }
            LoadingExtension.tex.SetPixels32(oldColors);
            LoadingExtension.tex.Apply();
        }
        public static Texture2D rotateTexture(Texture2D originalTexture, bool clockwise)
        {
            Color32[] original = originalTexture.GetPixels32();
            Color32[] rotated = new Color32[original.Length];
            int w = originalTexture.width;
            int h = originalTexture.height;

            int iRotated, iOriginal;

            for (int j = 0; j < h; ++j)
            {
                for (int i = 0; i < w; ++i)
                {
                    iRotated = (i + 1) * h - j - 1;
                    iOriginal = clockwise ? original.Length - 1 - (j * w + i) : j * w + i;
                    rotated[iRotated] = original[iOriginal];
                }
            }

            Texture2D rotatedTexture = new Texture2D(h, w);
            rotatedTexture.SetPixels32(rotated);
            rotatedTexture.Apply();
            return rotatedTexture;
        }

        public static void Unload(GameObject go)
        {
            Destroy(go);
        }

        void Update()
        {
            //track overlay location
            ps = LoadingExtension.go.transform.position;
            rt = LoadingExtension.go.transform.eulerAngles;
            sc = LoadingExtension.go.transform.localScale;

            bool controlDown = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            bool shiftDown = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            bool altDown = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
            float slowSpeedFactor = 0.1f;
            float fastSpeedFactor = 3f;
            float speedModifier;

            if (controlDown && altDown) { speedModifier = fastSpeedFactor; }
            else if (controlDown) { speedModifier = slowSpeedFactor; }
            else { speedModifier = 1f; }

            float positionDelta = 400f * speedModifier * Time.deltaTime;
            Vector3 rotationDelta = new Vector3(0f, 1f, 0f) * speedModifier;
            Vector3 scaleDelta = new Vector3(2.5f, 0f, 2.5f) * speedModifier;

            //fit to tiles
            if (isMovable && (shiftDown && Input.GetKeyDown(KeyCode.T)))
            {
                switch (c)
                {
                    case 1:
                        //1x1
                        LoadingExtension.go.transform.localScale = new Vector3(960f, 1f, 960f);
                        c += 1;
                        break;
                    case 2:
                        //3x3
                        LoadingExtension.go.transform.localScale = new Vector3(2880f, 1f, 2880f);
                        c += 1;
                        break;
                    case 3:
                        //5x5
                        LoadingExtension.go.transform.localScale = new Vector3(4800f, 1f, 4800f);
                        c += 1;
                        break;
                    case 4:
                        //9x9
                        LoadingExtension.go.transform.localScale = new Vector3(8640f, 1f, 8640f);
                        c = 1;
                        break;
                }
            }
            //cycle through images
            if (isMovable && (shiftDown && Input.GetKeyDown(KeyCode.R)))
            {
                string[] files = TextureLoad();
                count += 1;

                byte[] bytes = File.ReadAllBytes(files[count]);
                
                LoadingExtension.tex.LoadImage(bytes);
                //ShaderLoad.shader.SetTexture("_MainTex", LoadingExtension.tex);
                //LoadingExtension.go.GetComponent<Renderer>().material = ShaderLoad.shader;

                if (count==files.Length-1)
                {
                    count = 0;
                }

                //Set opacity
                UpdateOpacity();
                rotateTexture(LoadingExtension.tex, true);
            }
            //Position
            if (isMovable && (Input.GetKey(KeyCode.Keypad8) || shiftDown && Input.GetKey(KeyCode.UpArrow))) // UP
            {
                transform.position += new Vector3(0f, 0f, positionDelta);
            }
            else if (isMovable && (Input.GetKey(KeyCode.Keypad2) || shiftDown && Input.GetKey(KeyCode.DownArrow))) // DOWN
            {
                transform.position += new Vector3(0f, 0f, -positionDelta);
            }
            if (isMovable && (Input.GetKey(KeyCode.Keypad4) || shiftDown && Input.GetKey(KeyCode.LeftArrow))) // LEFT
            {
                transform.position += new Vector3(-positionDelta, 0f, 0f);
            }
            else if (isMovable && (Input.GetKey(KeyCode.Keypad6) || shiftDown && Input.GetKey(KeyCode.RightArrow))) // RIGHT
            {
                transform.position += new Vector3(positionDelta, 0f, 0f);
            }

            //Scale
            if (isMovable && (Input.GetKey(KeyCode.Keypad3) || shiftDown && Input.GetKey(KeyCode.Equals)))
            {
                transform.localScale += scaleDelta * speedModifier;
            }
            else if (isMovable && (Input.GetKey(KeyCode.Keypad1) || shiftDown && Input.GetKey(KeyCode.Minus)))
            {
                transform.localScale -= scaleDelta * speedModifier;
            }

            //Rotation
            if (isMovable && (Input.GetKey(KeyCode.Keypad7) || shiftDown && Input.GetKey(KeyCode.Q)))
            {
                transform.eulerAngles -= rotationDelta;
            }
            else if (isMovable && (Input.GetKey(KeyCode.Keypad9) || shiftDown && Input.GetKey(KeyCode.E)))
            {
                transform.eulerAngles += rotationDelta;
            }

            //90° rotation
            else if (isMovable && (shiftDown && Input.GetKeyDown(KeyCode.LeftBracket)))
            {
                transform.eulerAngles -= rotationDelta * 90;
            }
            else if (isMovable && (shiftDown && Input.GetKeyDown(KeyCode.RightBracket)))
            {
                transform.eulerAngles += rotationDelta * 90;
            }

            //Toggle active
            if (Input.GetKeyDown(KeyCode.KeypadEnter) || shiftDown && Input.GetKeyDown(KeyCode.Return))
            {
                if (!active)
                {
                    isMovable = true;
                    active = true;
                }
                else
                {
                    isMovable = false;
                    active = false;
                }
            }

            //Height
            if (Input.GetKey(KeyCode.KeypadPeriod) || shiftDown && Input.GetKey(KeyCode.X))
            {
                transform.position += new Vector3(0f, 400f * speedModifier * Time.deltaTime, 0f);
            }
            else if (Input.GetKey(KeyCode.Keypad0) || shiftDown && Input.GetKey(KeyCode.Z))
            {
                transform.position -= new Vector3(0f, 400f * speedModifier * Time.deltaTime, 0f);
            }

            //Lock
            if (Input.GetKeyDown(KeyCode.Keypad5) || shiftDown && Input.GetKeyDown(KeyCode.V))
            {
                isMovable = !isMovable;
            }

            //Reset rotation and position to default
            if (isMovable && (shiftDown && Input.GetKey(KeyCode.B)))
            {
                transform.eulerAngles = new Vector3(0f, 180f, 0f);
                transform.position = new Vector3(0f, 200f, 0f);
            }
        }
    }

    internal class RenderOver : SimulationManagerBase<RenderOver, MonoBehaviour>, ISimulationManager, IRenderableManager
    {
        public static void OnLevelLoaded()
        {
            SimulationManager.RegisterManager(instance);
        }

        protected override void SimulationStepImpl(int subStep)
        {
            base.SimulationStepImpl(subStep);
            //controls
        }

        protected override void EndOverlayImpl(RenderManager.CameraInfo cameraInfo)
        {
            base.EndOverlayImpl(cameraInfo);
            if (!MainLoad.active)
            {
                return;
            }
            float x = MainLoad.ps.x, y = MainLoad.ps.z;
            float sclx = MainLoad.sc.x, scly = MainLoad.sc.z;

            RenderManager renderManager = RenderManager.instance;
            var effect = renderManager.OverlayEffect.transform; 
            Quad3 position = new Quad3(
                new Vector3(-sclx + x, 0, -scly + y),//lefttop 1
                new Vector3(sclx + x, 0, -scly + y),//righttop 2
                new Vector3(sclx + x, 0, scly + y),//rightbottom 3 
                new Vector3(-sclx + x, 0, scly + y)//leftbottom 4
                
                );
            renderManager.OverlayEffect.DrawQuad(cameraInfo, LoadingExtension.tex, Color.white, position, -1f, 1800f, false, true);
            
        }
    }
}
