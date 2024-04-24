namespace XFEExtension.NetCore.XApp.Extensions;

/// <summary>
/// XApp拓展
/// </summary>
public static class XAppExtensions
{
    /// <summary>
    /// 获取主页面
    /// </summary>
    /// <param name="xApp">XApp应用</param>
    /// <returns></returns>
    public static async Task<CompilateResult> GetMainPage(this Core.XApp xApp) => await XAppLoader.GetMainPage(xApp);
}
