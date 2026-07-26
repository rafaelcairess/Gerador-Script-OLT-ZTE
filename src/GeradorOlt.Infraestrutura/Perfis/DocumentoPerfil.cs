using System.Text.Json.Serialization;
using GeradorOlt.Nucleo.Modelos;

namespace GeradorOlt.Infraestrutura.Perfis;

internal sealed class DocumentoPerfil
{
    [JsonPropertyName("slot")]
    public string Slot { get; init; } = "1";

    [JsonPropertyName("card")]
    public string Card { get; init; } = "3";

    [JsonPropertyName("pon_ini")]
    public string PonStart { get; init; } = "1";

    [JsonPropertyName("pon_fim")]
    public string PonEnd { get; init; } = "1";

    [JsonPropertyName("vlan")]
    public string Vlan { get; init; } = "100";

    [JsonPropertyName("onu_ini")]
    public string OnuStart { get; init; } = "1";

    [JsonPropertyName("onu_fim")]
    public string OnuEnd { get; init; } = "128";

    [JsonPropertyName("perfil")]
    public string TcontProfile { get; init; } = "1G";

    [JsonPropertyName("prefixo")]
    public string Prefix { get; init; } = "cliente";

    [JsonPropertyName("nome_gem1")]
    public string GemName1 { get; init; } = "GEM1";

    [JsonPropertyName("nome_gem2")]
    public string GemName2 { get; init; } = "GEM2";

    [JsonPropertyName("cabecalho")]
    public string Header { get; init; } = string.Empty;

    [JsonPropertyName("rodape")]
    public string Footer { get; init; } = string.Empty;

    [JsonPropertyName("servico2")]
    public bool SecondaryService { get; init; }

    [JsonPropertyName("vlan2")]
    public string SecondaryVlan { get; init; } = string.Empty;

    public DadosPerfil ToModel() =>
        new()
        {
            Slot = Slot,
            Card = Card,
            PonStart = PonStart,
            PonEnd = PonEnd,
            Vlan = Vlan,
            OnuStart = OnuStart,
            OnuEnd = OnuEnd,
            TcontProfile = TcontProfile,
            Prefix = Prefix,
            GemName1 = GemName1,
            GemName2 = GemName2,
            Header = Header,
            Footer = Footer,
            SecondaryService = SecondaryService,
            SecondaryVlan = SecondaryVlan
        };

    public static DocumentoPerfil FromModel(DadosPerfil model) =>
        new()
        {
            Slot = model.Slot,
            Card = model.Card,
            PonStart = model.PonStart,
            PonEnd = model.PonEnd,
            Vlan = model.Vlan,
            OnuStart = model.OnuStart,
            OnuEnd = model.OnuEnd,
            TcontProfile = model.TcontProfile,
            Prefix = model.Prefix,
            GemName1 = model.GemName1,
            GemName2 = model.GemName2,
            Header = model.Header,
            Footer = model.Footer,
            SecondaryService = model.SecondaryService,
            SecondaryVlan = model.SecondaryVlan
        };
}
