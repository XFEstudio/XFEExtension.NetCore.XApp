namespace XFEExtension.NetCore.XApp.Extensions;

public static class XAppExtensions
{
    public static async Task<Page?> GetMainPage(this Core.XApp xApp) => await XAppLoader.GetMainPage(xApp);
}
