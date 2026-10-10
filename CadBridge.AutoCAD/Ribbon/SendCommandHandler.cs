using CadApplication = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using System;
using System.Windows.Input;

namespace CadBridge.AutoCAD.Ribbon;

internal sealed class SendCommandHandler : ICommand
{
    private readonly string _command;

    public SendCommandHandler(string command) => _command = command;

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => true;

    public void Execute(object? parameter)
    {
        var document = CadApplication.DocumentManager.MdiActiveDocument;
        document?.SendStringToExecute(_command + " ", true, false, false);
    }
}
