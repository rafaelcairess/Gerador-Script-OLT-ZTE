using System.Windows;
using GeradorOlt.Apresentacao.ModelosVisao;
using GeradorOlt.Apresentacao.Servicos;
using GeradorOlt.Infraestrutura.Exportacao;
using GeradorOlt.Infraestrutura.Perfis;
using GeradorOlt.Nucleo.Geracao;
using GeradorOlt.Nucleo.Modelos;

namespace GeradorOlt.Apresentacao;

public partial class App : Application
{
    private void OnStartup(object sender, StartupEventArgs eventArgs)
    {
        if (eventArgs.Args.Contains("--smoke-test", StringComparer.OrdinalIgnoreCase))
        {
            Shutdown(RunSmokeTest());
            return;
        }

        var dialogService = new ServicoDialogosDesktop();
        var viewModel = new PrincipalViewModel(
            new GeradorScriptZte(),
            new ServicoPerfilJson(),
            new ServicoExportacaoTexto(),
            dialogService,
            new ServicoAreaTransferencia(),
            new CaminhosAplicacao(),
            new AgendadorAtualizacaoDispatcher(Dispatcher));

        var window = new JanelaPrincipal
        {
            DataContext = viewModel
        };

        MainWindow = window;
        window.Show();
    }

    private static int RunSmokeTest()
    {
        try
        {
            var configuration = new ConfiguracaoOlt(
                Slot: 1,
                Card: 1,
                PonStart: 1,
                PonEnd: 1,
                Vlan: 100,
                OnuStart: 1,
                OnuEnd: 1,
                TcontProfile: "1G",
                Prefix: "smoke_test");
            var generated = new GeradorScriptZte().Generate(configuration);
            return generated.Summary.TotalOnus == 1 ? 0 : 1;
        }
        catch (Exception)
        {
            return 1;
        }
    }
}
