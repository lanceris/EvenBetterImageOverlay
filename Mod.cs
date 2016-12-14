using ICities;
using UnityEngine;
using System.IO;
using System;

namespace EvenBetterImageOverlay
{
    public class EvenBetterImageOverlay : IUserMod
    {
        public string Description
        {
            get { return "Overlays an image (located at [Steam Skylines Folder]/Files/overlay.png)."; }
        }

        public string Name
        {
            get { return "EvenBetterImageOverlay"; }
        }
    }

    public class LoadingExtension : LoadingExtensionBase
    {
        public static GameObject go;
        public Config config;

        public override void OnLevelLoaded(LoadMode mode)
        {

            go = GameObject.CreatePrimitive(PrimitiveType.Plane);

            Texture2D tex = new Texture2D(2, 2);

            try
            {
                byte[] bytes = File.ReadAllBytes("Files/overlay.png");
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
        }

        public override void OnLevelUnloading()
        {
            go.GetComponent<Config>();
            Config.ins.SaveConfig();
        }

    }

    public class Movement : MonoBehaviour
    {
        float srtSlowSpeedFactor = 0.3f;
        public static Vector3 ps, rt, sc;
        bool isMovable = true;

        void Update()
        {
            ps = transform.position;
            rt = transform.eulerAngles;
            sc = transform.localScale;

            bool controlDown = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            bool isShiftKeyDown = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            float speedModifier = controlDown ? srtSlowSpeedFactor : 1.0f;

            // Image size
            Vector3 scaleDelta = new Vector3(2.5f, 0f, 2.5f) * speedModifier;

            if (isMovable && (Input.GetKey(KeyCode.KeypadPlus) || isShiftKeyDown && Input.GetKey(KeyCode.Plus)))
            {
                transform.localScale += scaleDelta * speedModifier;
            }

            else if (isMovable && (Input.GetKey(KeyCode.KeypadMinus) || isShiftKeyDown && Input.GetKey(KeyCode.Minus)))
            {
                transform.localScale -= scaleDelta * speedModifier;
            }

            // Image rotation
            Vector3 rotationDelta = new Vector3(0f, 1f, 0f) * speedModifier;

            if (isMovable && (Input.GetKey(KeyCode.Keypad7) || isShiftKeyDown && Input.GetKey(KeyCode.Q)))
            {
                transform.eulerAngles -= rotationDelta;
            }

            else if (isMovable && (Input.GetKey(KeyCode.Keypad9) || isShiftKeyDown && Input.GetKey(KeyCode.E)))
            {
                transform.eulerAngles += rotationDelta;
            }

            //rotate by 90 degree
            else if (isMovable && (isShiftKeyDown && Input.GetKeyDown(KeyCode.LeftBracket)))
            {
                transform.eulerAngles -= rotationDelta * 90;
            }
            else if (isMovable && (isShiftKeyDown && Input.GetKeyDown(KeyCode.RightBracket)))
            {
                transform.eulerAngles += rotationDelta * 90;
            }

            //reset rotation to default
            if (isMovable && (isShiftKeyDown && Input.GetKey(KeyCode.C)))
            {
                transform.eulerAngles = new Vector3(0f, 180f, 0f);
            }

                // Image position
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

            // Image toggle
            if (Input.GetKeyDown(KeyCode.KeypadEnter) || isShiftKeyDown && Input.GetKeyDown(KeyCode.Return))
            {
                gameObject.GetComponent<Renderer>().enabled = !gameObject.GetComponent<Renderer>().enabled;
            }

            // Image height
            if (isMovable && (Input.GetKey(KeyCode.KeypadPeriod) || isShiftKeyDown && Input.GetKey(KeyCode.X)))
            {
                transform.position += new Vector3(0f, 400f * speedModifier * Time.deltaTime, 0f);
            }

            else if ((isMovable && Input.GetKey(KeyCode.Keypad0) || isShiftKeyDown && Input.GetKey(KeyCode.Z)))
            {
                transform.position -= new Vector3(0f, 400f * speedModifier * Time.deltaTime, 0f);
            }

            // lock position
            if (Input.GetKeyDown(KeyCode.Keypad5) || isShiftKeyDown && Input.GetKeyDown(KeyCode.V))
            {
                isMovable = !isMovable;
            }
        }
    }

}
