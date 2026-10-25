using System.IO;

namespace GeradorOlt.Apresentacao.Servicos;

public sealed class CaminhosAplicacao : ICaminhosAplicacao
{
    private const string ApplicationFolderName = "GeradorOltZte";

    public CaminhosAplicacao()
    {
        var applicationData = Environment.GetFolderPath(
            Environment.SpecialFolder.ApplicationData);
        DefaultProfilePath = Path.Combine(
            applicationData,
            ApplicationFolderName,
            "perfil_config.json");
        LegacyDefaultProfilePath = Path.Combine(
            AppContext.BaseDirectory,
            "perfil_config.json");

        var documents = Environment.GetFolderPath(
            Environment.SpecialFolder.MyDocuments);
        InitialDirectory = Directory.Exists(documents)
            ? documents
            : AppContext.BaseDirectory;
    }

    public string DefaultProfilePath { get; }
    public string LegacyDefaultProfilePath { get; }
    public string InitialDirectory { get; }
}
