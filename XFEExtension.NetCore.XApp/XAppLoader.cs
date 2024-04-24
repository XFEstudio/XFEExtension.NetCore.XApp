using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using XFEExtension.NetCore.XApp.Core;

namespace XFEExtension.NetCore.XApp;

/// <summary>
/// XApp加载器
/// </summary>
public static class XAppLoader
{
    /// <summary>
    /// 编译并获取主页面
    /// </summary>
    /// <param name="xApp">XApp对象</param>
    /// <returns>编译结果信息</returns>
    public static async Task<CompilateResult> GetMainPage(Core.XApp xApp)
    {
        var mainMethod = LoadAssemblyFromXApp(xApp, out var diagnostic)?.EntryPoint;
        if (mainMethod is not null)
        {
            if (mainMethod?.ReturnType == typeof(Task))
                await (Task)mainMethod.Invoke(null, [Array.Empty<string>()])!;
            else
                mainMethod?.Invoke(null, [Array.Empty<string>()]);
            var pageIsNotNull = XAppBuilder.CurrentXAppMainPage is not null;
            return new CompilateResult(pageIsNotNull, XAppBuilder.CurrentXAppMainPage, diagnostic);
        }
        else
        {
            return new CompilateResult(false, null, diagnostic);
        }
    }
    /// <summary>
    /// 从XApp对象中加载程序集
    /// </summary>
    /// <param name="xApp">XApp对象</param>
    /// <param name="diagnostic">诊断信息</param>
    /// <returns></returns>
    public static Assembly? LoadAssemblyFromXApp(Core.XApp xApp, out ImmutableArray<Diagnostic> diagnostic) => CompilateCode(out diagnostic, xApp.AppFiles.CodeFiles);
    /// <summary>
    /// 编译代码
    /// </summary>
    /// <param name="diagnostics">诊断信息</param>
    /// <param name="xAppCodes">XApp的代码文件</param>
    /// <returns></returns>
    public static Assembly? CompilateCode(out ImmutableArray<Diagnostic> diagnostics, params XAppCode[] xAppCodes)
    {
        var syntaxTrees = new List<SyntaxTree>();
        foreach (var xAppCode in xAppCodes)
        {
            if (xAppCode.FileType == XAppFileType.XFML || xAppCode.FileType == XAppFileType.Resource || xAppCode.FileType == XAppFileType.Image)
                continue;
            var fileName = Path.GetFileNameWithoutExtension(xAppCode.FileName);
            var split = fileName.Split('.');
            if (split.Length > 1)
            {
                fileName = split[0];
            }
            if (xAppCode.FileType == XAppFileType.CS && xAppCode.FileName.Contains(".g.cs"))
            {
                var autoGenCodeFile = xAppCodes.Where(code => code.FileType == XAppFileType.XFML && code.FileName.Contains($"{fileName}")).ToArray();
                if (autoGenCodeFile is not null && autoGenCodeFile.Length > 0)
                {
                    var codeFile = autoGenCodeFile[0];
                    xAppCode.Code = xAppCode.Code?.Replace("[XFMLTOLOAD]", codeFile.Code?.Replace("\"", "\"\""));
                }
                Console.WriteLine(xAppCode.Code);
            }
            syntaxTrees.Add(SyntaxFactory.ParseSyntaxTree(xAppCode.Code!));
        }
        var compilationOptions = new CSharpCompilationOptions(OutputKind.WindowsApplication);
        var compilation = CSharpCompilation.Create("ProcessingAssembly")
            .WithOptions(compilationOptions)
            .AddReferences
            (
                MetadataReference.CreateFromFile(Assembly.Load("System").Location),
                MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
                MetadataReference.CreateFromFile(Assembly.Load("System.Private.CoreLib").Location),
                MetadataReference.CreateFromFile(Assembly.Load("System.Runtime.Loader").Location),
                MetadataReference.CreateFromFile(Assembly.Load("System.Collections.Concurrent").Location),
                MetadataReference.CreateFromFile(Assembly.Load("System.Linq.Expressions").Location),
                MetadataReference.CreateFromFile(Assembly.Load("System.Collections").Location),
                MetadataReference.CreateFromFile(Assembly.Load("System.Linq").Location),
                MetadataReference.CreateFromFile(Assembly.Load("System.Threading").Location),
                MetadataReference.CreateFromFile(Assembly.Load("System.Threading.Tasks").Location),
                MetadataReference.CreateFromFile(Assembly.Load("Microsoft.CSharp").Location),
                MetadataReference.CreateFromFile(Assembly.Load("System.ObjectModel").Location),
                MetadataReference.CreateFromFile(Assembly.Load("System.Text.Encoding.Extensions").Location),
                MetadataReference.CreateFromFile(Assembly.Load("System.Collections.Immutable").Location),
                MetadataReference.CreateFromFile(Assembly.Load("System").Location),
                MetadataReference.CreateFromFile(Assembly.Load("Microsoft.Maui").Location),
                MetadataReference.CreateFromFile(Assembly.Load("Microsoft.Maui.Controls").Location),
                MetadataReference.CreateFromFile(Assembly.Load("Microsoft.Maui.Controls.Xaml").Location),
                MetadataReference.CreateFromFile(Assembly.Load("XFEExtension.NetCore.XApp.Core").Location),
                MetadataReference.CreateFromFile(Assembly.Load("XFEExtension.NetCore.XApp").Location),
                MetadataReference.CreateFromFile(Assembly.Load("Microsoft.Maui.Essentials").Location),
                MetadataReference.CreateFromFile(Assembly.Load("Microsoft.Maui.Graphics").Location)
            )
            .AddSyntaxTrees(syntaxTrees);
        byte[] assemblyBytes;
        using var stream = new MemoryStream();
        var result = compilation.Emit(stream);
        foreach (var diagnostic in result.Diagnostics)
        {
            if (diagnostic.Severity == DiagnosticSeverity.Error)
                Trace.WriteLine($"无法编译：行{diagnostic.Location.GetLineSpan().StartLinePosition.Line + 1}，列{diagnostic.Location.GetLineSpan().StartLinePosition.Character + 1}：{diagnostic.GetMessage()}");
            else if (diagnostic.Severity == DiagnosticSeverity.Warning)
                Trace.WriteLine($"警告：行{diagnostic.Location.GetLineSpan().StartLinePosition.Line + 1}，列{diagnostic.Location.GetLineSpan().StartLinePosition.Character + 1}：{diagnostic.GetMessage()}");
            else if (diagnostic.Severity == DiagnosticSeverity.Info)
                Trace.WriteLine($"提示：行{diagnostic.Location.GetLineSpan().StartLinePosition.Line + 1}，列{diagnostic.Location.GetLineSpan().StartLinePosition.Character + 1}：{diagnostic.GetMessage()}");
            else
                Trace.WriteLine(diagnostic.ToString());
        }
        diagnostics = result.Diagnostics;
        if (result.Success)
        {
            assemblyBytes = stream.ToArray();
            Trace.WriteLine("编译成功");
            return Assembly.Load(assemblyBytes);
        }
        else
        {
            return null;
        }
    }
}
