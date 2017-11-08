//You can copy this script's file and use it for your own scripts
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Linq;
using ColossalFramework.UI;
using UnityEngine;
        

namespace ModTools
{
    class ExampleScript : IModEntryPoint
    {
        public void OnModLoaded()
        {
            Debug.Log("Hello");
        }
        public void OnModUnloaded()
        {
            throw new Exception("Goodbye Cruel World!"); //replace this line with your clean up script
        }
    }
}