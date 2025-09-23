using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;

namespace StarResonanceDpsAnalysis.WPF.ViewModels;

public partial class LogEntry : ObservableObject
{
    [ObservableProperty] private DateTime _timestamp;
    [ObservableProperty] private LogLevel _level;
    [ObservableProperty] private string _message = string.Empty;
    [ObservableProperty] private string _category = string.Empty;
    [ObservableProperty] private Exception? _exception;
}