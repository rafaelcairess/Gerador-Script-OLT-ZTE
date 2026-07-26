using System.Globalization;
using System.IO;
using GeradorOlt.Apresentacao.Infraestrutura;
using GeradorOlt.Apresentacao.Servicos;
using GeradorOlt.Nucleo.Exportacao;
using GeradorOlt.Nucleo.Geracao;
using GeradorOlt.Nucleo.Perfis;
using GeradorOlt.Nucleo.Validacao;

namespace GeradorOlt.Apresentacao.ModelosVisao;

public sealed class PrincipalViewModel : BaseViewModel
{
    private readonly IGeradorScript _scriptGenerator;
    private readonly IServicoPerfil _profileService;
    private readonly IServicoExportacaoScript _scriptExportService;
    private readonly IServicoDialogos _dialogService;
    private readonly IServicoAreaTransferencia _clipboardService;
    private readonly CultureInfo _culture = CultureInfo.GetCultureInfo("pt-BR");

    private string _lastFolder = Environment.CurrentDirectory;
    private string _scriptPreview = string.Empty;
    private string _totalPons = "—";
    private string _totalOnus = "—";
    private string _totalServices = "—";
    private string _totalLines = "— linhas";
    private string _statusMessage = "Aguardando configuração";
    private bool _statusIsError;
    private bool _hasValidScript;

    public PrincipalViewModel(
        IGeradorScript scriptGenerator,
        IServicoPerfil profileService,
        IServicoExportacaoScript scriptExportService,
        IServicoDialogos dialogService,
        IServicoAreaTransferencia clipboardService)
    {
        _scriptGenerator = scriptGenerator;
        _profileService = profileService;
        _scriptExportService = scriptExportService;
        _dialogService = dialogService;
        _clipboardService = clipboardService;

        Configuration = new ConfiguracaoViewModel();
        Configuration.Changed += (_, _) => RefreshPreview();

        GenerateCommand = new Comando(
            () => RefreshPreview("Script validado e atualizado com sucesso."));
        CopyCommand = new Comando(CopyScript, () => HasValidScript);
        ExportCommand = new Comando(ExportScript, () => HasValidScript);
        SaveProfileCommand = new Comando(SaveProfile);
        LoadProfileCommand = new Comando(LoadProfile);
        NewScenarioCommand = new Comando(NewScenario);
        OpenFolderCommand = new Comando(OpenFolder);
        AboutCommand = new Comando(ShowAbout);

        RefreshPreview();
        TryLoadDefaultProfile();
    }

    public ConfiguracaoViewModel Configuration { get; }

    public Comando GenerateCommand { get; }
    public Comando CopyCommand { get; }
    public Comando ExportCommand { get; }
    public Comando SaveProfileCommand { get; }
    public Comando LoadProfileCommand { get; }
    public Comando NewScenarioCommand { get; }
    public Comando OpenFolderCommand { get; }
    public Comando AboutCommand { get; }

    public string ScriptPreview
    {
        get => _scriptPreview;
        private set => SetProperty(ref _scriptPreview, value);
    }

    public string TotalPons
    {
        get => _totalPons;
        private set => SetProperty(ref _totalPons, value);
    }

    public string TotalOnus
    {
        get => _totalOnus;
        private set => SetProperty(ref _totalOnus, value);
    }

    public string TotalServices
    {
        get => _totalServices;
        private set => SetProperty(ref _totalServices, value);
    }

    public string TotalLines
    {
        get => _totalLines;
        private set => SetProperty(ref _totalLines, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public bool StatusIsError
    {
        get => _statusIsError;
        private set => SetProperty(ref _statusIsError, value);
    }

    public bool HasValidScript
    {
        get => _hasValidScript;
        private set
        {
            if (SetProperty(ref _hasValidScript, value))
            {
                CopyCommand.RaiseCanExecuteChanged();
                ExportCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private void RefreshPreview(string? successMessage = null)
    {
        try
        {
            var generated = _scriptGenerator.Generate(Configuration.BuildConfiguration());
            ScriptPreview = generated.Content;
            TotalPons = FormatNumber(generated.Summary.TotalPons);
            TotalOnus = FormatNumber(generated.Summary.TotalOnus);
            TotalServices = FormatNumber(generated.Summary.TotalServices);
            TotalLines = $"{FormatNumber(generated.Summary.TotalLines)} linhas";
            SetStatus(
                successMessage ?? "Configuração válida • pronta para exportar",
                isError: false);
            HasValidScript = true;
        }
        catch (Exception exception)
            when (exception is ExcecaoValidacaoConfiguracao or FormatException)
        {
            ScriptPreview =
                "# O preview aparecerá aqui quando a configuração estiver válida."
                + Environment.NewLine + Environment.NewLine
                + $"# {exception.Message}";
            TotalPons = "—";
            TotalOnus = "—";
            TotalServices = "—";
            TotalLines = "— linhas";
            SetStatus(exception.Message, isError: true);
            HasValidScript = false;
        }
    }

    private void CopyScript()
    {
        try
        {
            _clipboardService.SetText(ScriptPreview);
            SetStatus("Script copiado para a área de transferência.", isError: false);
        }
        catch (Exception exception)
        {
            _dialogService.ShowError("Não foi possível copiar", exception.Message);
        }
    }

    private void ExportScript()
    {
        var fileName =
            $"script_pon{Configuration.PonStart}-{Configuration.PonEnd}"
            + $"_vlan{Configuration.Vlan}.txt";
        var path = _dialogService.SelectScriptToSave(fileName);
        if (path is null)
        {
            return;
        }

        try
        {
            _scriptExportService.Export(path, ScriptPreview);
            UpdateLastFolder(path);
            SetStatus($"Arquivo {Path.GetFileName(path)} exportado.", isError: false);
        }
        catch (Exception exception) when (IsFileException(exception))
        {
            _dialogService.ShowError("Não foi possível exportar", exception.Message);
        }
    }

    private void SaveProfile()
    {
        var path = _dialogService.SelectProfileToSave();
        if (path is null)
        {
            return;
        }

        try
        {
            _profileService.Save(path, Configuration.ToProfile());
            UpdateLastFolder(path);
            SetStatus($"Perfil {Path.GetFileName(path)} salvo.", isError: false);
        }
        catch (Exception exception) when (IsFileException(exception))
        {
            _dialogService.ShowError("Não foi possível salvar o perfil", exception.Message);
        }
    }

    private void LoadProfile()
    {
        var path = _dialogService.SelectProfileToOpen();
        if (path is null)
        {
            return;
        }

        try
        {
            Configuration.Apply(_profileService.Load(path));
            UpdateLastFolder(path);
            SetStatus($"Perfil {Path.GetFileName(path)} carregado.", isError: false);
        }
        catch (Exception exception) when (IsFileException(exception))
        {
            _dialogService.ShowError("Não foi possível carregar o perfil", exception.Message);
        }
    }

    private void TryLoadDefaultProfile()
    {
        var path = Path.Combine(Environment.CurrentDirectory, "perfil_config.json");
        if (!File.Exists(path))
        {
            return;
        }

        try
        {
            Configuration.Apply(_profileService.Load(path));
        }
        catch (Exception exception) when (IsFileException(exception))
        {
            SetStatus($"Perfil automático ignorado: {exception.Message}", isError: true);
        }
    }

    private void NewScenario()
    {
        Configuration.Reset();
        SetStatus("Novo cenário iniciado.", isError: false);
    }

    private void OpenFolder()
    {
        try
        {
            _dialogService.OpenFolder(_lastFolder);
        }
        catch (Exception exception)
            when (exception is InvalidOperationException
                or System.ComponentModel.Win32Exception)
        {
            _dialogService.ShowError("Não foi possível abrir a pasta", exception.Message);
        }
    }

    private void ShowAbout() =>
        _dialogService.ShowInformation(
            "Sobre o Gerador OLT ZTE",
            "Gerador de Scripts OLT ZTE • versão 2.0"
            + Environment.NewLine + Environment.NewLine
            + "Automação de provisionamento para OLTs ZTE Titan."
            + Environment.NewLine
            + "Arquitetura C# • WPF • MVVM"
            + Environment.NewLine + Environment.NewLine
            + "Rafael Caires & Lucas Alexandre");

    private string FormatNumber(int number) => number.ToString("N0", _culture);

    private void UpdateLastFolder(string path) =>
        _lastFolder = Path.GetDirectoryName(path) ?? Environment.CurrentDirectory;

    private void SetStatus(string message, bool isError)
    {
        StatusMessage = message;
        StatusIsError = isError;
    }

    private static bool IsFileException(Exception exception) =>
        exception is IOException or UnauthorizedAccessException or InvalidDataException;
}
