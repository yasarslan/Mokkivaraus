namespace Mokkivaraus.Views
{
    public partial class AlueetPage : ContentPage
    {
        public AlueetPage()
        {
            InitializeComponent();
        }

        //MENU - sidebar////////////////////

        private async void OnMainMenuTapped(object sender, EventArgs e)
        {
            if (sender is Label label)
            {
                await label.TranslateTo(10, 0, 50);
                await label.TranslateTo(-10, 0, 50);
                await label.TranslateTo(5, 0, 50);
                await label.TranslateTo(0, 0, 50);
            }
            await Navigation.PushAsync(new Views.MainMenuPage());
        }
        private async void OnMokitClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MokitPage());
        }

        private async void OnAlueetClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AlueetPage());
        }

        private void OnPalvelutClicked(object sender, EventArgs e)
        {
            // TODO: Navigoi Palvelut-sivulle
        }

        private async void OnVarauksetClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new VarauksetViewPage());
        }

        private async void OnAsiakkaatClicked(object sender, EventArgs e)
        {
            
            await Navigation.PushAsync(new Views.Asiakkaat());
        }

        private void OnLaskutClicked(object sender, EventArgs e)
        {
            // TODO: Navigoi Laskut-sivulle
        }

        private void OnRaportitClicked(object sender, EventArgs e)
        {
            // TODO: Navigoi Raportit-sivulle
        }

        private async void OnLogoutTapped(object sender, EventArgs e)
        {

            await Navigation.PushAsync(new Views.LoginPage());


            Navigation.RemovePage(this);
        }


        private async void OnAddRegionClicked(object sender, EventArgs e)
        {

        }

        private void OnEditClicked(object sender, EventArgs e)
        {


        }

        private void OnSearchRegion(object sender, EventArgs e)
        {

        }
    }
}
