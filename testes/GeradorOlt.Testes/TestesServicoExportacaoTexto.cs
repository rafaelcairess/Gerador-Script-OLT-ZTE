using System.Text;
using GeradorOlt.Infraestrutura.Exportacao;
using Xunit;

namespace GeradorOlt.Testes;

public sealed class TestesServicoExportacaoTexto : IDisposable
{
    private readonly string _directory = Path.Combine(
        Path.GetTempPath(), $"gerador-olt-export-tests-{Guid.NewGuid():N}");

    public TestesServicoExportacaoTexto() => Directory.CreateDirectory(_directory);

    [Fact]
    public void Export_WritesUtf8WithoutByteOrderMark()
    {
        var path = Path.Combine(_directory, "script.txt");
        const string content = "! Configuração validada";

        new ServicoExportacaoTexto().Export(path, content);

        var bytes = File.ReadAllBytes(path);
        Assert.False(bytes.AsSpan().StartsWith(Encoding.UTF8.Preamble));
        Assert.Equal(content, Encoding.UTF8.GetString(bytes));
    }

    [Fact]
    public void Export_AtomicallyOverwritesExistingScript()
    {
        var path = Path.Combine(_directory, "script.txt");
        File.WriteAllText(path, "conteúdo anterior");

        new ServicoExportacaoTexto().Export(path, "conteúdo novo");

        Assert.Equal("conteúdo novo", File.ReadAllText(path));
        Assert.Empty(Directory.GetFiles(_directory, "*.tmp"));
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }
}
