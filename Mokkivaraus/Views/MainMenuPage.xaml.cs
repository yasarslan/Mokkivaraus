using System.Data;
using System.Threading.Tasks;

namespace Mokkivaraus.Views;

public partial class MainMenuPage : ContentPage
{
    DatabaseHelper dbHelper = new DatabaseHelper();
    public MainMenuPage()
	{
		InitializeComponent();
        
    }

    


    protected override  void OnAppearing()
    {
        base.OnAppearing();
        NavigationPage.SetHasBackButton(this, false); // hide back button        
    }








    //MENU - sidebar////////////////////

    private async void OnMokitClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new MokitPage());
    }

    private async void OnAlueetClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AlueetPage());
    }

    private async void OnPalvelutClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new PalvelutPage());
    }

    private async void OnVarauksetClicked(object sender, EventArgs e)
    {
        // TODO: Navigoi Varaukset-sivulle
        await Navigation.PushAsync(new Views.VarauksetViewPage());
    }

    private async void OnAsiakkaatClicked(object sender, EventArgs e)
    {
        //Navigoi Asiakkaat-sivulle
        await Navigation.PushAsync(new Views.Asiakkaat());
    }

    private async void OnLaskutClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Views.LaskutPage());
    }

    private async void OnRaportitClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Views.RaportitPage());

    }

    private async void OnLogoutTapped(object sender, EventArgs e)
    {

        await Navigation.PushAsync(new Views.LoginPage());


        Navigation.RemovePage(this);
    }

    private void OnVarauksetClicked(object sender, TappedEventArgs e)
    {

    }
}