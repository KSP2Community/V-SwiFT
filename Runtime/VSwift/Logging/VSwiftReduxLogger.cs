using ReduxLib.Logging;
using VSwift.Modules.Logging;

namespace VSwift.Logging
{
    /// <summary>
    /// <see cref="Modules.Logging.IVSwiftLogger" /> implementation that forwards to a <see cref="ReduxLib.Logging.ILogger" />.
    /// </summary>
    public class VSwiftReduxLogger : IVSwiftLogger
    {
        /// <summary>
        /// Creates the logger forwarding to the given <see cref="ReduxLib.Logging.ILogger" />.
        /// </summary>
        /// <param name="logger">The underlying Redux logger.</param>
        public VSwiftReduxLogger(ILogger logger)
        {
            Logger = logger;
        }

        /// <summary>
        /// Gets the underlying Redux logger.
        /// </summary>
        public ILogger Logger { get; }

        /// <inheritdoc />
        public void LogDebug(object debug)
        {
            Logger.LogDebug(debug);
        }

        /// <inheritdoc />
        public void LogInfo(object info)
        {
            Logger.LogInfo(info);
        }

        /// <inheritdoc />
        public void LogWarning(object warning)
        {
            Logger.LogWarning(warning);
        }

        /// <inheritdoc />
        public void LogError(object error)
        {
            Logger.LogError(error);
        }
    }
}
