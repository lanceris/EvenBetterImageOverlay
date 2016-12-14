using UnityEngine;
using System.IO;
using System.Xml.Serialization;


namespace EvenBetterImageOverlay
{
    public class Config : MonoBehaviour
    {
        public static Config ins;
        public Configuration config;
        public Vector3 pos;
        public Vector3 rot;
        public Vector3 scl;
        private static readonly string configPath = "EvenBetterImageOverlay.xml";

        void Awake()
        {
            ins = this;
            config = Configuration.Deserialize(configPath);

            if (config == null)
            {
                config = new Configuration();
                LoadingExtension.go.transform.position = pos = new Vector3(0f, 200f, 0f);
                LoadingExtension.go.transform.eulerAngles = rot = new Vector3(0f, 180f, 0f);
                LoadingExtension.go.transform.localScale = scl = new Vector3(175f, 1f, 175f);
                SaveConfig();
            }
            else
            {
                LoadingExtension.go.transform.position = pos = new Vector3(config.posx, config.posy, config.posz);
                LoadingExtension.go.transform.eulerAngles = rot = new Vector3(config.rotx, config.roty, config.rotz);
                LoadingExtension.go.transform.localScale = scl = new Vector3(config.sclx, config.scly, config.sclz);
            }
            SaveConfig();
        }

        public void SaveConfig()
        {
            config.posx = Movement.ps.x;
            config.posy = Movement.ps.y;
            config.posz = Movement.ps.z;

            config.rotx = Movement.rt.x;
            config.roty = Movement.rt.y;
            config.rotz = Movement.rt.z;

            config.sclx = Movement.sc.x;
            config.scly = Movement.sc.y;
            config.sclz = Movement.sc.z;
            Configuration.Serialize(configPath, config);
        }
    }

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
