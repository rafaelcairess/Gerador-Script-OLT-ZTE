using GeradorOlt.Infraestrutura.Perfis;
using GeradorOlt.Nucleo.Modelos;
using Xunit;

namespace GeradorOlt.Testes;

public sealed class TestesServicoPerfilJson : IDisposable
{
    private readonly string _directory = Path.Combine(
        Path.GetTempPath(), $"gerador-olt-tests-{Guid.NewGuid():N}");

    public TestesServicoPerfilJson() => Directory.CreateDirectory(_directory);

    [Fact]
    public void SaveAndLoad_PreservesProfile()
    {
        var path = Path.Combine(_directory, "perfil.json");
        var service = new ServicoPerfilJson();
        var profile = new DadosPerfil
        {
            Slot = "2",
            Vlan = "300",
            SecondaryService = true,
            SecondaryVlan = "400"
        };

        service.Save(path, profile);
        var loaded = service.Load(path);

        Assert.Equal("2", loaded.Slot);
        Assert.Equal("300", loaded.Vlan);
        Assert.True(loaded.SecondaryService);
        Assert.Equal("400", loaded.SecondaryVlan);
        Assert.Contains("\"versao\": 2", File.ReadAllText(path));
    }

    [Fact]
    public void Load_ReadsLegacyPropertyNames()
    {
        var path = Path.Combine(_directory, "perfil-legado.json");
        File.WriteAllText(
            path,
            """
            {
              "slot": "1",
              "pon_ini": "3",
              "pon_fim": "5",
              "servico2": true,
              "vlan2": "200"
            }
            """);

        var loaded = new ServicoPerfilJson().Load(path);

        Assert.Equal("3", loaded.PonStart);
        Assert.Equal("5", loaded.PonEnd);
        Assert.True(loaded.SecondaryService);
        Assert.Equal("200", loaded.SecondaryVlan);
    }

    [Fact]
    public void Load_RejectsCorruptedJsonWithDomainFriendlyError()
    {
        var path = Path.Combine(_directory, "perfil-corrompido.json");
        File.WriteAllText(path, "{ perfil inválido");

        var exception = Assert.Throws<InvalidDataException>(
            () => new ServicoPerfilJson().Load(path));

        Assert.Contains("corrompido", exception.Message);
    }

    [Fact]
    public void Load_RejectsProfileFromUnsupportedFutureVersion()
    {
        var path = Path.Combine(_directory, "perfil-futuro.json");
        File.WriteAllText(path, """
            {
              "versao": 999,
              "slot": "1"
            }
            """);

        var exception = Assert.Throws<InvalidDataException>(
            () => new ServicoPerfilJson().Load(path));

        Assert.Contains("não suportada", exception.Message);
    }

    [Fact]
    public void Save_AtomicallyOverwritesExistingProfile()
    {
        var path = Path.Combine(_directory, "perfil.json");
        File.WriteAllText(path, "conteúdo anterior");

        new ServicoPerfilJson().Save(path, new DadosPerfil { Vlan = "777" });

        Assert.Equal("777", new ServicoPerfilJson().Load(path).Vlan);
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
