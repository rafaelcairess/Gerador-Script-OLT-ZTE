namespace GeradorOlt.Nucleo.Validacao;

public sealed class ExcecaoValidacaoConfiguracao(string message)
    : ArgumentException(message);
