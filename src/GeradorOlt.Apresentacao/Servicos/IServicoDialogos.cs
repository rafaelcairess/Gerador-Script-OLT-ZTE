namespace GeradorOlt.Apresentacao.Servicos;

public interface IServicoDialogos
{
    public string? SelectProfileToOpen();
    public string? SelectProfileToSave();
    public string? SelectScriptToSave(string suggestedName);
    public void ShowError(string title, string message);
    public void ShowInformation(string title, string message);
    public void OpenFolder(string path);
}
