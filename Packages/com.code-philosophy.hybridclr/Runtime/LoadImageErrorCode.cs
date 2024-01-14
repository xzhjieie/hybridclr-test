
namespace HybridCLR
{
    public enum LoadImageErrorCode
	{
		OK = 0,
		BAD_IMAGE, // dll 不合法
		NOT_IMPLEMENT, // 不支持的元数据特性
		AOT_ASSEMBLY_NOT_FIND, // 对应的AOT assembly未找到
		HOMOLOGOUS_ONLY_SUPPORT_AOT_ASSEMBLY, // 不能给解释器assembly补充元数据
		HOMOLOGOUS_ASSEMBLY_HAS_LOADED, // 已经补充过了，不能再次补充
		INVALID_HOMOLOGOUS_MODE, // 非法HomologousImageMode
		DISABLE_HYBRIDCLR, // 未添加宏定义 ENABLE_HYBRIDCLR，调用 RuntimeApi 类型中的接口无效
	};
}

