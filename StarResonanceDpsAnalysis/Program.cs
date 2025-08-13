using System.Text;

namespace StarResonanceDpsAnalysis
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            AntdUI.Config.SetDpi(1.0F);//Ä¬ÈÏdpi
            //AntdUI.Config.TextRenderingHighQuality = true;
            //Application.SetHighDpiMode(HighDpiMode.SystemAware);
            //AntdUI.Config.SetCorrectionTextRendering("SAO Welcome TT", "Î¢ÈíÑÅºÚ");
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }


    }
}