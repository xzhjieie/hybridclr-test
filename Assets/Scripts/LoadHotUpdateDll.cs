#if !M3PRODUCTION && (UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_WIN)
//#define ENABLE_INTRANET_TEST
#endif

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using HybridCLR;
using UnityEngine.Networking;
using Newtonsoft.Json;
using UnityEditorInternal;
using System.Collections;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;

namespace GameApp
{
    public static class LoadHotUpdateDll
    {
        private static readonly List<string> s_HotUpdateDllNames = new List<string>(2) {
            "Hotfix"
        };

        static Dictionary<string, Assembly> s_DllDict = new Dictionary<string, Assembly>();
        public static Dictionary<string, Assembly> DllDict
        {
            get => s_DllDict;
        }

        public static bool IsLoaded { get; private set; } = false;

        public static bool Enable { get => HybridCLR.ExportRuntimeApis.Enable; }

        private readonly static List<RuntimePlatform> s_SupportedPlatforms = new List<RuntimePlatform>(5) {
            RuntimePlatform.Android,
            RuntimePlatform.IPhonePlayer,
            RuntimePlatform.WindowsPlayer,
            RuntimePlatform.OSXEditor,
            RuntimePlatform.WindowsEditor
        };

        // 内网包测试使用
#if ENABLE_INTRANET_TEST
        readonly static string s_ClientRevision;
        readonly static bool s_DownloadFromIntranet;
#endif
        static LoadHotUpdateDll()
        {
#if ENABLE_INTRANET_TEST
            s_DownloadFromIntranet = false;
            s_ClientRevision = "";
            if (Application.platform == RuntimePlatform.Android ||
                Application.platform == RuntimePlatform.IPhonePlayer ||
                Application.platform == RuntimePlatform.WindowsPlayer)
            {
                // 读取 svn 版本号，在打内网包时，会复制 Assets/GameRes/LuaProj/BuildInfo.lua.txt 到 Assets/Resources/BuildInfo.lua.txt
                var textAsset = Resources.Load<TextAsset>("BuildInfo.lua");
                if (textAsset != null)
                {
                    var kRegex = new Regex(@"ClientRevision='Revision: \d+'");
                    var kMatch = kRegex.Match(textAsset.text);
                    if (kMatch != null && !string.IsNullOrEmpty(kMatch.Value))
                    {
                        int iBegin = kMatch.Value.IndexOf(':') + 1;
                        int iEnd = kMatch.Value.LastIndexOf('\'');
                        s_ClientRevision = kMatch.Value.Substring(iBegin, iEnd - iBegin);
                        s_ClientRevision = s_ClientRevision.Trim();
                        s_DownloadFromIntranet = true;
                        Debug.Log($"[LoadDll] ClientRevision={s_ClientRevision}");
                    }
                }
            }
#endif
        }

        public static void Load()
        {
            if (IsLoaded)
            {
                Debug.LogError("[LoadDll] Tried to load Hotfix.dll twice in one session!");
                return;
            }

            if (Enable && !s_SupportedPlatforms.Contains(Application.platform))
            {
                throw new NotSupportedException($"HybridCLR don't support {Application.platform} platform.");
            }

            if (Enable)
            {
                float fBeginTime = UnityEngine.Time.realtimeSinceStartup;
                LoadMetadataForAOTAssemblies();
                float fEndTime = UnityEngine.Time.realtimeSinceStartup;
                Debug.Log($"[LoadDll] LoadMetadataForAOTAssemblies, Time={(fEndTime - fBeginTime) * 1000}ms");
            }

            foreach (var dllName in s_HotUpdateDllNames)
            {
                Assembly ass = null;
                if (!Application.isEditor && Enable)
                {
                    float fBeginTime = UnityEngine.Time.realtimeSinceStartup;

                    byte[] kData = null;
#if ENABLE_INTRANET_TEST
                    if (s_DownloadFromIntranet)
                    {
                        // 内网测试使用，直接同步下载即可；异步的话有很多初始化脚本要修改，太麻烦了
                        string kUrl = $"http://192.168.0.24/HotUpdateDlls/{Application.platform}_{s_ClientRevision}_{dllName}.dll.bytes";
                        kData = DownloadDllBytesFile(kUrl);
                    }
#endif
                    // 从本地读取
                    if (kData == null)
                        kData = ReadDllBytesFile($"{dllName}.dll.bytes");

                    if (kData != null)
                        ass = Assembly.Load(kData);

                    kData = null;

                    float fEndTime = UnityEngine.Time.realtimeSinceStartup;
                    Debug.Log($"[LoadDll] Load {dllName}, Time={(fEndTime - fBeginTime) * 1000}ms");
                }
                else
                {
                    ass = System.AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == dllName);
                }
                s_DllDict.Add(dllName, ass);
            }

            foreach (var pair in s_DllDict)
                OnHotfixDllLoaded(pair.Value);

            HotApis.InitGameAppInterface();

            IsLoaded = true;
        }

        static void OnHotfixDllLoaded(Assembly hotfixDll)
        {
            var go = new GameObject();
            foreach (var type in hotfixDll.GetTypes())
            {
                if (type.IsAssignableFrom(typeof(MonoBehaviour)))
                {
                    Debug.Log($"[LoadDll] AddCompoent: {type.Name}");
                    go.AddComponent(type);
                }
            }
            GameObject.Destroy(go);
        }

        /// <summary>
        /// 为aot assembly加载原始metadata， 这个代码放aot或者热更新都行。
        /// 一旦加载后，如果AOT泛型函数对应native实现不存在，则自动替换为解释模式执行
        /// </summary>
        static void LoadMetadataForAOTAssemblies()
        {
            /// 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据。
            /// 热更新dll不缺元数据，不需要补充，如果调用LoadMetadataForAOTAssembly会返回错误

            var jsonText = ReadDllConfigFile("SupplementalMetadata.json");
            var aotMetaAssemblyFiles = JsonConvert.DeserializeObject<List<string>>(jsonText);
            HomologousImageMode kMode = HomologousImageMode.SuperSet;
            // UNITY_ANDROID 和 UNITY_WEBGL 平台下，需要用到 UnityWebRequest，无法支持多线程，其它平台都可以支持
#if UNITY_ANDROID || UNITY_WEBGL
            foreach (var aotDllName in aotMetaAssemblyFiles)
            {
                byte[] dllBytes = ReadDllBytesFile($"{aotDllName}.bytes");
                // 加载assembly对应的dll，会自动为它hook。一旦aot泛型函数的native函数不存在，用解释器版本代码
                LoadImageErrorCode err = ExportRuntimeApis.LoadMetadataForAOTAssembly(dllBytes, kMode);
                if (err == LoadImageErrorCode.OK)
                {
#if !M3PRODUCTION && !M3PROFILER
                    Debug.Log($"[LoadDll] LoadMetadataForAOTAssembly:{aotDllName}. mode:{kMode} ret:{err}");
#endif
                }
                else
                {
                    Debug.LogError($"[LoadDll] LoadMetadataForAOTAssembly:{aotDllName}. mode:{kMode} ret:{err}");
                }
            }
#else
            int[] kUsingFlags = new int[aotMetaAssemblyFiles.Count];
            string[] kLogs = new string[aotMetaAssemblyFiles.Count];
            // 最多开4个任务
            int iTaskCnt = Mathf.Clamp(SystemInfo.processorCount, 1, 4);
            Task[] kTasks = new Task[iTaskCnt];
            for (int i = 0; i < kTasks.Length; ++i)
            {
                kTasks[i] = Task.Factory.StartNew(() =>
                {
                    for (int k = 0; k < kUsingFlags.Length; ++k)
                    {
                        if (0 == Interlocked.Exchange(ref kUsingFlags[k], 1))
                        {
                            var aotDllName = aotMetaAssemblyFiles[k];
                            byte[] dllBytes = ReadDllBytesFile($"{aotDllName}.bytes");
                            // 加载assembly对应的dll，会自动为它hook。一旦aot泛型函数的native函数不存在，用解释器版本代码
                            LoadImageErrorCode err = ExportRuntimeApis.LoadMetadataForAOTAssembly(dllBytes, kMode);
                            if (err == LoadImageErrorCode.OK)
                            {
#if !M3PRODUCTION && !M3PROFILER
                                kLogs[k] = $"0[LoadDll] LoadMetadataForAOTAssembly:{aotDllName}. mode:{kMode} ret:{err}";
#endif
                            }
                            else
                            {
                                kLogs[k] = $"1[LoadDll] LoadMetadataForAOTAssembly:{aotDllName}. mode:{kMode} ret:{err}";
                            }
                        }
                    }
                });
            }

            // 等待完成后，在主线程执行日志输出
            Task.WaitAll(kTasks);

            foreach (var kLog in kLogs)
            {
                if (kLog != null)
                {
                    if (kLog.StartsWith("1"))
                    {
                        Debug.LogError(kLog);
                    }
                    else
                    {
#if !M3PRODUCTION && !M3PROFILER
                        Debug.Log(kLog);
#endif
                    }
                }
            }
#endif
        }

        static byte[] ReadDllBytesFile(string filename)
        {
            // persistentDataPath 存在就从这里读取，方便做测试
            if (FileUtility.TryReadAllBytes($"{FileUtility.PersistentDataPath}/HotUpdateDlls/{filename}", out var result))
            {
                return result;
            }
            else
            {
                return FileUtility.ReadAllBytesFromStreamingAssets($"HotUpdateDlls/{filename}");
            }
        }

        static string ReadDllConfigFile(string filename)
        {
            // persistentDataPath 存在就从这里读取，方便做测试
            if (FileUtility.TryReadAllText($"{FileUtility.PersistentDataPath}/HotUpdateDlls/{filename}", out var result))
            {
                return result;
            }
            else
            {
                return FileUtility.ReadAllTextFromStreamingAssets($"HotUpdateDlls/{filename}");
            }
        }

#if ENABLE_INTRANET_TEST
        static byte[] DownloadDllBytesFile(string kUrl)
        {
            byte[] kData = null;

            var fStartTime = UnityEngine.Time.realtimeSinceStartup;

            var request = UnityWebRequest.Get(kUrl);
            request.timeout = 3;
            request.SendWebRequest();
            while (true)
            {
                if (request.downloadHandler.isDone)
                {
                    kData = request.downloadHandler.data;
                    Debug.Log($"[LoadDll] DownloadDllBytesFile Success. url={kUrl}");
                    break;
                }
                else if (request.isHttpError || request.isNetworkError)
                {
                    Debug.LogWarning($"[LoadDll] DownloadDllBytesFile Failed. url={kUrl},code={request.responseCode}, error={request.error}");
                    break;
                }
                //yield return Yielders.EndOfFrame;
            }
            request.Dispose();

            var fEndTime = UnityEngine.Time.realtimeSinceStartup;
            Debug.Log($"[LoadDll] DownloadDllBytesFile, Time={(fEndTime - fStartTime) * 1000}ms");
            return kData;
        }
#endif
    }
}