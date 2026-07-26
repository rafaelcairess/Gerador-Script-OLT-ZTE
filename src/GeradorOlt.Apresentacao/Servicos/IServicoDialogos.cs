namespace GeradorOlt.Apresentacao.Servicos;

public interface IServicoDialogos
{
    string? SelectProfileToOpen();
    string? SelectProfileToSave();
    string? SelectScriptToSave(string suggestedName);
    void ShowError(string title, string message);
    void ShowInformation(string title, string message);
    void OpenFolder(string path);
}
