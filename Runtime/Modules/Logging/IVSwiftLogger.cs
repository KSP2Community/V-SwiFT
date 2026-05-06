namespace VSwift.Modules.Logging
{
    /// <summary>
    /// Interface for the V-SwiFT module-side logger, forwarding to the host's logging stack.
    /// </summary>
    /// <remarks>
    /// Implementations are wired up by the V-SwiFT plugin host (see <see cref="Logging.VSwiftReduxLogger" />) so the modules assembly does not need to reference the host logging stack directly.
    /// </remarks>
    public interface IVSwiftLogger
    {
        /// <summary>
        /// Gets or sets the active logger instance, assigned by the V-SwiFT plugin during initialization.
        /// </summary>
        public static IVSwiftLogger Instance { get; set; } = null!;

        /// <summary>
        /// Logs the given object at debug level.
        /// </summary>
        /// <param name="debug">The object to log.</param>
        public void LogDebug(object debug);

        /// <summary>
        /// Logs the given object at info level.
        /// </summary>
        /// <param name="info">The object to log.</param>
        public void LogInfo(object info);

        /// <summary>
        /// Logs the given object at warning level.
        /// </summary>
        /// <param name="warning">The object to log.</param>
        public void LogWarning(object warning);

        /// <summary>
        /// Logs the given object at error level.
        /// </summary>
        /// <param name="error">The object to log.</param>
        public void LogError(object error);

    }
}
