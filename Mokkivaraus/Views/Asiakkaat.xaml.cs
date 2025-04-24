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
        const string GetAsiakasQuery = "SELECT a.etunimi,a.sukunimi,a.postinro,a.lahiosoite,a.email,a.puhelinnro,v.varaus_id FROM asiakas a JOIN varaus v on a.asiakas_id=v.asiakas_id";

        //var asiakkaat = new ObservableCollection<Asiakas>();

        try
        {
            var dataTable = await dbHelper.GetDataAsync(GetAsiakasQuery);
            if (dataTable.Rows != null)
            {
                foreach (System.Data.DataRow row in dataTable.Rows)
                {
                    var asiakas = new Asiakas
                    {
                        etunimi = row["a.etunimi"].ToString(),
                        sukunimi = row["a.sukunimi"].ToString(),
                        postiNo = row["a.postinro"].ToString(),
                        lahiOsoite = row["a.lahiosoite"].ToString(),
                        email = row["a.email"].ToString(),
                        puhelin = row["a.puhekinnro"].ToString(),
                        varausID = Convert.ToInt32(row["v.varaus_id"])
                    };
                    AsiakasLista.Add(asiakas);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Virhe haettaessa asiakkaita: {ex.Message}");
            return new ObservableCollection<Asiakas>(); // Return an empty collection to avoid null issues
        }
        return AsiakasLista;
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

    private async void OnMokitClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new MokitPage());
    }

    private void OnAlueetClicked(object sender, EventArgs e)
    {
        // TODO: Navigoi Alueet-sivulle
    }

    private void OnPalvelutClicked(object sender, EventArgs e)
    {
        // TODO: Navigoi Palvelut-sivulle
    }

    private void OnVarauksetClicked(object sender, EventArgs e)
    {
        // TODO: Navigoi Varaukset-sivulle
    }

    private void OnAsiakkaatClicked(object sender, EventArgs e)
    {
        // TODO: Navigoi Asiakkaat-sivulle
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