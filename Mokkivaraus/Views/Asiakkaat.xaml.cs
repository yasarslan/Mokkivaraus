using MySql.Data.MySqlClient;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Mokkivaraus.Models;
namespace Mokkivaraus.Views;

public partial class Asiakkaat : ContentPage
{
    public ObservableCollection<Asiakas> AsiakasLista = new ObservableCollection<Asiakas>();
    DatabaseHelper dbHelper = new DatabaseHelper();
    public async Task<ObservableCollection<Asiakas>> GetAsiakasAsync()
    {
        const string GetAsiakasQuery = "SELECT a.etunimi,a.sukunimi,a.postinro,a.lahiosoite,a.email,a.puhelinnro,v.varaus_id FROM vn.asiakas a INNER JOIN vn.varaus v on a.asiakas_id=v.asiakas_id";

        var customer = new ObservableCollection<Asiakas>();

        try
        {
            var dataTable = await dbHelper.GetDataAsync(GetAsiakasQuery);
            if (dataTable.Rows != null)
            {
                foreach (System.Data.DataRow row in dataTable.Rows)
                {
                    var asiakas = new Asiakas
                    {
                        etunimi = row["etunimi"].ToString(),
                        sukunimi = row["sukunimi"].ToString(),
                        postiNo = row["postinro"].ToString(),
                        lahiOsoite = row["lahiosoite"].ToString(),
                        email = row["email"].ToString(),
                        puhelin = row["puhelinnro"].ToString(),
                        varausID = Convert.ToInt32(row["varaus_id"])
                    };
                    customer.Add(asiakas);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Virhe haettaessa asiakkaita: {ex.Message}");
            return new ObservableCollection<Asiakas>(); // Return an empty collection to avoid null issues
        }
        return customer;
    }


    public Asiakkaat()
	{
		InitializeComponent();
        AsiakasListaView.ItemsSource = AsiakasLista;
        LoadAsiakas();
	}

    

    
    private async void LoadAsiakas()  // Load and add customers to collection
    {
        var asiakasFromDb = await GetAsiakasAsync(); // Fetch customers
        if (asiakasFromDb != null) // 
        {
            foreach (var asiakas in asiakasFromDb)
            {
                AsiakasLista.Add(asiakas); // Add each customer to the list
            }
        }
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
        //Navigoi Asiakkaat-sivulle
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

    private void OnAddCustomerClicked(object sender, EventArgs e)
    {

    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e) // Seach event handler to filter the customer list based on the search text
    {
        var searchText = CustomerSearchBar.Text?.ToLower() ?? string.Empty;
        var filteredList = AsiakasLista.Where(a =>
            (a.etunimi?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
            (a.sukunimi?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
            (a.postiNo?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
            (a.lahiOsoite?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
            (a.email?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
            (a.puhelin?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false)
            //a.varausID.ToString().Contains(searchText, StringComparison.OrdinalIgnoreCase)
        ).ToList();

        AsiakasListaView.ItemsSource = new ObservableCollection<Asiakas>(filteredList);
    }
}