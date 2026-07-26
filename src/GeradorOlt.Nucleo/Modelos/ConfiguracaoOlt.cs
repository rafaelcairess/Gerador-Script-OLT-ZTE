namespace GeradorOlt.Nucleo.Modelos;

public sealed record ConfiguracaoOlt(
    int Slot,
    int Card,
    int PonStart,
    int PonEnd,
    int Vlan,
    int OnuStart,
    int OnuEnd,
    string TcontProfile,
    string Prefix,
    int? SecondaryVlan = null,
    string? Header = null,
    string? Footer = null,
    string GemName1 = "GEM1",
    string GemName2 = "GEM2");
