using XFEExtension.NetCore.ImplExtension;

namespace XFEExtension.NetCore.XApp;

/// <summary>
/// XApp应用构建器
/// </summary>
[CreateImpl]
public abstract class XAppBuilder
{
    internal static Page? CurrentXAppMainPage { get; private set; }
    /// <summary>
    /// 主页（Page布局）
    /// </summary>
    public Page? MainPage { get; set; }
    /// <summary>
    /// 主页（Shell布局）
    /// </summary>
    public Shell? MainShell { get; set; }
    /// <summary>
    /// 创建构建器
    /// </summary>
    /// <returns></returns>
    public static XAppBuilder CreateBuilder() => new XAppBuilderImpl();
    /// <summary>
    /// 使用Shell布局
    /// </summary>
    /// <param name="mainShell"></param>
    /// <returns></returns>
    public XAppBuilder WithShellDev(Shell mainShell)
    {
        MainShell = mainShell;
        return this;
    }
    /// <summary>
    /// 使用Page布局
    /// </summary>
    /// <param name="mainPage"></param>
    /// <returns></returns>
    public XAppBuilder WithPageDev(Page mainPage)
    {
        MainPage = mainPage;
        return this;
    }
    /// <summary>
    /// 构建并运行
    /// </summary>
    public void BuildAndRun() => CurrentXAppMainPage = MainPage ?? MainShell;
}
