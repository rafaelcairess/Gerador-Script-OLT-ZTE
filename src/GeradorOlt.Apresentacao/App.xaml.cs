using System.Windows;
using GeradorOlt.Apresentacao.ModelosVisao;
using GeradorOlt.Apresentacao.Servicos;
using GeradorOlt.Infraestrutura.Exportacao;
using GeradorOlt.Infraestrutura.Perfis;
using GeradorOlt.Nucleo.Geracao;

namespace GeradorOlt.Apresentacao;

public partial class App : Application
{
    private void OnStartup(object sender, StartupEventArgs _)
    {
        var dialogService = new ServicoDialogosDesktop();
        var viewModel = new PrincipalViewModel(
            new GeradorScriptZte(),
            new ServicoPerfilJson(),
            new ServicoExportacaoTexto(),
            dialogService,
            new ServicoAreaTransferencia());

        var window = new JanelaPrincipal
        {
            DataContext = viewModel
        };

        MainWindow = window;
        window.Show();
    }
}
