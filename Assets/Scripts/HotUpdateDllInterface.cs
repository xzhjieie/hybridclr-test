using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using HybridCLR;
using System.Collections;

namespace GameApp
{
    public static partial class HotApis
    {
        private static Type s_ExportInterface;

        public static bool Initialized { get; private set; } = false;

        public static void InitGameAppInterface()
        {
            LoadHotUpdateDll.DllDict.TryGetValue("GameApp", out var gameAppAss);
            if (gameAppAss == null)
                return;

            s_ExportInterface = gameAppAss.GetType("ExportInterface");

            InitInterfaceAuto();

            Initialized = true;
        }

        private static T GetAction<T>(string kMethodName)
        {
            if (s_ExportInterface != null)
                return (T)s_ExportInterface.GetMethod(kMethodName)?.Invoke(null, null);
            else
                return default;
        }
    }
}