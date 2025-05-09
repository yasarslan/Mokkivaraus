namespace Mokkivaraus
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            


            //MainPage = new NavigationPage(new Views.MainMenuPage());

            // Set the main page to the login page
            MainPage = new NavigationPage(new Views.LoginPage());
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = base.CreateWindow(activationState);

#if WINDOWS
            window.Width = 1200;  
            window.Height = 800; 
#endif

            return window;
        }
    }
}
    