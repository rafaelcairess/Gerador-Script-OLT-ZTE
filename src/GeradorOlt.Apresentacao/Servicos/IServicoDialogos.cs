namespace GeradorOlt.Apresentacao.Servicos;

public interface IServicoDialogos
{
    public string? SelectProfileToOpen(string initialDirectory);
    public string? SelectProfileToSave(string initialDirectory);
    public string? SelectScriptToSave(string suggestedName, string initialDirectory);
    public bool Confirm(string title, string message);
    public void ShowError(string title, string message);
    public void ShowInformation(string title, string message);
    public void OpenFolder(string path);
}
