using System.Runtime.CompilerServices;

namespace OutSystems.UltimatePDF_ExternalLogic.E2ETests;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class E2EFactAttribute : FactAttribute {
    public E2EFactAttribute(
        [CallerFilePath] string? sourceFile = null,
        [CallerLineNumber] int sourceLine = 0)
        : base(sourceFile, sourceLine) {
        Timeout = 95_000;
    }
}
