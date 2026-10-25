using System.Diagnostics;
using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace GeradorOlt.Apresentacao.Servicos;

public sealed class ServicoDialogosDesktop : IServicoDialogos
{
    public string? SelectProfileToOpen(string initialDirectory)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Carregar perfil",
            Filter = "Perfil JSON (*.json)|*.json|Todos os arquivos (*.*)|*.*",
            InitialDirectory = GetExistingDirectory(initialDirectory)
        };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public string? SelectProfileToSave(string initialDirectory)
    {
        var dialog = new SaveFileDialog
        {
            Title = "Salvar perfil",
            FileName = "perfil_config.json",
            DefaultExt = ".json",
            Filter = "Perfil JSON (*.json)|*.json|Todos os arquivos (*.*)|*.*",
            InitialDirectory = GetExistingDirectory(initialDirectory)
        };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public string? SelectScriptToSave(string suggestedName, string initialDirectory)
    {
        var dialog = new SaveFileDialog
        {
            Title = "Exportar script",
            FileName = suggestedName,
            DefaultExt = ".txt",
            Filter = "Arquivo de texto (*.txt)|*.txt|Todos os arquivos (*.*)|*.*",
            InitialDirectory = GetExistingDirectory(initialDirectory)
        };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public bool Confirm(string title, string message) =>
        MessageBox.Show(
            message,
            title,
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning,
            MessageBoxResult.No) == MessageBoxResult.Yes;

    public void ShowError(string title, string message) =>
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);

    public void ShowInformation(string title, string message) =>
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);

    public void OpenFolder(string path)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "explorer.exe",
            Arguments = $"\"{path}\"",
            UseShellExecute = true
        });
    }

    private static string GetExistingDirectory(string path) =>
        Directory.Exists(path) ? path : AppContext.BaseDirectory;
}
