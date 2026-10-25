using System.Text;
using GeradorOlt.Infraestrutura.Arquivos;
using GeradorOlt.Nucleo.Exportacao;

namespace GeradorOlt.Infraestrutura.Exportacao;

public sealed class ServicoExportacaoTexto : IServicoExportacaoScript
{
    private static readonly Encoding Utf8WithoutBom = new UTF8Encoding(false);

    public void Export(string path, string content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(content);
        EscritorArquivoAtomico.WriteAllText(path, content, Utf8WithoutBom);
    }
}
