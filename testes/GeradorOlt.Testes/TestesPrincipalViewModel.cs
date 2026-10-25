using GeradorOlt.Apresentacao.ModelosVisao;
using GeradorOlt.Apresentacao.Servicos;
using GeradorOlt.Nucleo.Exportacao;
using GeradorOlt.Nucleo.Geracao;
using GeradorOlt.Nucleo.Modelos;
using GeradorOlt.Nucleo.Perfis;
using Xunit;

namespace GeradorOlt.Testes;

public sealed class TestesPrincipalViewModel
{
    [Fact]
    public void Constructor_GeneratesInitialPreviewAndMetrics()
    {
        var viewModel = CreateViewModel();

        Assert.True(viewModel.HasValidScript);
        Assert.False(viewModel.IsDirty);
        Assert.Equal("1", viewModel.TotalPons);
        Assert.Equal("128", viewModel.TotalOnus);
        Assert.Equal("128", viewModel.TotalServices);
        Assert.Contains("interface gpon_onu-1/3/1:1", viewModel.ScriptPreview);
    }

    [Fact]
    public void InvalidInput_DisablesExportAndShowsContextualError()
    {
        var viewModel = CreateViewModel();

        viewModel.Configuration.Slot = "invalido";

        Assert.False(viewModel.HasValidScript);
        Assert.True(viewModel.StatusIsError);
        Assert.True(viewModel.IsDirty);
        Assert.Contains("número inteiro", viewModel.StatusMessage);
        Assert.False(viewModel.ExportCommand.CanExecute(null));
    }

    [Fact]
    public void SecondaryService_UpdatesPreviewAndMetrics()
    {
        var viewModel = CreateViewModel();

        viewModel.Configuration.IsSecondaryServiceEnabled = true;
        viewModel.Configuration.SecondaryVlan = "200";

        Assert.True(viewModel.HasValidScript);
        Assert.Equal("256", viewModel.TotalServices);
        Assert.Contains("service 2 gemport 2 vlan 200", viewModel.ScriptPreview);
    }

    [Fact]
    public void LoadProfile_KeepsErrorStatusWhenLoadedProfileIsInvalid()
    {
        var profileService = new FakeProfileService
        {
            ProfileToLoad = new DadosPerfil { Slot = "inválido" }
        };
        var dialogs = new FakeDialogService { ProfileToOpen = "perfil-invalido.json" };
        var viewModel = CreateViewModel(profileService, dialogs);

        viewModel.LoadProfileCommand.Execute(null);

        Assert.False(viewModel.HasValidScript);
        Assert.True(viewModel.StatusIsError);
        Assert.False(viewModel.IsDirty);
        Assert.Contains("carregado, mas contém erros", viewModel.StatusMessage);
        Assert.Contains("número inteiro", viewModel.StatusMessage);
    }

    [Fact]
    public void LoadProfile_ReportsSuccessForValidProfile()
    {
        var profileService = new FakeProfileService
        {
            ProfileToLoad = new DadosPerfil { Vlan = "500" }
        };
        var dialogs = new FakeDialogService { ProfileToOpen = "perfil-valido.json" };
        var viewModel = CreateViewModel(profileService, dialogs);

        viewModel.LoadProfileCommand.Execute(null);

        Assert.True(viewModel.HasValidScript);
        Assert.False(viewModel.StatusIsError);
        Assert.False(viewModel.IsDirty);
        Assert.Contains("perfil-valido.json carregado", viewModel.StatusMessage);
    }

    [Fact]
    public void SaveProfile_ClearsDirtyStateAndUsesLastDirectory()
    {
        var profileService = new FakeProfileService();
        var dialogs = new FakeDialogService { ProfileToSave = "C:\\perfis\\cliente.json" };
        var viewModel = CreateViewModel(profileService, dialogs);
        viewModel.Configuration.Vlan = "300";

        viewModel.SaveProfileCommand.Execute(null);

        Assert.False(viewModel.IsDirty);
        Assert.Equal("300", profileService.SavedProfile?.Vlan);
        Assert.Equal("C:\\dados", dialogs.LastInitialDirectory);
    }

    [Fact]
    public void SaveDefaultProfile_UsesAutomaticProfilePathWithoutDialog()
    {
        var profileService = new FakeProfileService();
        var dialogs = new FakeDialogService();
        var viewModel = CreateViewModel(profileService, dialogs);
        viewModel.Configuration.Vlan = "300";

        viewModel.SaveDefaultProfileCommand.Execute(null);

        Assert.False(viewModel.IsDirty);
        Assert.Equal("300", profileService.SavedProfile?.Vlan);
        Assert.Equal("C:\\dados\\perfil_config.json", profileService.SavedPath);
        Assert.Null(dialogs.LastInitialDirectory);
        Assert.Contains("próxima abertura", viewModel.StatusMessage);
    }

    [Fact]
    public void NewScenario_DoesNotDiscardUnsavedChangesWithoutConfirmation()
    {
        var dialogs = new FakeDialogService { ConfirmationResult = false };
        var viewModel = CreateViewModel(dialogService: dialogs);
        viewModel.Configuration.Vlan = "300";

        viewModel.NewScenarioCommand.Execute(null);

        Assert.Equal("300", viewModel.Configuration.Vlan);
        Assert.True(viewModel.IsDirty);
        Assert.Equal(1, dialogs.ConfirmationCount);
    }

    [Fact]
    public void NewScenario_ResetsConfigurationAfterConfirmation()
    {
        var dialogs = new FakeDialogService { ConfirmationResult = true };
        var viewModel = CreateViewModel(dialogService: dialogs);
        viewModel.Configuration.Vlan = "300";

        viewModel.NewScenarioCommand.Execute(null);

        Assert.Equal("100", viewModel.Configuration.Vlan);
        Assert.False(viewModel.IsDirty);
        Assert.True(viewModel.HasValidScript);
    }

    [Fact]
    public void CopyAndExport_UseCurrentValidScript()
    {
        var dialogs = new FakeDialogService { ScriptToSave = "C:\\scripts\\olt.txt" };
        var clipboard = new FakeServicoAreaTransferencia();
        var exporter = new FakeScriptExportService();
        var viewModel = CreateViewModel(
            dialogService: dialogs,
            clipboardService: clipboard,
            exportService: exporter);

        viewModel.CopyCommand.Execute(null);
        viewModel.ExportCommand.Execute(null);

        Assert.Equal(viewModel.ScriptPreview, clipboard.Text);
        Assert.Equal("C:\\scripts\\olt.txt", exporter.Path);
        Assert.Equal(viewModel.ScriptPreview, exporter.Content);
        Assert.Contains("exportado", viewModel.StatusMessage);
        Assert.True(viewModel.OpenFolderCommand.CanExecute(null));
    }

    [Fact]
    public void PendingPreview_DisablesCopyAndExportUntilRefreshCompletes()
    {
        var scheduler = new ManualUpdateScheduler();
        var viewModel = CreateViewModel(updateScheduler: scheduler);

        viewModel.Configuration.Vlan = "300";

        Assert.False(viewModel.HasValidScript);
        Assert.False(viewModel.CopyCommand.CanExecute(null));
        Assert.Contains("Atualizando", viewModel.StatusMessage);

        scheduler.RunPending();

        Assert.True(viewModel.HasValidScript);
        Assert.Contains("vlan 300", viewModel.ScriptPreview);
    }

    [Fact]
    public void ConfirmClose_ProtectsUnsavedChanges()
    {
        var dialogs = new FakeDialogService { ConfirmationResult = false };
        var viewModel = CreateViewModel(dialogService: dialogs);
        viewModel.Configuration.Vlan = "300";

        var canClose = viewModel.ConfirmClose();

        Assert.False(canClose);
        Assert.Equal(1, dialogs.ConfirmationCount);
    }

    [Fact]
    public void CancelledFileDialogs_DoNotChangeCurrentState()
    {
        var dialogs = new FakeDialogService();
        var viewModel = CreateViewModel(dialogService: dialogs);
        var initialStatus = viewModel.StatusMessage;

        viewModel.ExportCommand.Execute(null);
        viewModel.SaveProfileCommand.Execute(null);
        viewModel.LoadProfileCommand.Execute(null);

        Assert.Equal(initialStatus, viewModel.StatusMessage);
        Assert.True(viewModel.ConfirmClose());
        Assert.Equal(0, dialogs.ConfirmationCount);
    }

    private static PrincipalViewModel CreateViewModel(
        FakeProfileService? profileService = null,
        FakeDialogService? dialogService = null,
        FakeServicoAreaTransferencia? clipboardService = null,
        FakeScriptExportService? exportService = null,
        IAgendadorAtualizacao? updateScheduler = null) =>
        new(
            new GeradorScriptZte(),
            profileService ?? new FakeProfileService(),
            exportService ?? new FakeScriptExportService(),
            dialogService ?? new FakeDialogService(),
            clipboardService ?? new FakeServicoAreaTransferencia(),
            new FakeApplicationPaths(),
            updateScheduler ?? new ImmediateUpdateScheduler());

    private sealed class FakeProfileService : IServicoPerfil
    {
        public DadosPerfil ProfileToLoad { get; init; } = new();
        public DadosPerfil? SavedProfile { get; private set; }
        public string? SavedPath { get; private set; }

        public void Save(string path, DadosPerfil profile)
        {
            SavedPath = path;
            SavedProfile = profile;
        }
        public DadosPerfil Load(string path) => ProfileToLoad;
    }

    private sealed class FakeScriptExportService : IServicoExportacaoScript
    {
        public string? Path { get; private set; }
        public string? Content { get; private set; }

        public void Export(string path, string content)
        {
            Path = path;
            Content = content;
        }
    }

    private sealed class FakeServicoAreaTransferencia : IServicoAreaTransferencia
    {
        public string? Text { get; private set; }
        public void SetText(string text) => Text = text;
    }

    private sealed class FakeDialogService : IServicoDialogos
    {
        public string? ProfileToOpen { get; init; }
        public string? ProfileToSave { get; init; }
        public string? ScriptToSave { get; init; }
        public bool ConfirmationResult { get; init; } = true;
        public int ConfirmationCount { get; private set; }
        public string? LastInitialDirectory { get; private set; }

        public string? SelectProfileToOpen(string initialDirectory)
        {
            LastInitialDirectory = initialDirectory;
            return ProfileToOpen;
        }

        public string? SelectProfileToSave(string initialDirectory)
        {
            LastInitialDirectory = initialDirectory;
            return ProfileToSave;
        }

        public string? SelectScriptToSave(string suggestedName, string initialDirectory)
        {
            LastInitialDirectory = initialDirectory;
            return ScriptToSave;
        }

        public bool Confirm(string title, string message)
        {
            ConfirmationCount++;
            return ConfirmationResult;
        }

        public void ShowError(string title, string message)
        {
        }

        public void ShowInformation(string title, string message)
        {
        }

        public void OpenFolder(string path)
        {
        }
    }

    private sealed class FakeApplicationPaths : ICaminhosAplicacao
    {
        public string DefaultProfilePath => "C:\\dados\\perfil_config.json";
        public string LegacyDefaultProfilePath => "C:\\app\\perfil_config.json";
        public string InitialDirectory => "C:\\dados";
    }

    private sealed class ImmediateUpdateScheduler : IAgendadorAtualizacao
    {
        public void Schedule(Action action) => action();
        public void Cancel()
        {
        }
    }

    private sealed class ManualUpdateScheduler : IAgendadorAtualizacao
    {
        private Action? _pendingAction;

        public void Schedule(Action action) => _pendingAction = action;
        public void Cancel() => _pendingAction = null;

        public void RunPending()
        {
            var action = _pendingAction;
            _pendingAction = null;
            action?.Invoke();
        }
    }
}
