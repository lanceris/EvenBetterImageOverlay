using System.Reflection;
using UnityEngine;
using ColossalFramework.Plugins;

namespace EvenBetterImageOverlay
{
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
            
            string assetsUri = "file:///" + modPath.Replace("\\", "/") + "/shader";
            WWW www = new WWW(assetsUri);
            AssetBundle assetBundle = www.assetBundle;

            string shaderName = "OverlayShader.shader";
            Shader shaderContent = assetBundle.LoadAsset(shaderName) as Shader;
            shader = new Material(shaderContent);

            assetBundle.Unload(false);
        }

        public void Awake()
        {
            instance = this;
            LoadShaders();
        }
    }

}