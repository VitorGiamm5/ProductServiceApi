namespace ProductServiceApp.Web.Clients;

public sealed class LoadingState
{
    private int _activeOperations;

    public bool IsLoading => _activeOperations > 0;

    public event Action? Changed;

    public IDisposable Begin()
    {
        _activeOperations++;
        Changed?.Invoke();

        return new LoadingScope(this);
    }

    private void End()
    {
        if (_activeOperations > 0)
        {
            _activeOperations--;
        }

        Changed?.Invoke();
    }

    private sealed class LoadingScope(LoadingState state) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            state.End();
        }
    }
}
