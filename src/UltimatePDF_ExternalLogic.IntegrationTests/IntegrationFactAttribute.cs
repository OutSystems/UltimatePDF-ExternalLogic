using System.Runtime.CompilerServices;

namespace OutSystems.UltimatePDF_ExternalLogic.IntegrationTests;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class IntegrationFactAttribute : FactAttribute {
    public IntegrationFactAttribute(
        [CallerFilePath] string? sourceFile = null,
        [CallerLineNumber] int sourceLine = 0)
        : base(sourceFile, sourceLine) {
        Timeout = 95_000;
    }
}
