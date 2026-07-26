using System.Windows;

namespace GeradorOlt.Apresentacao.Servicos;

public sealed class ServicoAreaTransferencia : IServicoAreaTransferencia
{
    public void SetText(string text) => Clipboard.SetText(text);
}
