using System.Data;

namespace Mokkivaraus.Views;

public partial class MainMenuPage : ContentPage
{
    DatabaseHelper dbHelper = new DatabaseHelper();
    public MainMenuPage()
	{
		InitializeComponent();
        LoadData();
    }

    private async void LoadData()
    {
        DataTable dt = await dbHelper.GetDataAsync("SELECT * FROM mokki");
        foreach (DataRow row in dt.Rows)
                {
                    Console.WriteLine(row["mokkinimi"]); 
        }
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

    private void OnPalvelutClicked(object sender, EventArgs e)
    {
        // TODO: Navigoi Palvelut-sivulle
    }

    private void OnVarauksetClicked(object sender, EventArgs e)
    {
        // TODO: Navigoi Varaukset-sivulle
    }

    private async void OnAsiakkaatClicked(object sender, EventArgs e)
    {
        // TODO: Navigoi Asiakkaat-sivulle
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


}