using AutoFnhk.Input;

namespace AutoFnhk.AI;

/// <summary>
/// Runs the AI decision loop for the user's private Fortnoob test environment.
/// An observation provider can feed live game state; this controller then
/// decides, logs the reason, and executes only supported keyboard actions.
/// </summary>
public sealed class AiRuntimeController : IDisposable
{
    private readonly AiDecisionEngine _decisionEngine;
    private readonly KeyboardActionExecutor _executor;
    private CancellationTokenSource? _cts;

    public event Action<AiDecision>? DecisionMade;

    public bool IsRunning => _cts is not null;

    public AiRuntimeController(
        AiDecisionEngine? decisionEngine = null,
        KeyboardActionExecutor? executor = null)
    {
        _decisionEngine = decisionEngine ?? new AiDecisionEngine();
        _executor = executor ?? new KeyboardActionExecutor();
    }

    public void Stop()
    {
        _cts?.Cancel();
        _cts = null;
    }

    public async Task RunAsync(
        Func<CancellationToken, Task<AiGameState>> observationProvider,
        AiPlan? plan,
        int skillLevel,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(observationProvider);
        Stop();
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var token = _cts.Token;

        try
        {
            while (!token.IsCancellationRequested)
            {
                var state = await observationProvider(token);
                var decision = _decisionEngine.Decide(state, plan, skillLevel);
                DecisionMade?.Invoke(decision);

                var actions = _decisionEngine.ToKeyboardActions(decision);
                if (actions.Count > 0)
                    await _executor.ExecuteSequenceAsync(actions, token);

                await Task.Delay(25, token);
            }
        }
        catch (OperationCanceledException)
        {
            // Normal stop.
        }
        finally
        {
            if (_cts is not null)
            {
                _cts.Dispose();
                _cts = null;
            }
        }
    }

    public void Dispose() => Stop();
}
