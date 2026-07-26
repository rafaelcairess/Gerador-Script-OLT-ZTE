using GeradorOlt.Nucleo.Modelos;

namespace GeradorOlt.Nucleo.Geracao;

public interface IGeradorScript
{
    ScriptGerado Generate(ConfiguracaoOlt configuration);
}
