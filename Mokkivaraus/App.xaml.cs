namespace Mokkivaraus
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new Views.MainMenuPage());

            // Set the main page to the login page
            //MainPage = new NavigationPage(new Views.LoginPage());
        }
    }
}
