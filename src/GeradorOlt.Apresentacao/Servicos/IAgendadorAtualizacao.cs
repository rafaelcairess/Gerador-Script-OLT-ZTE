namespace GeradorOlt.Apresentacao.Servicos;

public interface IAgendadorAtualizacao
{
    public void Schedule(Action action);
    public void Cancel();
}
