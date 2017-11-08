using System.Reflection;
using UnityEngine;
using ColossalFramework.Plugins;

namespace EvenBetterImageOverlay
{
    //loading shader from assetbundle
    public class ShaderLoad : MonoBehaviour
    {
        public static ShaderLoad instance = null;
        public static Material shader;
        private static string cachedModPath = null;
        static string modPath
        {
            get
            {
                if (cachedModPath == null)
                {
                    cachedModPath =
                        PluginManager.instance.FindPluginInfo(Assembly.GetAssembly(typeof(ShaderLoad))).modPath;
                }

                return cachedModPath;
            }
        }

        void LoadShaders()
        {
            string shdn;
            if (Application.platform == RuntimePlatform.OSXPlayer)
            {
                shdn = "/osxshader";
            }
            else if (Application.platform == RuntimePlatform.LinuxPlayer)
            {
                shdn = "/linuxshader";
            }
            else
            {
                shdn = "/shader";
            }
            string assetsUri = "file:///" + modPath.Replace("\\", "/") + shdn;
            WWW www = new WWW(assetsUri);
            AssetBundle assetBundle = www.assetBundle;
            string shaderName = "OverlayShader.shader";
            Shader shaderContent = assetBundle.LoadAsset(shaderName) as Shader;
            shader = new Material(shaderContent);

            assetBundle.Unload(false);
            www.Dispose();
        }

        public void Awake()
        {
            instance = this;
            LoadShaders();
        }
    }

}