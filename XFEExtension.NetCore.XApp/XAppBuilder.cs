using XFEExtension.NetCore.ImplExtension;

namespace XFEExtension.NetCore.XApp;

[CreateImpl]
public abstract class XAppBuilder
{
    internal static Page? CurrentXAppMainPage { get; private set; }
    public Page? MainPage { get; set; }
    public Shell? MainShell { get; set; }
    public static XAppBuilder CreateBuilder() => new XAppBuilderImpl();
    public XAppBuilder WithShellDev(Shell mainShell)
    {
        MainShell = mainShell;
        return this;
    }

    public XAppBuilder WithPageDev(Page mainPage)
    {
        MainPage = mainPage;
        return this;
    }

    public void BuildAndRun() => CurrentXAppMainPage = MainPage ?? MainShell;
}
