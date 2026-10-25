using System.ComponentModel;
using System.Windows;
using GeradorOlt.Apresentacao.ModelosVisao;

namespace GeradorOlt.Apresentacao;

public partial class JanelaPrincipal : Window
{
    public JanelaPrincipal()
    {
        InitializeComponent();
        Closing += OnClosing;
    }

    private void OnClosing(object? sender, CancelEventArgs eventArgs)
    {
        if (DataContext is PrincipalViewModel viewModel && !viewModel.ConfirmClose())
        {
            eventArgs.Cancel = true;
        }
    }
}
