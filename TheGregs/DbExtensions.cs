global using static TheGregs.DbExtensions;

using System;
using System.Data.Common;

namespace TheGregs;

/// <summary>
/// Contains extension methods for the <see cref="System.Data"/> namespace.
/// </summary>
static class DbExtensions {
    /// <summary>
    /// Creates a new command assigned to this connection with a specified
    /// <see cref="DbCommand.CommandText"/>.
    /// </summary>
    /// <param name="commandText">
    /// The <see cref="DbCommand.CommandText"/> of the new command.
    /// </param>
    /// <returns>The created command.</returns>
    public static DbCommand CreateCommand(this DbConnection conn, string commandText) {
        DbCommand command = conn.CreateCommand();
        command.CommandText = commandText;
        return command;
    }

    /// <summary>
    /// Sets the value of a named <see cref="DbParameter"/> for this command.
    /// </summary>
    /// <remarks>
    /// Works regardless of whether an associated <see cref="DbParameter"/>
    /// already exists or not.
    /// </remarks>
    /// <param name="paramName">The name of the parameter.</param>
    /// <param name="value">The value to set the parameter to.</param>
    public static void SetParam(this DbCommand command, string paramName, object value) {
        if (!command.Parameters.Contains(paramName)) {
            // Parameter does not already exist
            // Create a new one
            DbParameter parameter = command.CreateParameter();
            parameter.ParameterName = paramName;
            parameter.Value = value;
            _ = command.Parameters.Add(parameter);
            return;
        }

        // Parameter already exists, just change its value
        command.Parameters[paramName].Value = value;
    }
}

