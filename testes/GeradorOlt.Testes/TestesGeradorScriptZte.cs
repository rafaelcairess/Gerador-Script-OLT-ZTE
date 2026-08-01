using GeradorOlt.Nucleo.Geracao;
using GeradorOlt.Nucleo.Modelos;
using GeradorOlt.Nucleo.Validacao;
using Xunit;

namespace GeradorOlt.Testes;

public sealed class TestesGeradorScriptZte
{
    private readonly GeradorScriptZte _generator = new();

    [Fact]
    public void Generate_CreatesEveryPonAndOnuInRange()
    {
        var configuration = CreateConfiguration(
            ponStart: 2, ponEnd: 3, onuStart: 4, onuEnd: 6);

        var result = _generator.Generate(configuration);

        Assert.Equal(6, result.Summary.TotalOnus);
        Assert.Equal(6, CountOccurrences(result.Content, "! Configuracao ONU"));
        Assert.Contains("interface gpon_onu-1/2/2:4", result.Content);
        Assert.Contains("interface gpon_onu-1/2/3:6", result.Content);
    }

    [Fact]
    public void Generate_AddsSecondaryServiceWhenConfigured()
    {
        var result = _generator.Generate(CreateConfiguration(secondaryVlan: 200));

        Assert.Equal(2, result.Summary.TotalServices);
        Assert.Contains("gemport 2 name GEM2 tcont 4", result.Content);
        Assert.Contains("service-port 2 user-vlan 200 vlan 200", result.Content);
        Assert.Contains("service 2 gemport 2 vlan 200", result.Content);
    }

    [Fact]
    public void Generate_NormalizesEmptyPrefixAndGemNames()
    {
        var configuration = CreateConfiguration(
            prefix: "", gemName1: "", gemName2: "");

        var result = _generator.Generate(configuration);

        Assert.Contains("name automatico_1_1", result.Content);
        Assert.Contains("gemport 1 name GEM1 tcont 4", result.Content);
    }

    [Fact]
    public void Generate_PreservesHeaderAndFooterComments()
    {
        var configuration = CreateConfiguration(
            header: "Janela de migração", footer: "! Validar clientes");

        var result = _generator.Generate(configuration);

        Assert.StartsWith("! Janela de migração", result.Content);
        Assert.EndsWith("! Validar clientes", result.Content);
    }

    [Fact]
    public void Generate_RejectsInvertedPonRange()
    {
        var configuration = CreateConfiguration(ponStart: 4, ponEnd: 2);

        var exception = Assert.Throws<ExcecaoValidacaoConfiguracao>(
            () => _generator.Generate(configuration));

        Assert.Contains("PON inicial", exception.Message);
    }

    [Fact]
    public void Generate_RejectsDuplicateVlans()
    {
        var configuration = CreateConfiguration(vlan: 100, secondaryVlan: 100);

        var exception = Assert.Throws<ExcecaoValidacaoConfiguracao>(
            () => _generator.Generate(configuration));

        Assert.Contains("diferente", exception.Message);
    }

    [Fact]
    public void Generate_AcceptsHighestUsableVlan()
    {
        var result = _generator.Generate(CreateConfiguration(vlan: 4094));

        Assert.Contains("service-port 1 user-vlan 4094 vlan 4094", result.Content);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(4095)]
    [InlineData(4096)]
    public void Generate_RejectsReservedOrOutOfRangePrimaryVlan(int vlan)
    {
        var exception = Assert.Throws<ExcecaoValidacaoConfiguracao>(
            () => _generator.Generate(CreateConfiguration(vlan: vlan)));

        Assert.Contains("VLAN principal", exception.Message);
        Assert.Contains("1 e 4094", exception.Message);
    }

    [Theory]
    [InlineData(4095)]
    [InlineData(4096)]
    public void Generate_RejectsReservedOrOutOfRangeSecondaryVlan(int secondaryVlan)
    {
        var exception = Assert.Throws<ExcecaoValidacaoConfiguracao>(
            () => _generator.Generate(CreateConfiguration(secondaryVlan: secondaryVlan)));

        Assert.Contains("VLAN do serviço adicional", exception.Message);
        Assert.Contains("1 e 4094", exception.Message);
    }

    [Fact]
    public void Generate_MatchesExpectedCompleteScript()
    {
        var expectedPath = Path.Combine(
            AppContext.BaseDirectory,
            "ArquivosEsperados",
            "script-basico.txt");
        var expected = File.ReadAllText(expectedPath).ReplaceLineEndings("\n");

        var result = _generator.Generate(CreateConfiguration());
        var actual = result.Content.ReplaceLineEndings("\n");

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Generate_SupportsMaximumPonAndOnuBatch()
    {
        var configuration = CreateConfiguration(
            ponStart: 1,
            ponEnd: ValidadorConfiguracao.MaxPon,
            onuStart: 1,
            onuEnd: ValidadorConfiguracao.MaxOnu);

        var result = _generator.Generate(configuration);

        Assert.Equal(16, result.Summary.TotalPons);
        Assert.Equal(2048, result.Summary.TotalOnus);
        Assert.Equal(2048, result.Summary.TotalServices);
        Assert.Equal(2048, CountOccurrences(result.Content, "! Configuracao ONU"));
        Assert.Contains("interface gpon_onu-1/2/16:128", result.Content);
    }

    [Fact]
    public void Generate_RejectsCommandInjectionInIdentifier()
    {
        var configuration = CreateConfiguration(gemName1: "GEM1 exit");

        var exception = Assert.Throws<ExcecaoValidacaoConfiguracao>(
            () => _generator.Generate(configuration));

        Assert.Contains("aceita apenas", exception.Message);
    }

    private static ConfiguracaoOlt CreateConfiguration(
        int ponStart = 1,
        int ponEnd = 1,
        int vlan = 100,
        int onuStart = 1,
        int onuEnd = 1,
        string prefix = "cliente",
        int? secondaryVlan = null,
        string? header = null,
        string? footer = null,
        string gemName1 = "GEM1",
        string gemName2 = "GEM2") =>
        new(
            Slot: 1,
            Card: 2,
            PonStart: ponStart,
            PonEnd: ponEnd,
            Vlan: vlan,
            OnuStart: onuStart,
            OnuEnd: onuEnd,
            TcontProfile: "1G",
            Prefix: prefix,
            SecondaryVlan: secondaryVlan,
            Header: header,
            Footer: footer,
            GemName1: gemName1,
            GemName2: gemName2);

    private static int CountOccurrences(string source, string value) =>
        source.Split(value, StringSplitOptions.None).Length - 1;
}
