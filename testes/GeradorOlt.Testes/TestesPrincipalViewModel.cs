using GeradorOlt.Apresentacao.ModelosVisao;
using GeradorOlt.Apresentacao.Servicos;
using GeradorOlt.Infraestrutura.Perfis;
using GeradorOlt.Nucleo.Exportacao;
using GeradorOlt.Nucleo.Geracao;
using Xunit;

namespace GeradorOlt.Testes;

public sealed class TestesPrincipalViewModel
{
    [Fact]
    public void Constructor_GeneratesInitialPreviewAndMetrics()
    {
        var viewModel = CreateViewModel();

        Assert.True(viewModel.HasValidScript);
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

    private static PrincipalViewModel CreateViewModel() =>
        new(
            new GeradorScriptZte(),
            new ServicoPerfilJson(),
            new FakeScriptExportService(),
            new FakeDialogService(),
            new FakeServicoAreaTransferencia());

    private sealed class FakeScriptExportService : IServicoExportacaoScript
    {
        public void Export(string path, string content)
        {
        }
    }

    private sealed class FakeServicoAreaTransferencia : IServicoAreaTransferencia
    {
        public void SetText(string text)
        {
        }
    }

    private sealed class FakeDialogService : IServicoDialogos
    {
        public string? SelectProfileToOpen() => null;
        public string? SelectProfileToSave() => null;
        public string? SelectScriptToSave(string suggestedName) => null;
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
}
