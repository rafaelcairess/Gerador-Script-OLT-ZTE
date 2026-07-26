namespace GeradorOlt.Nucleo.Modelos;

public sealed record ResumoScript(
    int TotalPons,
    int TotalOnus,
    int TotalServices,
    int TotalLines);

public sealed record ScriptGerado(string Content, ResumoScript Summary);
