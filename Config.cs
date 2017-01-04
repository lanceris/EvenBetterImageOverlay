using UnityEngine;
using System.IO;
using System.Xml.Serialization;


namespace EvenBetterImageOverlay
{
    //what to save & where to save to
    public class Config : MonoBehaviour
    {
        public static Config ins;
        public Configuration config;
        private static readonly string configPath = "EvenBetterImageOverlay.xml";

        public void Awake()
        {
            ins = this;
            config = Configuration.Deserialize(configPath);
            if (config == null)
            {
                config = new Configuration();
                LoadingExtension.go.transform.position = new Vector3(0f, 200f, 0f);
                LoadingExtension.go.transform.eulerAngles = new Vector3(0f, 180f, 0f);
                LoadingExtension.go.transform.localScale = new Vector3(193f, 1f, 193f);
            }
            else
            {

                LoadingExtension.go.transform.position = new Vector3(config.posx, config.posy, config.posz);
                LoadingExtension.go.transform.eulerAngles = new Vector3(config.rotx, config.roty, config.rotz);
                LoadingExtension.go.transform.localScale = new Vector3(config.sclx, config.scly, config.sclz);
            }
            
        }

        public void SaveConfig()
        {
            config.posx = MainLoad.ps.x;
            config.posy = MainLoad.ps.y;
            config.posz = MainLoad.ps.z;

            config.rotx = MainLoad.rt.x;
            config.roty = MainLoad.rt.y;
            config.rotz = MainLoad.rt.z;

            config.sclx = MainLoad.sc.x;
            config.scly = MainLoad.sc.y;
            config.sclz = MainLoad.sc.z;
            Configuration.Serialize(configPath, config);
        }
    }

    //actual (de-)serialization
    public class Configuration
    {
        public float posx, posy, posz, sclx, scly, sclz, rotx, roty, rotz;

        public void OnPreSerialize()
        {
        }

        public void OnPostDeserialize()
        {
        }

        public static void Serialize(string filename, Configuration config)
        {
            var serializer = new XmlSerializer(typeof(Configuration));

            using (var writer = new StreamWriter(filename))
            {
                config.OnPreSerialize();
                serializer.Serialize(writer, config);
            }
            
        }

        public static Configuration Deserialize(string filename)
        {
            var serializer = new XmlSerializer(typeof(Configuration));

            try
            {
                using (var reader = new StreamReader(filename))
                {
                    var config = (Configuration)serializer.Deserialize(reader);
                    config.OnPostDeserialize();
                    return config;
                }
            }
            catch { }

            return null;
        }
    }
}
