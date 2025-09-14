using System.Windows;

namespace StarResonanceDpsAnalysis.WPF.Models;

public class ApplicationController : IApplicationController
{
    public void Shutdown()
    {
        Application.Current.Shutdown();
    }
}

public interface IApplicationController
{
    void Shutdown();
}