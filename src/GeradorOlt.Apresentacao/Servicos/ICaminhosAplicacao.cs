namespace GeradorOlt.Apresentacao.Servicos;

public interface ICaminhosAplicacao
{
    public string DefaultProfilePath { get; }
    public string LegacyDefaultProfilePath { get; }
    public string InitialDirectory { get; }
}
