using GeradorOlt.Nucleo.Modelos;

namespace GeradorOlt.Nucleo.Geracao;

public interface IGeradorScript
{
    public ScriptGerado Generate(ConfiguracaoOlt configuration);
}
