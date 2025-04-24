namespace Mokkivaraus.Views;
using MySql.Data.MySqlClient;
using System.Collections.ObjectModel;
using System.Diagnostics;

public partial class MokitPage : ContentPage
{
    private ObservableCollection<Mokki> mokkiLista = new ObservableCollection<Mokki>(); // ObservableCollection to hold the list of cabins (Mokit)
    DatabaseHelper dbHelper = new DatabaseHelper(); // Instance of the DatabaseHelper class to manage database operations

    public async Task<ObservableCollection<Mokki>>GetCabinsAsync() // Async method to fetch cabins data from the database
    {
        const string GetCabinsQuery = "SELECT mokkinimi, alue_id, hinta, varustelu FROM mokki"; // SQL query to retrieve cabin data

        var cabins = new ObservableCollection<Mokki>(); // Collection to store cabins fetched from the database
        try
        {
            var dataTable = await dbHelper.GetDataAsync(GetCabinsQuery); // Asynchronous call to fetch data from the database
            if (dataTable?.Rows != null) //Ensure there are rows in the fetched data
            {
                foreach (System.Data.DataRow row in dataTable.Rows)
                {
                    var mokki = new Mokki  // Create a Mokki object and populate it with data from each row
                    {
                        MokkiNimi = row["mokkinimi"].ToString(),
                        Katuosoite = row["alue_id"].ToString(),
                        Hinta = Convert.ToDecimal(row["hinta"]),
                        Varustelu = row["varustelu"].ToString()
                    };
                    cabins.Add(mokki); // Add the created Mokki object to the ObservableCollection
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Virhe haettaessa mökkejä: {ex.Message}");
            return new ObservableCollection<Mokki>(); // Return an empty collection to avoid null issues
        }
        return cabins; // Return the populated collection of cabins
    }


   
    public MokitPage()
	{
		InitializeComponent();
        MokkiListaView.ItemsSource = mokkiLista; // Bind the ObservableCollection to a ListView for displayin
        LoadMokit(); // Call method to load cabins asynchronously
    }

    private async void LoadMokit()  // Async method to load cabins and add them to the mokkiLista collection
    {
        var cabinsFromDb = await GetCabinsAsync(); // Fetch cabins asynchronously
        if (cabinsFromDb != null) // If cabins were fetched successfully
        {
            foreach (var mokki in cabinsFromDb)
            {
                mokkiLista.Add(mokki); // Add each cabin to the list
            }
        }
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

    //lisäämökki popuplist
    private void OnAddCabinClicked(object sender, EventArgs e)
    {
        PopupOverlay.IsVisible = true;
    }


    private async void OnSavePopupClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Tallenna", "Mökki lisätty (esimerkki)", "OK");
        PopupOverlay.IsVisible = false;
    }

    private async void OnSaveCabinClicked(object sender, EventArgs e)
    {
        var yeniMokki = new Mokki
        {
            Alue = AlueEntry.Text,
            Postinumero = PostinroEntry.Text,
            MokkiNimi = NimiEntry.Text,
            Katuosoite = SijaintiEntry.Text,
            Hinta = decimal.TryParse(HintaEntry.Text, out decimal hinta) ? hinta : 0,
            Kuvaus = KuvausEntry.Text,
            Henkilomaara = int.TryParse(KapasiteettiEntry.Text, out int kapasiteetti) ? kapasiteetti : 0,
            Varustelu = VarusteluEntry.Text
        };

        bool success = await InsertCabinToDatabase(yeniMokki);

        if (success)
        {
            mokkiLista.Add(yeniMokki);
            await DisplayAlert("Onnistui", "Mökki lisätty onnistuneesti!", "OK");

            // Clear fields
            NimiEntry.Text = "";
            SijaintiEntry.Text = "";
            HintaEntry.Text = "";
            KapasiteettiEntry.Text = "";
            AlueEntry.Text = "";
            VarusteluEntry.Text = "";
            PostinroEntry.Text = "";
            KuvausEntry.Text = "";

            PopupOverlay.IsVisible = false;
        }

        else 
        {
            await DisplayAlert("Virhe", "Mökin lisääminen epäonnistui.", "OK");
        }

           
    }

    private async Task<bool> InsertCabinToDatabase(Mokki mokki)
    {
        const string insertQuery = @"
        INSERT INTO mokki (alue_id, postinro, mokkinimi, katuosoite, hinta, kuvaus, henkilomaara, varustelu)
        VALUES (@alue_id, @postinro, @mokkinimi, @katuosoite, @hinta, @kuvaus, @henkilomaara, @varustelu)";

        var parameters = new Dictionary<string, object>
    {
        {"@mokkinimi", mokki.MokkiNimi!},
        {"@alue_id", mokki.Alue!},
        {"@hinta", mokki.Hinta},
        {"@henkilomaara", mokki.Henkilomaara},
        {"@katuosoite", mokki.Katuosoite!},
        {"@varustelu", mokki.Varustelu!},
        {"@postinro", mokki.Postinumero!},
        {"@kuvaus", mokki.Kuvaus!}
    };

        DatabaseHelper dbHelper = new DatabaseHelper();
        try
        {
            int result = await dbHelper.ExecuteNonQueryAsync(insertQuery, parameters);
            return result > 0;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Virhe lisättäessä mökkiä tietokantaan: " + ex.Message);
            return false;
        }
    }

    private void OnCancelPopupClicked(object sender, EventArgs e)
    {
        PopupOverlay.IsVisible = false;
    }

    private void OnPoistaClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;
        var mokki = (Mokki)button.CommandParameter;

        mokkiLista.Remove(mokki);
    }

    private void OnEditCabinClicked(object sender, EventArgs e)
    {
        // TODO: Backend 
    }

}