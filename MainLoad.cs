using ICities;
using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using ColossalFramework;
using ColossalFramework.UI;
using ColossalFramework.Math;

namespace EvenBetterImageOverlay
{
    public class EvenBetterImageOverlay : IUserMod
    {
        private UISlider slider;
        public string helperText = @"Move:                                                                    Shift + arrows or keypad arrows

Enlarge:                                                                 Shift + plus(+) or keypad 3
Reduce:                                                                  Shift + minus(-) or keypad 1

Precise movement:                                             Hold Ctrl
Fast movement:                                             Hold Ctrl + Alt";

        public const string keyBindingsSettingsFileName = "ImageOverlayKeyBindings";

        public string Description
        {
            get { return "Overlays an images located at [Steam Skylines Folder]/Files/*.png"; }
        }

        public string Name
        {
            get { return "Image Overlay"; }
        }

        public EvenBetterImageOverlay()
        {
            try
            {
                // Creating setting file
                if (GameSettings.FindSettingsFileByName(EvenBetterImageOverlay.keyBindingsSettingsFileName) == null)
                {
                    GameSettings.AddSettingsFile(new SettingsFile[] { new SettingsFile() { fileName = EvenBetterImageOverlay.keyBindingsSettingsFileName } });
                }
            }
            catch
            {
                // We could catch the exception here
            }
        }

        public void OnSettingsUI(UIHelperBase helper)
        {
            UIHelperBase group = helper.AddGroup(Name);
            UIPanel panel = ((UIHelper)group).self as UIPanel;

            group.AddSpace(20);

            slider = (UISlider)helper.AddSlider("Overlay Alpha", 1f, 255f, 1f, Config.overlayAlpha, (f) =>
            {
                Config.overlayAlpha = f;
                Config.ins.SaveConfig();
                slider.tooltip = $"{Mathf.Floor((Config.overlayAlpha / 255f) * 100)}%\n\nSet opacity level for overlay.";
                slider.RefreshTooltip();
            });
            slider.width = 510f;
            slider.height = 10f;
            slider.color = Color.cyan;
            slider.scrollWheelAmount = 1f;
            slider.tooltip = $"{Mathf.Floor((Config.overlayAlpha / 255f) * 100)}%\n\nSet opacity level for overlay.";
            group.AddSpace(20);
            group.AddButton("Apply", MainLoad.Kek);

            group.AddSpace(10);
            group = helper.AddGroup("Main keybindings");
            panel = ((UIHelper)group).self as UIPanel;
            group.AddSpace(10);

            ((UIPanel)((UIHelper)group).self).gameObject.AddComponent<MainOptionsKeyMappings>();

            group.AddSpace(10);
            group = helper.AddGroup("Movement keybindings");
            group.AddGroup(helperText);
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
            go = new GameObject();
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
            go.AddComponent<MainLoad>();
            go.AddComponent<Config>();
            tex = MainLoad.ApplyOpacity(tex, "both");
            MainLoad.textureDict.Add(deffile[0], tex);
            RenderOver.OnLevelLoaded();
        }

        public override void OnLevelUnloading()
        {
            levelLoaded = false;
            go.GetComponent<Config>();
            Config.ins.SaveConfig();
            MainLoad.Unload(go);
        }
    }

    public class MainLoad : MonoBehaviour
    {
        public static Dictionary<string, Texture2D> textureDict = new Dictionary<string, Texture2D>();
        public static Vector3 ps, rt, sc;
        public static bool isMovable = true;
        public static bool active = true;
        int count = 0;
        int c = 1;

        public static Texture2D FlipTexture(Texture2D textureToFlip)
        {
            Texture2D texture = new Texture2D(textureToFlip.width, textureToFlip.height);

            for (int y = 0; y < textureToFlip.height; ++y)
            {
                for (int x = 0; x < textureToFlip.width; ++x)
                {
                    texture.SetPixel(x, y, textureToFlip.GetPixel(y, x));
                }
            }
            return texture;
        }

        public static string[] TextureLoad()
        {
            string[] fileList = Directory.GetFiles("Files/", "*.png");
            return fileList;
        }

        public static void Kek()
        {
            ApplyOpacity(LoadingExtension.tex, "no");
        }

        public static Texture2D ApplyOpacity(Texture2D texture, string flip)
        {
            if (LoadingExtension.levelLoaded)
            {
                if (flip == "no")
                {
                    Color32[] oldColors = texture.GetPixels32();
                    for (int i = 0; i < oldColors.Length; i++)
                    {
                        if (oldColors[i].a != 0f)
                        {
                            Color32 newColor = new Color32(oldColors[i].r, oldColors[i].g, oldColors[i].b, (byte)((Config.overlayAlpha / 255f) * 255));
                            oldColors[i] = newColor;
                        }
                    }
                    texture.SetPixels32(oldColors);
                }
                else if (flip == "both")
                {
                    texture = FlipTexture(texture);
                    Color32[] oldColors = texture.GetPixels32();
                    for (int i = 0; i < oldColors.Length; i++)
                    {
                        if (oldColors[i].a != 0f)
                        {
                            Color32 newColor = new Color32(oldColors[i].r, oldColors[i].g, oldColors[i].b, (byte)((Config.overlayAlpha / 255f) * 255));
                            oldColors[i] = newColor;
                        }
                    }
                    texture.SetPixels32(oldColors);
                }
                texture.Apply();
                return texture;
            }
            return null;
        }

        public static void Unload(GameObject go)
        {
            Destroy(go);
            textureDict.Clear();
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
            Vector3 rotationDelta = new Vector3(0f, 1f, 0f) * speedModifier / 5.0f;
            Vector3 rotate90Degree = new Vector3(0f, 90f, 0f);
            Vector3 scaleDelta = new Vector3(2.5f, 0f, 2.5f) * speedModifier;

            //fit to tiles
            if (isMovable && OptionsKeyMapping.autoFitImage.IsKeyUp())
            {
                c += 1;
                if (c == 5) c = 1;
                switch (c)
                {
                    case 1:
                        //1x1
                        LoadingExtension.go.transform.localScale = new Vector3(960f, 1f, 960f);
                        break;
                    case 2:
                        //3x3
                        LoadingExtension.go.transform.localScale = new Vector3(2880f, 1f, 2880f);
                        break;
                    case 3:
                        //5x5
                        LoadingExtension.go.transform.localScale = new Vector3(4800f, 1f, 4800f);
                        break;
                    case 4:
                        //9x9
                        LoadingExtension.go.transform.localScale = new Vector3(8640f, 1f, 8640f);
                        break;
                }
            }
            //cycle through images
            if (isMovable && OptionsKeyMapping.cycleThroughImages.IsKeyUp())
            {
                string[] files = TextureLoad();
                //Texture2D texture = LoadingExtension.tex;
                Texture2D texture = new Texture2D(1, 1);
                if (textureDict.ContainsKey(files[count]))
                {
                    texture = textureDict[files[count]];
                }
                else
                {
                    byte[] bytes = File.ReadAllBytes(files[count]);
                    texture.LoadImage(bytes);
                    texture = ApplyOpacity(texture, "both");
                    textureDict.Add(files[count], texture);
                }
                texture.Apply();
                LoadingExtension.tex = texture;
                count += 1;
                if (count == files.Length) count = 0;
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

            //Rotate
            if (isMovable && OptionsKeyMapping.rotateCounterClockwise.IsPressed()) //rotating clockwise revert
            {
                transform.localEulerAngles -= rotationDelta;
            }
            else if (isMovable && OptionsKeyMapping.rotateClockwise.IsPressed()) //rotating clockwise
            {
                transform.localEulerAngles += rotationDelta;
            }

        //Rotate 90 degrees
            if (isMovable && (Input.GetKeyDown(KeyCode.LeftBracket) && shiftDown)) //rotating clockwise revert
            {
                transform.localEulerAngles -= rotate90Degree;
            }
            
            if (isMovable && (Input.GetKeyDown(KeyCode.RightBracket) && shiftDown)) //rotating clockwise
            {
                transform.localEulerAngles += rotate90Degree;
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

            //Toggle active
            if (OptionsKeyMapping.toggleOverlay.IsKeyUp())
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

            //Lock
            if (OptionsKeyMapping.lockImage.IsKeyUp())
            {
                if (active) isMovable = !isMovable;
            }

            //Reset position to default
            if (isMovable && OptionsKeyMapping.resetImage.IsKeyUp())
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

        protected override void EndOverlayImpl(RenderManager.CameraInfo cameraInfo)
        {
            base.EndOverlayImpl(cameraInfo);
            if (!MainLoad.active) return;
            float x = MainLoad.ps.x, y = MainLoad.ps.z;
            float sclx = MainLoad.sc.x, scly = MainLoad.sc.z;


            Quaternion rot = Quaternion.Euler(MainLoad.rt.x, MainLoad.rt.y, MainLoad.rt.z);
            Vector3 center = new Vector3(x, 0, y);
            Quad3 position = new Quad3(
                            new Vector3(-sclx + x, 0, -scly + y),//lefttop 1
                            new Vector3(sclx + x, 0, -scly + y),//righttop 2
                            new Vector3(sclx + x, 0, scly + y),//rightbottom 3 
                            new Vector3(-sclx + x, 0, scly + y)//leftbottom 4
                );
            position.a = rot * (position.a - center) + center;
            position.b = rot * (position.b - center) + center;
            position.c = rot * (position.c - center) + center;
            position.d = rot * (position.d - center) + center;


            RenderManager.instance.OverlayEffect.DrawQuad(cameraInfo, LoadingExtension.tex, Color.white, position, -1f, 1800f, false, true);

        }
    }
}
