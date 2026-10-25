using System.Windows.Threading;

namespace GeradorOlt.Apresentacao.Servicos;

public sealed class AgendadorAtualizacaoDispatcher : IAgendadorAtualizacao
{
    private static readonly TimeSpan Delay = TimeSpan.FromMilliseconds(200);
    private readonly DispatcherTimer _timer;
    private Action? _pendingAction;

    public AgendadorAtualizacaoDispatcher(Dispatcher dispatcher)
    {
        ArgumentNullException.ThrowIfNull(dispatcher);
        _timer = new DispatcherTimer(Delay, DispatcherPriority.Background, OnTick, dispatcher);
        _timer.Stop();
    }

    public void Schedule(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        _pendingAction = action;
        _timer.Stop();
        _timer.Start();
    }

    public void Cancel()
    {
        _timer.Stop();
        _pendingAction = null;
    }

    private void OnTick(object? sender, EventArgs eventArgs)
    {
        _timer.Stop();
        var action = _pendingAction;
        _pendingAction = null;
        action?.Invoke();
    }
}
