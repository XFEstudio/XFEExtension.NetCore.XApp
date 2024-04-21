using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Diagnostics;
using System.Reflection;
using XFEExtension.NetCore.XApp.Core;

namespace XFEExtension.NetCore.XApp;

public class XAppLoader
{
    public static async Task<Page?> GetMainPage(Core.XApp xApp)
    {
        var mainMethod = LoadAssemblyFromXApp(xApp)?.EntryPoint;
        if (mainMethod is not null)
        {
            if (mainMethod?.ReturnType == typeof(Task))
                await (Task)mainMethod.Invoke(null, [Array.Empty<string>()])!;
            else
                mainMethod?.Invoke(null, [Array.Empty<string>()]);
            return XAppBuilder.CurrentXAppMainPage;
        }
        else
        {
            return null;
        }
    }

    public static Assembly? LoadAssemblyFromXApp(Core.XApp xApp) => CompilateCode(xApp.AppFiles.CodeFiles);
    public static Assembly? CompilateCode(params XAppCode[] xAppCodes)
    {
        var syntaxTrees = new List<SyntaxTree>();
        foreach (var xAppCode in xAppCodes)
        {
            syntaxTrees.Add(SyntaxFactory.ParseSyntaxTree(xAppCode.Code!));
        }
        var compilationOptions = new CSharpCompilationOptions(OutputKind.WindowsApplication);
        var compilation = CSharpCompilation.Create("ProcessingAssembly")
            .WithOptions(compilationOptions)
            .AddReferences(AppDomain.CurrentDomain.GetAssemblies().Where(assembly => !assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location)).Select(assembly => MetadataReference.CreateFromFile(assembly.Location)))
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
