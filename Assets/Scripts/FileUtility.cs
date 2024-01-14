using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using System.Net.Http;
using System.Net;

namespace GameApp
{
    public static class FileUtility
    {
        public static readonly string PersistentDataPath;
        public static readonly string StreamingAssetsPath;

        static FileUtility()
        {
            PersistentDataPath = Application.persistentDataPath;
            StreamingAssetsPath = Application.streamingAssetsPath;
        }

        /// <summary>
        /// UNITY_ANDROID 和 UNITY_WEBGL 平台下，需要用到 UnityWebRequest，无法支持多线程，其它平台都可以支持
        /// </summary>
        /// <param name="filePathRelativeToStreamingAssets"></param>
        /// <returns></returns>
        public static byte[] ReadAllBytesFromStreamingAssets(string filePathRelativeToStreamingAssets)
        {
            byte[] result = null;
            string url = Path.Combine(StreamingAssetsPath, filePathRelativeToStreamingAssets);
#if UNITY_ANDROID || UNITY_WEBGL
            var request = UnityWebRequest.Get(url);
            request.timeout = 3;
            request.SendWebRequest();
            while (true)
            {
                if (!string.IsNullOrEmpty(request.error))
                {
                    Debug.LogError($"[FileUtility] ReadAllBytesFromStreamingAssets Failed. url={url}, error={request.error}");
                    break;
                }
                else if (request.downloadHandler.isDone)
                {
                    result = request.downloadHandler.data;
                    //Debug.Log($"[FileUtility] ReadBytesFromStreamingAssets Success. url={url}");
                    break;
                }
            }
            request.Dispose();
            return result;
#else
            if (File.Exists(url))
            {
                try
                {
                    result = File.ReadAllBytes(url);
                    //Debug.Log($"[FileUtility] ReadAllBytesFromStreamingAssets Success. url={url}");
                }
                catch (Exception exception)
                {
                    Debug.LogError($"[FileUtility] {exception.Message}\n{exception.StackTrace}");
                    result = null;
                }
            }
            else
            {
                Debug.LogError($"[FileUtility] ReadAllBytesFromStreamingAssets Failed. The file<{url}> don't existed.");
            }
            return result;
#endif
        }

        /// <summary>
        /// UNITY_ANDROID 和 UNITY_WEBGL 平台下，需要用到 UnityWebRequest，无法支持多线程，其它平台都可以支持
        /// </summary>
        /// <param name="filePathRelativeToStreamingAssets"></param>
        /// <returns></returns>
        public static string ReadAllTextFromStreamingAssets(string filePathRelativeToStreamingAssets)
        {
            string result = null;
            string url = Path.Combine(StreamingAssetsPath, filePathRelativeToStreamingAssets);
#if UNITY_ANDROID || UNITY_WEBGL
            var request = UnityWebRequest.Get(url);
            request.timeout = 3;
            request.SendWebRequest();
            while (true)
            {
                if (!string.IsNullOrEmpty(request.error))
                {
                    Debug.LogError($"[FileUtility] ReadAllTextFromStreamingAssets Failed. url={url}, error={request.error}");
                    break;
                }
                else if (request.downloadHandler.isDone)
                {
                    result = request.downloadHandler.text;
                    //Debug.Log($"[FileUtility] ReadBytesFromStreamingAssets Success. url={url}");
                    break;
                }
            }
            request.Dispose();
            return result;
#else
            if (File.Exists(url))
            {
                try
                {
                    result = File.ReadAllText(url);
                    //Debug.Log($"[FileUtility] ReadAllTextFromStreamingAssets Success. url={url}");
                }
                catch (Exception exception)
                {
                    Debug.LogError($"[FileUtility] {exception.Message}\n{exception.StackTrace}");
                    result = null;
                }
            }
            else
            {
                Debug.LogError($"[FileUtility] ReadAllTextFromStreamingAssets Failed. The file<{url}> don't existed.");
            }
            return result;
#endif

        }

        public static bool TryReadAllBytes(string filepath, out byte[] result)
        {
            result = null;

            if (!File.Exists(filepath))
                return false;

            result = File.ReadAllBytes(filepath);
            return true;
        }

        public static bool TryReadAllText(string filepath, out string result)
        {
            result = null;
            if (!File.Exists(filepath))
                return false;

            result = File.ReadAllText(filepath);
            return true;
        }
    }
}