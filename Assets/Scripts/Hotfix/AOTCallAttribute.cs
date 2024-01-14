using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Assertions;

/*
1. GameAppHotUpdate.dll 需要导出的属性、字段或方法，添加 AOTCallAttribute 特性；
2. 然后编译 GameAppHotUpdate；
3. 在 Unity 中运行 Tools/热更新相关/根据 AOTCall 特性自动导出 HotApis；
4. 会覆写 Program/Client/GameApp/GameAppHotUpdate/ExportInterfaceAuto.cs 和 Program/Client/GameApp/GameApp/HotUpdate/ImportInterfaceAuto.cs；
5. 在 GameApp 中通过 HotApis 调用；

对于导出的 Method 暂时不支持参数类型修饰符（ref，in，out，param）；
存在缺省参数的 Method，支持导出，但是不会填充缺省参数，在外部调用时，需要补全所有参数；

有扩展需求的，可以在 Unity 的 AnalyzeGameAppDll.cs 脚本中自己扩展

 */

public enum AccessType : byte
{
    /// <summary>
    /// 通过 Instance 调用特性目标
    /// </summary>
    Instance,
    /// <summary>
    /// 通过 GetInstance() 调用特性目标
    /// </summary>
    GetInstance,
    /// <summary>
    /// 通过静态方式调用特性目标
    /// </summary>
    Static,
    /// <summary>
    /// 自定义调用特性目标
    /// </summary>
    Custom,
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method)]
public class AOTCallAttribute : Attribute
{
    public AccessType aotCallType { get; private set; }

    public string customAccess { get; private set; }

    public AOTCallAttribute(AccessType callType, string accessPath = null)
    {
        UnityEngine.Debug.Assert(callType != AccessType.Custom || !string.IsNullOrEmpty(accessPath), $"AccessType={callType} 时，accessPath 不能为空。");
        aotCallType = callType;
        this.customAccess = accessPath;
    }
}
