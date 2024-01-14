using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HybridCLR
{
    public static class ExportRuntimeApis
    {
        public static bool Enable
        {
            get
            {
#if ENABLE_HYBRIDCLR
                return true;
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// 加载补充元数据assembly
        /// </summary>
        /// <param name="dllBytes"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public static LoadImageErrorCode LoadMetadataForAOTAssembly(byte[] dllBytes, HomologousImageMode mode)
        {
#if ENABLE_HYBRIDCLR
            return RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, mode);
#else
            return LoadImageErrorCode.DISABLE_HYBRIDCLR;
#endif
        }


        /// <summary>
        /// 获取解释器线程栈的最大StackObject个数(size*8 为最终占用的内存大小)
        /// </summary>
        /// <returns></returns>
        public static int GetInterpreterThreadObjectStackSize()
        {
#if ENABLE_HYBRIDCLR
            return RuntimeApi.GetInterpreterThreadObjectStackSize();
#else
            return 0;
#endif
        }

        /// <summary>
        /// 设置解释器线程栈的最大StackObject个数(size*8 为最终占用的内存大小)
        /// </summary>
        /// <param name="size"></param>
        public static void SetInterpreterThreadObjectStackSize(int size)
        {
#if ENABLE_HYBRIDCLR
            RuntimeApi.SetInterpreterThreadObjectStackSize(size);
#endif
        }

        /// <summary>
        /// 获取解释器线程函数帧数量(sizeof(InterpreterFrame)*size 为最终占用的内存大小)
        /// </summary>
        /// <returns></returns>
        public static int GetInterpreterThreadFrameStackSize()
        {
#if ENABLE_HYBRIDCLR
            return RuntimeApi.GetInterpreterThreadFrameStackSize();
#else
            return 0;
#endif
        }

        /// <summary>
        /// 设置解释器线程函数帧数量(sizeof(InterpreterFrame)*size 为最终占用的内存大小)
        /// </summary>
        /// <param name="size"></param>
        public static void SetInterpreterThreadFrameStackSize(int size)
        {
#if ENABLE_HYBRIDCLR
            RuntimeApi.SetInterpreterThreadFrameStackSize(size);
#endif
        }
    }
}
