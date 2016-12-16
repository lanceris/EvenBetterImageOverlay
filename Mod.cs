using ICities;
using UnityEngine;
using System.IO;

namespace EvenBetterImageOverlay
{
    public class EvenBetterImageOverlay : IUserMod
    {
        public string Description
        {
            get { return "Overlays an images located at [Steam Skylines Folder]/Files/*.png"; }
        }

        public string Name
        {
            get { return "EvenBetterImageOverlay"; }
        }
    }

    public class LoadingExtension : LoadingExtensionBase
    {
        string[] deffile = Movement.TextureLoad();
        public static GameObject go;
        public Config config;
        public static Texture2D tex;
        public static int width, height;
        public override void OnLevelLoaded(LoadMode mode)
        {
            
            go = GameObject.CreatePrimitive(PrimitiveType.Plane);
            tex = new Texture2D(2, 2);
            try
            {
                byte[] bytes = File.ReadAllBytes(deffile[0]);
                tex.LoadImage(bytes);
            }
            catch
            {
                go.SetActive(false);
                return;
            }
            go.AddComponent<ShaderLoad>();
            ShaderLoad.shader.SetTexture("_MainTex", tex);

            go.GetComponent<Renderer>().material = ShaderLoad.shader;

            go.AddComponent<Movement>();
            go.AddComponent<Config>();
            
            //go.transform.SetParent(Camera.main.transform.parent);
        }

        public override void OnLevelUnloading()
        {
            //save
            go.GetComponent<Config>();
            Config.ins.SaveConfig();
        }

    }

    public class Movement : MonoBehaviour
    {
        string[] fl = TextureLoad();
        float srtSlowSpeedFactor = 0.1f;
        public static Vector3 ps, rt, sc;
        bool isMovable = true;
        int count = -1;
        int c = 1;

        //load files
        public static string[] TextureLoad()
        {
            string[] fileList = Directory.GetFiles("Files/", "*.png");
            return fileList;
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
            }

            //Scale
            Vector3 scaleDelta = new Vector3(2.5f, 0f, 2.5f) * speedModifier;

            if (isMovable && (Input.GetKey(KeyCode.KeypadPlus) || isShiftKeyDown && Input.GetKey(KeyCode.Plus)))
            {
                transform.localScale += scaleDelta * speedModifier;
            }
            else if (isMovable && (Input.GetKey(KeyCode.KeypadMinus) || isShiftKeyDown && Input.GetKey(KeyCode.Minus)))
            {
                transform.localScale -= scaleDelta * speedModifier;
            }

            //Rotation
            Vector3 rotationDelta = new Vector3(0f, 1f, 0f) * speedModifier;

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

            //Position
            float positionDelta = 400f * speedModifier * Time.deltaTime;

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

            //Toggle
            if (isMovable && (Input.GetKeyDown(KeyCode.KeypadEnter) || isShiftKeyDown && Input.GetKeyDown(KeyCode.Return)))
            {
                gameObject.GetComponent<Renderer>().enabled = !gameObject.GetComponent<Renderer>().enabled;
            }

            //Height
            if (isMovable && (Input.GetKey(KeyCode.KeypadPeriod) || isShiftKeyDown && Input.GetKey(KeyCode.X)))
            {
                transform.position += new Vector3(0f, 400f * speedModifier * Time.deltaTime, 0f);
            }

            else if ((isMovable && Input.GetKey(KeyCode.Keypad0) || isShiftKeyDown && Input.GetKey(KeyCode.Z)))
            {
                transform.position -= new Vector3(0f, 400f * speedModifier * Time.deltaTime, 0f);
            }

            //Lock
            if (Input.GetKeyDown(KeyCode.Keypad5) || isShiftKeyDown && Input.GetKeyDown(KeyCode.V))
            {
                isMovable = !isMovable;
            }

            //Reset rotation and position to default
            if (isMovable && (isShiftKeyDown && Input.GetKey(KeyCode.C)))
            {
                transform.eulerAngles = new Vector3(0f, 180f, 0f);
                transform.position = new Vector3(0f, 200f, 0f);
            }
        }
    }


}
