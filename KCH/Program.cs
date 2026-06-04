using KCH.Data;
using KCH.Forms;

namespace KCH;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        DatabaseHelper.Initialize();
        Application.Run(new LoginForm());
    }
}
