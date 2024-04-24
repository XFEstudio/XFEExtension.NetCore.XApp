using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace XFEExtension.NetCore.XApp;
/// <summary>
/// XApp编译结果
/// </summary>
/// <param name="IsCompilateSuccessful">是否编译成功</param>
/// <param name="Page">程序主页面</param>
/// <param name="Diagnostics">诊断信息</param>
public record class CompilateResult(bool IsCompilateSuccessful, Page? Page, ImmutableArray<Diagnostic> Diagnostics) { }
