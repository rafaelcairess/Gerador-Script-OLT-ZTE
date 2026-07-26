namespace GeradorOlt.Nucleo.Modelos;

public sealed class DadosPerfil
{
    public string Slot { get; set; } = "1";

    public string Card { get; set; } = "3";

    public string PonStart { get; set; } = "1";

    public string PonEnd { get; set; } = "1";

    public string Vlan { get; set; } = "100";

    public string OnuStart { get; set; } = "1";

    public string OnuEnd { get; set; } = "128";

    public string TcontProfile { get; set; } = "1G";

    public string Prefix { get; set; } = "cliente";

    public string GemName1 { get; set; } = "GEM1";

    public string GemName2 { get; set; } = "GEM2";

    public string Header { get; set; } = string.Empty;

    public string Footer { get; set; } = string.Empty;

    public bool SecondaryService { get; set; }

    public string SecondaryVlan { get; set; } = string.Empty;
}
