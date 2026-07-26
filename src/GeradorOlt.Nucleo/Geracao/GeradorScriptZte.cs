using System.Text;
using GeradorOlt.Nucleo.Modelos;
using GeradorOlt.Nucleo.Validacao;

namespace GeradorOlt.Nucleo.Geracao;

public sealed class GeradorScriptZte : IGeradorScript
{
    private const string Separator = "! ########################################################";

    public ScriptGerado Generate(ConfiguracaoOlt configuration)
    {
        ValidadorConfiguracao.Validate(configuration);

        var prefix = ValidadorConfiguracao.NormalizePrefix(configuration.Prefix);
        var gemName1 = ValidadorConfiguracao.NormalizeIdentifier(
            configuration.GemName1, "GEM1");
        var gemName2 = ValidadorConfiguracao.NormalizeIdentifier(
            configuration.GemName2, "GEM2");
        var lines = new List<string>();

        AppendComment(lines, configuration.Header);
        if (!string.IsNullOrWhiteSpace(configuration.Header))
        {
            lines.Add(string.Empty);
        }

        for (var pon = configuration.PonStart; pon <= configuration.PonEnd; pon++)
        {
            lines.Add(Separator);
            lines.Add($"! ### INICIO CONFIGURACAO PON {pon} / VLAN {configuration.Vlan} ###");
            lines.Add(Separator);
            lines.Add(string.Empty);

            for (var onu = configuration.OnuStart; onu <= configuration.OnuEnd; onu++)
            {
                AppendOnu(lines, configuration, prefix, gemName1, gemName2, pon, onu);
            }

            lines.Add(Separator);
            lines.Add($"! ### FIM CONFIGURACAO PON {pon} / VLAN {configuration.Vlan} ###");
            lines.Add(Separator);
            lines.Add(string.Empty);
        }

        AppendComment(lines, configuration.Footer);

        var content = string.Join(Environment.NewLine, lines);
        var totalPons = configuration.PonEnd - configuration.PonStart + 1;
        var totalOnus = totalPons * (configuration.OnuEnd - configuration.OnuStart + 1);
        var servicesPerOnu = configuration.SecondaryVlan.HasValue ? 2 : 1;
        var summary = new ResumoScript(
            totalPons,
            totalOnus,
            totalOnus * servicesPerOnu,
            lines.Count);

        return new ScriptGerado(content, summary);
    }

    private static void AppendOnu(
        List<string> lines,
        ConfiguracaoOlt configuration,
        string prefix,
        string gemName1,
        string gemName2,
        int pon,
        int onu)
    {
        lines.Add($"! Configuracao ONU {onu}");
        lines.Add($"interface gpon_onu-{configuration.Slot}/{configuration.Card}/{pon}:{onu}");
        lines.Add($"name {prefix}_{pon}_{onu}");
        lines.Add($"description {prefix}_{pon}_{onu}");
        lines.Add($"tcont 4 profile {configuration.TcontProfile}");
        lines.Add($"gemport 1 name {gemName1} tcont 4");
        if (configuration.SecondaryVlan.HasValue)
        {
            lines.Add($"gemport 2 name {gemName2} tcont 4");
        }

        lines.Add("exit");
        lines.Add($"interface vport-{configuration.Slot}/{configuration.Card}/{pon}.{onu}:1");
        lines.Add($"service-port 1 user-vlan {configuration.Vlan} vlan {configuration.Vlan}");
        if (configuration.SecondaryVlan is { } secondaryVlan)
        {
            lines.Add($"service-port 2 user-vlan {secondaryVlan} vlan {secondaryVlan}");
        }

        lines.Add("exit");
        lines.Add($"pon-onu-mng gpon_onu-{configuration.Slot}/{configuration.Card}/{pon}:{onu}");
        lines.Add($"service 1 gemport 1 vlan {configuration.Vlan}");
        lines.Add($"vlan port eth_0/1 mode tag vlan {configuration.Vlan}");
        if (configuration.SecondaryVlan is { } serviceVlan)
        {
            lines.Add($"service 2 gemport 2 vlan {serviceVlan}");
            lines.Add($"vlan port eth_0/2 mode tag vlan {serviceVlan}");
        }

        lines.Add("exit");
        lines.Add(string.Empty);
    }

    private static void AppendComment(List<string> lines, string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        foreach (var rawLine in text.ReplaceLineEndings("\n").Split('\n'))
        {
            var line = rawLine.Trim();
            lines.Add(line.StartsWith('!') ? line : $"! {line}");
        }
    }
}
