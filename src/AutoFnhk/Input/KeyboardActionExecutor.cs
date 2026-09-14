namespace AutoFnhk.Input;

/// <summary>
/// Executes named keyboard actions for the user's own Fortnoob test environment.
/// The caller remains responsible for deciding when an action is appropriate.
/// </summary>
public sealed class KeyboardActionExecutor
{
    private readonly KeyboardInputService _keyboard;

    public KeyboardActionExecutor(KeyboardInputService? keyboard = null)
    {
        _keyboard = keyboard ?? new KeyboardInputService();
    }

    public Task<bool> ExecuteAsync(
        KeyboardAction action,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);

        return action.Type switch
        {
            KeyboardActionType.Press => _keyboard.PressAsync(action.Key, cancellationToken),
            KeyboardActionType.Hold => _keyboard.HoldAsync(action.Key, action.DurationMs, cancellationToken),
            _ => Task.FromResult(false)
        };
    }

    public async Task<int> ExecuteSequenceAsync(
        IEnumerable<KeyboardAction> actions,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(actions);

        var count = 0;
        foreach (var action in actions)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (await ExecuteAsync(action, cancellationToken))
                count++;
        }

        return count;
    }
}
