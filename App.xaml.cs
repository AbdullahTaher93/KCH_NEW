using System.Windows;

namespace KCH_New
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // Ensure DB is created and migrated
            using var db = new Data.AppDbContext();
            db.Database.EnsureCreated();
        }
    }
}
