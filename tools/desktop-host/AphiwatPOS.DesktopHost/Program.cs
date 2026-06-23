namespace AphiwatPOS.DesktopHost;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        using var splash = new SplashForm();
        splash.Show();
        splash.Update();

        Application.Run(new MainForm(splash));
    }    
}
