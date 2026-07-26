using System.Text.RegularExpressions;
using GeradorOlt.Nucleo.Modelos;

namespace GeradorOlt.Nucleo.Validacao;

public static partial class ValidadorConfiguracao
{
    public const int MaxSlot = 99;
    public const int MaxCard = 99;
    public const int MaxPon = 16;
    public const int MaxOnu = 128;
    public const int MaxVlan = 4096;

    public static readonly IReadOnlyList<string> TcontProfiles =
        ["100M", "200M", "500M", "1G", "2G", "1GPON"];

    public static void Validate(ConfiguracaoOlt configuration)
    {
        ValidateRange(configuration.Slot, "Slot", 1, MaxSlot);
        ValidateRange(configuration.Card, "Card", 1, MaxCard);
        ValidateRange(configuration.PonStart, "PON inicial", 1, MaxPon);
        ValidateRange(configuration.PonEnd, "PON final", 1, MaxPon);
        ValidateRange(configuration.Vlan, "VLAN principal", 1, MaxVlan);
        ValidateRange(configuration.OnuStart, "ONU inicial", 1, MaxOnu);
        ValidateRange(configuration.OnuEnd, "ONU final", 1, MaxOnu);

        if (configuration.PonStart > configuration.PonEnd)
        {
            throw new ExcecaoValidacaoConfiguracao(
                "PON inicial não pode ser maior que PON final.");
        }

        if (configuration.OnuStart > configuration.OnuEnd)
        {
            throw new ExcecaoValidacaoConfiguracao(
                "ONU inicial não pode ser maior que ONU final.");
        }

        if (!TcontProfiles.Contains(configuration.TcontProfile))
        {
            throw new ExcecaoValidacaoConfiguracao(
                $"Perfil TCONT inválido: {configuration.TcontProfile}.");
        }

        ValidateIdentifier(NormalizePrefix(configuration.Prefix), "Prefixo");
        ValidateIdentifier(NormalizeIdentifier(configuration.GemName1, "GEM1"), "Nome GEM 1");
        ValidateIdentifier(NormalizeIdentifier(configuration.GemName2, "GEM2"), "Nome GEM 2");

        if (configuration.SecondaryVlan is { } secondaryVlan)
        {
            ValidateRange(secondaryVlan, "VLAN do serviço adicional", 1, MaxVlan);
            if (secondaryVlan == configuration.Vlan)
            {
                throw new ExcecaoValidacaoConfiguracao(
                    "VLAN do serviço adicional deve ser diferente da principal.");
            }
        }
    }

    public static string NormalizePrefix(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "automatico" : value.Trim();

    public static string NormalizeIdentifier(string? value, string fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();

    private static void ValidateRange(int value, string field, int minimum, int maximum)
    {
        if (value < minimum || value > maximum)
        {
            throw new ExcecaoValidacaoConfiguracao(
                $"'{field}' deve estar entre {minimum} e {maximum}.");
        }
    }

    private static void ValidateIdentifier(string value, string field)
    {
        if (value.Length > 32)
        {
            throw new ExcecaoValidacaoConfiguracao(
                $"'{field}' deve ter no máximo 32 caracteres.");
        }

        if (!IdentifierPattern().IsMatch(value))
        {
            throw new ExcecaoValidacaoConfiguracao(
                $"'{field}' aceita apenas letras, números, ponto, underline ou hífen.");
        }
    }

    [GeneratedRegex(@"^[a-zA-Z0-9_.\-]+$")]
    private static partial Regex IdentifierPattern();
}
