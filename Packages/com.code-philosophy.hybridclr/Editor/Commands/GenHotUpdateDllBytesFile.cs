using HybridCLR.Editor.AOT;
using HybridCLR.Editor.Meta;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace HybridCLR.Editor.Commands
{
    public class GenHotUpdateDllBytesFile
    {
        /// <summary>
        /// 在 AOTGenericReferences.cs 文件中查找需要用到的补充元数据列表，
        /// 从裁剪后的 AOT dll 中复制对于的 dll 文件，到 Assets/StreamingAseets/HotUpdateDlls 目录下，修改为 bytes 文件
        /// </summary>
        [MenuItem("HybridCLR/Generate/GameApp.dll.bytes", priority = 107)]
        public static void GeneratePatchedAOTAssemblies()
        {
            GenerateDllBytesFile();

        }

        public static void GenerateDllBytesFile()
        {
            foreach (var dllFile in SettingsUtil.HotUpdateAssemblyFilesExcludePreserved)
            {
                string dllFilePath = $"{SettingsUtil.GameAppHotUpdateDllDir}/{dllFile}";
                string bytesFilePath = $"{SettingsUtil.GameAppHotUpdateDllBytesDir}/{dllFile}.bytes";
                if (File.Exists(dllFilePath))
                {
                    File.Copy(dllFilePath, bytesFilePath, true);
                    Debug.Log($"生成热更文件 -> {bytesFilePath}");
                }
                else
                {
                    Debug.LogError($"生成热更文件失败 -> {bytesFilePath}.");
                }
            }

            AssetDatabase.Refresh();
        }
    }
}
