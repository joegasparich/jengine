/// <summary>
/// Abstraction for an easier Disposable pattern implementation.
/// Consumers should override the DisposeManagedObjects and DisposeUnmanagedObjects
/// to dispose the desired objects
/// https://gist.github.com/azborgonovo/c086d14bdab3900843c9
/// </summary>
/// <remarks>https://msdn.microsoft.com/pt-br/library/fs2xkftw(v=vs.110).aspx</remarks>
public abstract class Disposable : IDisposable
{
    protected bool disposed = false;

    // Public implementation of Dispose pattern callable by consumers.
    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    // Protected implementation of Dispose pattern.
    protected virtual void Dispose(bool disposing) {
        if (disposed)
            return;

        if (disposing)
            DisposeManagedObjects();

        DisposeUnmanagedObjects();
        disposed = true;
    }

    /// <summary>
    /// Free any managed objects
    /// </summary>
    protected virtual void DisposeManagedObjects() { }

    /// <summary>
    /// Free any unmanaged objects
    /// </summary>
    protected virtual void DisposeUnmanagedObjects() { }

    ~Disposable() {
        Dispose(false);
    }
}