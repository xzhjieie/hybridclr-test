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
    public class AddSupplementalMetadataCommand
    {
        /// <summary>
        /// 在 AOTGenericReferences.cs 文件中查找需要用到的补充元数据列表，
        /// 从裁剪后的 AOT dll 中复制对于的 dll 文件，到 Assets/StreamingAseets/HotUpdateDlls 目录下，修改为 bytes 文件
        /// </summary>
        [MenuItem("HybridCLR/Generate/补充元数据", priority = 106)]
        public static void GeneratePatchedAOTAssemblies()
        {
            GeneratePatchedAOTAssemblies(EditorUserBuildSettings.activeBuildTarget);
        }

        public static void GeneratePatchedAOTAssemblies(BuildTarget buildTarget)
        {
            string aotGenericRefFilePath = $"{Application.dataPath}/{HybridCLR.Editor.Settings.HybridCLRSettings.Instance.outputAOTGenericReferenceFile}";
            if (!File.Exists(aotGenericRefFilePath))
            {
                Debug.LogError($"{aotGenericRefFilePath} not found. 无法自动生成补充元数据.");
                return;
            }
            string strippedAOTDllOutputRootDir = $"{Application.dataPath}/../{HybridCLR.Editor.Settings.HybridCLRSettings.Instance.strippedAOTDllOutputRootDir}/{buildTarget}";
            if (!Directory.Exists(strippedAOTDllOutputRootDir))
            {
                Debug.LogError($"{strippedAOTDllOutputRootDir} not found. 裁剪后的 AOT dll 存放目录不存在，无法自动生成补充元数据.");
                return;
            }

            List<string> aotDllNames = new List<string>();
            using (var reader = File.OpenText(aotGenericRefFilePath))
            {
                bool start = false;
                while (!reader.EndOfStream)
                {
                    // 只处理以 {{ AOT assemblies 为起始标记，以 }} 为结束标记的区间
                    string line = reader.ReadLine();
                    if (start)
                    {
                        if (line.Contains('{'))
                        {
                            continue;
                        }
                        else if (line.Contains("};"))
                        {
                            start = false;
                            break;
                        }
                        else
                        {
                            string aotDllName = line.Replace("\"", "").Replace("\"", "").Replace(",", "").Trim();
                            aotDllNames.Add(aotDllName);
                            Debug.Log(aotDllName);
                        }
                    }
                    else if (line.Contains("PatchedAOTAssemblyList"))
                    {
                        start = true;
                    }
                }
            }

            if (aotDllNames.Count == 0)
                return;

            // copy 数据文件
            string byteAOTDllFileDir = SettingsUtil.SupplementalMetadataDir;
            var strippedAOTDllOutputRootDirInfo = new DirectoryInfo(strippedAOTDllOutputRootDir);
            var fileInfos = strippedAOTDllOutputRootDirInfo.GetFiles();
            foreach (var aotDll in aotDllNames)
            {
                FileInfo targetFileInfo = null;
                foreach (var info in fileInfos)
                {
                    if (aotDll == info.Name)
                    {
                        targetFileInfo = info;
                        break;
                    }
                }

                if (targetFileInfo != null)
                {
                    string dstFilePath = $"{byteAOTDllFileDir}/{targetFileInfo.Name}.bytes";
                    File.Copy(targetFileInfo.FullName, dstFilePath, true);
                    Debug.Log($"生成补充元数据文件: {dstFilePath}.");
                }
                else
                {
                    Debug.LogError($"找不到裁剪后的 {strippedAOTDllOutputRootDirInfo}/{aotDll}，无法自动生成这个补充元数据.");
                }
            }

            // 生成补充元数据配置文件
            StringBuilder builder = new StringBuilder();
            builder.Append('[');
            for (int i = 0; i < aotDllNames.Count; ++i)
            {
                builder.Append('\"').Append(aotDllNames[i]).Append('\"');
                if (i != aotDllNames.Count - 1)
                    builder.Append(',');
            }
            builder.Append(']');

            string supplementalMetadataFile = $"{byteAOTDllFileDir}/SupplementalMetadata.json";
            using (var writer = File.CreateText(supplementalMetadataFile))
            {
                writer.Write(builder.ToString());
            }
            Debug.Log($"生成配置文件: {supplementalMetadataFile}");

            AssetDatabase.Refresh();
        }
    }
}
