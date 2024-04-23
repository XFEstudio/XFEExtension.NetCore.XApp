using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Diagnostics;
using System.Reflection;
using XFEExtension.NetCore.XApp.Core;

namespace XFEExtension.NetCore.XApp;

public static class XAppLoader
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
            if (xAppCode.FileType == XAppFileType.XFML)
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
                    xAppCode.Code = xAppCode.Code?.Replace("[XFMLTOLOAD]", codeFile.Code);
                }
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
