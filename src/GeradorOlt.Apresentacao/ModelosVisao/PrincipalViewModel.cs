using System.Globalization;
using System.IO;
using System.Reflection;
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
    private readonly ICaminhosAplicacao _applicationPaths;
    private readonly IAgendadorAtualizacao _previewScheduler;
    private readonly CultureInfo _culture = CultureInfo.GetCultureInfo("pt-BR");
    private readonly string _applicationVersion = GetApplicationVersion();

    private string _lastFolder;
    private string _scriptPreview = string.Empty;
    private string _totalPons = "—";
    private string _totalOnus = "—";
    private string _totalServices = "—";
    private string _totalLines = "— linhas";
    private string _statusMessage = "Aguardando configuração";
    private bool _statusIsError;
    private bool _hasValidScript;
    private bool _isDirty;
    private bool _hasExportedFile;

    public PrincipalViewModel(
        IGeradorScript scriptGenerator,
        IServicoPerfil profileService,
        IServicoExportacaoScript scriptExportService,
        IServicoDialogos dialogService,
        IServicoAreaTransferencia clipboardService,
        ICaminhosAplicacao applicationPaths,
        IAgendadorAtualizacao previewScheduler)
    {
        _scriptGenerator = scriptGenerator;
        _profileService = profileService;
        _scriptExportService = scriptExportService;
        _dialogService = dialogService;
        _clipboardService = clipboardService;
        _applicationPaths = applicationPaths;
        _previewScheduler = previewScheduler;
        _lastFolder = applicationPaths.InitialDirectory;

        Configuration = new ConfiguracaoViewModel();
        Configuration.Changed += (_, _) => OnConfigurationChanged();

        GenerateCommand = new Comando(
            GenerateNow);
        CopyCommand = new Comando(CopyScript, () => HasValidScript);
        ExportCommand = new Comando(ExportScript, () => HasValidScript);
        SaveProfileCommand = new Comando(SaveProfile);
        SaveDefaultProfileCommand = new Comando(SaveDefaultProfile);
        LoadProfileCommand = new Comando(LoadProfile);
        NewScenarioCommand = new Comando(NewScenario);
        OpenFolderCommand = new Comando(OpenFolder, () => HasExportedFile);
        AboutCommand = new Comando(ShowAbout);

        RefreshPreview();
        IsDirty = false;
        TryLoadDefaultProfile();
    }

    public ConfiguracaoViewModel Configuration { get; }

    public Comando GenerateCommand { get; }
    public Comando CopyCommand { get; }
    public Comando ExportCommand { get; }
    public Comando SaveProfileCommand { get; }
    public Comando SaveDefaultProfileCommand { get; }
    public Comando LoadProfileCommand { get; }
    public Comando NewScenarioCommand { get; }
    public Comando OpenFolderCommand { get; }
    public Comando AboutCommand { get; }

    public string ApplicationVersion => _applicationVersion;

    public bool HasExportedFile
    {
        get => _hasExportedFile;
        private set
        {
            if (SetProperty(ref _hasExportedFile, value))
            {
                OpenFolderCommand.RaiseCanExecuteChanged();
            }
        }
    }

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

    public bool IsDirty
    {
        get => _isDirty;
        private set => SetProperty(ref _isDirty, value);
    }

    private void OnConfigurationChanged()
    {
        IsDirty = true;
        HasValidScript = false;
        SetStatus("Atualizando validação…", isError: false);
        _previewScheduler.Schedule(() => RefreshPreview());
    }

    private void GenerateNow()
    {
        _previewScheduler.Cancel();
        RefreshPreview("Script validado localmente e atualizado com sucesso.");
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
            ReportError("Não foi possível copiar", exception);
        }
    }

    private void ExportScript()
    {
        var fileName =
            $"script_pon{Configuration.PonStart}-{Configuration.PonEnd}"
            + $"_vlan{Configuration.Vlan}.txt";
        var path = _dialogService.SelectScriptToSave(fileName, _lastFolder);
        if (path is null)
        {
            return;
        }

        try
        {
            _scriptExportService.Export(path, ScriptPreview);
            UpdateLastFolder(path);
            HasExportedFile = true;
            SetStatus($"Arquivo {Path.GetFileName(path)} exportado.", isError: false);
        }
        catch (Exception exception) when (IsFileException(exception))
        {
            ReportError("Não foi possível exportar", exception);
        }
    }

    private void SaveProfile()
    {
        var path = _dialogService.SelectProfileToSave(_lastFolder);
        if (path is null)
        {
            return;
        }

        try
        {
            _profileService.Save(path, Configuration.ToProfile());
            UpdateLastFolder(path);
            IsDirty = false;
            SetStatus($"Perfil {Path.GetFileName(path)} exportado.", isError: false);
        }
        catch (Exception exception) when (IsFileException(exception))
        {
            ReportError("Não foi possível salvar o perfil", exception);
        }
    }

    private void SaveDefaultProfile()
    {
        var path = _applicationPaths.DefaultProfilePath;

        try
        {
            _profileService.Save(path, Configuration.ToProfile());
            IsDirty = false;
            SetStatus("Perfil padrão salvo. Ele será carregado na próxima abertura.", isError: false);
        }
        catch (Exception exception) when (IsFileException(exception))
        {
            ReportError("Não foi possível salvar o perfil padrão", exception);
        }
    }

    private void LoadProfile()
    {
        var path = _dialogService.SelectProfileToOpen(_lastFolder);
        if (path is null)
        {
            return;
        }

        try
        {
            Configuration.Apply(_profileService.Load(path));
            _previewScheduler.Cancel();
            RefreshPreview();
            UpdateLastFolder(path);
            CompleteProfileLoad(Path.GetFileName(path));
        }
        catch (Exception exception) when (IsFileException(exception))
        {
            ReportError("Não foi possível carregar o perfil", exception);
        }
    }

    private void TryLoadDefaultProfile()
    {
        var path = new[]
            {
                _applicationPaths.DefaultProfilePath,
                _applicationPaths.LegacyDefaultProfilePath
            }
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(File.Exists);
        if (path is null)
        {
            return;
        }

        try
        {
            Configuration.Apply(_profileService.Load(path));
            _previewScheduler.Cancel();
            RefreshPreview();
            UpdateLastFolder(path);
            CompleteProfileLoad(Path.GetFileName(path));
        }
        catch (Exception exception) when (IsFileException(exception))
        {
            SetStatus($"Perfil automático ignorado: {exception.Message}", isError: true);
        }
    }

    private void NewScenario()
    {
        if (!ConfirmDiscardChanges())
        {
            return;
        }

        Configuration.Reset();
        _previewScheduler.Cancel();
        RefreshPreview();
        IsDirty = false;
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
            ReportError("Não foi possível abrir a pasta", exception);
        }
    }

    private void ShowAbout() =>
        _dialogService.ShowInformation(
            "Sobre o Gerador OLT ZTE",
            $"Gerador de Scripts OLT ZTE • versão {GetApplicationVersion()}"
            + Environment.NewLine + Environment.NewLine
            + "Automação de provisionamento para OLTs ZTE Titan."
            + Environment.NewLine
            + "Arquitetura C# • WPF • MVVM"
            + Environment.NewLine + Environment.NewLine
            + "Rafael Caires & Lucas Alexandre");

    private static string GetApplicationVersion()
    {
        var informationalVersion = Assembly.GetEntryAssembly()?
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;
        return informationalVersion?.Split('+')[0] ?? "desconhecida";
    }

    private string FormatNumber(int number) => number.ToString("N0", _culture);

    public bool ConfirmClose() => ConfirmDiscardChanges();

    private bool ConfirmDiscardChanges() =>
        !IsDirty || _dialogService.Confirm(
            "Descartar alterações?",
            "As alterações do cenário atual ainda não foram salvas. Deseja descartá-las?");

    private void UpdateLastFolder(string path)
    {
        var directory = Path.GetDirectoryName(path);
        _lastFolder = !string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory)
            ? directory
            : _applicationPaths.InitialDirectory;
    }

    private void CompleteProfileLoad(string fileName)
    {
        IsDirty = false;
        if (HasValidScript)
        {
            SetStatus($"Perfil {fileName} carregado.", isError: false);
            return;
        }

        SetStatus(
            $"Perfil {fileName} carregado, mas contém erros: {StatusMessage}",
            isError: true);
    }

    private void SetStatus(string message, bool isError)
    {
        StatusMessage = message;
        StatusIsError = isError;
    }

    private void ReportError(string title, Exception exception)
    {
        SetStatus($"{title}: {exception.Message}", isError: true);
        _dialogService.ShowError(title, exception.Message);
    }

    private static bool IsFileException(Exception exception) =>
        exception is IOException or UnauthorizedAccessException or InvalidDataException;
}
