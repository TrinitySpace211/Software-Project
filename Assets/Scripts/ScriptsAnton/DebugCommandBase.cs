using System;
using UnityEngine;

/// <summary>
/// The base of all Commands for the Cheat GUI
/// </summary>
public class DebugCommandBase {

    private string _commandId;
    private string _commandDescription;
    private string _commandFormat;

    public string commandId { get { return _commandId; } }
    public string commandDescription { get { return _commandDescription; } }
    public string commandFormat { get { return _commandFormat; } }

    /// <summary>
    /// Constructor of the base command
    /// </summary>
    /// <param name="id">The ID of the command</param>
    /// <param name="description">The Description</param>
    /// <param name="format">The command format as information</param>
    public DebugCommandBase(string id, string description, string format) {
        _commandId = id;
        _commandDescription = description;
        _commandFormat = format;
    }
}

/// <summary>
/// The standard Command class without an amount
/// </summary>
public class DebugCommand : DebugCommandBase {

    private Action command;

    /// <summary>
    /// Constructor of the Command
    /// </summary>
    /// <param name="id">The ID of the command</param>
    /// <param name="description">The Description</param>
    /// <param name="format">The command format as information</param>
    /// <param name="command">The command itself with all the lines that should be executed</param>
    public DebugCommand(string id, string description, string format, Action command) : base(id, description, format) {
        this.command = command;
    }

    /// <summary>
    /// Executes the command 
    /// </summary>
    public void Invoke() {
        command.Invoke();
    }
}

/// <summary>
/// The standard Command class with an amount collected
/// </summary>
/// <typeparam name="T1">The amount in the command</typeparam>
public class DebugCommand<T1> : DebugCommandBase {

    private Action<T1> command;

    /// <summary>
    /// Constructor of the Command
    /// </summary>
    /// <param name="id">The ID of the command</param>
    /// <param name="description">The Description</param>
    /// <param name="format">The command format as information</param>
    /// <param name="command">The command itself with all the lines that should be executed</param>
    public DebugCommand(string id, string description, string format, Action<T1> command) : base(id, description, format) {
        this.command = command;
    }

    /// <summary>
    /// Executes the command 
    /// </summary>
    public void Invoke(T1 value) {
        command.Invoke(value);
    }
}