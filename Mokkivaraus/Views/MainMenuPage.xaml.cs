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

    private async void OnLogoutTapped(object sender, EventArgs e)
    {
        
        await Navigation.PushAsync(new Views.LoginPage());

        
        Navigation.RemovePage(this);
    }

    private void OnAddCabinClicked(object sender, EventArgs e)
    {
        // TODO: Tämä ohjaa mökin lisäyssivulle (backend kehittäjä hoitaa navigoinnin)
        DisplayAlert("Info", "Lisää mökki -painiketta klikattu", "OK");
    }

    private void OnAddReservationClicked(object sender, EventArgs e)
    {
        // TODO: Tämä ohjaa varauksen luontisivulle (backend kehittäjä hoitaa navigoinnin)
        DisplayAlert("Info", "Tee varaus -painiketta klikattu", "OK");
    }
}