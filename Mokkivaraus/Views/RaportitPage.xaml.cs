using System.Data;
using System.Threading.Tasks;

namespace Mokkivaraus.Views;

public partial class RaportitPage : ContentPage
{
    DatabaseHelper dbHelper = new DatabaseHelper();
    private List<Alue> alueetLista = new List<Alue>();

    public RaportitPage()
	{
		InitializeComponent();
        Loaded += RaportitPage_Loaded; // Event handler for when the page is fully loaded
    }    

    private async void RaportitPage_Loaded(object? sender, EventArgs e)
    {
        await LoadAlueet(); // Ensure the area data is loaded when the page is shown
        base.OnAppearing();
    }

    private async Task LoadAlueet()
    {
        alueetLista.Clear(); // Clear the list before loading new data
        string query = "SELECT alue_id, nimi FROM alue";
        var dt = await dbHelper.GetDataAsync(query);
        foreach (DataRow row in dt.Rows)
        {
            alueetLista.Add(new Alue
            {
                AlueId = Convert.ToInt32(row["alue_id"]),
                AlueNimi = row["nimi"].ToString()
            });
        }
        AluePicker.ItemsSource = alueetLista; // Bind the Picker to the list
        AluePicker.ItemDisplayBinding = new Binding("AlueNimi"); // Display the area name
    }

    private async void OnNaytaRaporttiClicked(object sender, EventArgs e)
    {
        string? raporttiTyyppi = RaporttiTyyppiPicker.SelectedItem as string;
        DateTime alku = AloitusPaivaPicker.Date;
        DateTime loppu = LoppuPaivaPicker.Date;
        var selectedAlue = AluePicker.SelectedItem as Alue;

        // Validate user input
        if (string.IsNullOrEmpty(raporttiTyyppi))
        {
            await DisplayAlert("Virhe", "Valitse raporttityyppi.", "OK");
            return;
        }
        if (selectedAlue == null)
        {
            await DisplayAlert("Virhe", "Valitse alue.", "OK");
            return;
        }

        // Fetch data depending on the report type
        List<RaporttiRivi> raporttiData;
        if (raporttiTyyppi == "Majoittumiset valituilla alueilla")
            raporttiData = await HaeMajoittumisetRaportti(selectedAlue.AlueId, alku, loppu);
        else
            raporttiData = await HaePalvelutRaportti(selectedAlue.AlueId, alku, loppu);

        // Display the data or nodatalabel
        if (raporttiData.Count == 0)
        {
            RaporttiCollectionView.ItemsSource = null;
            RaporttiCollectionView.IsVisible = false;
            NoDataLabel.IsVisible = true;
            SumLabel.IsVisible = false;
        }
        else
        {
            RaporttiCollectionView.ItemsSource = raporttiData;
            RaporttiCollectionView.IsVisible = true;
            NoDataLabel.IsVisible = false;

            // Calculate the sum of Hinta
            decimal sum = 0;
            foreach (var row in raporttiData)
            {
                if (decimal.TryParse(row.Hinta, out decimal hinta))
                    sum += hinta;
            }
            SumLabel.Text = $"Yhteensä: {sum:C}"; 
            SumLabel.IsVisible = true;
        }
    }

    private async Task<List<RaporttiRivi>> HaeMajoittumisetRaportti(int alueId, DateTime alku, DateTime loppu) // Fetch booking report data based on selected area and dates
    {
        string query = @"
        SELECT a.nimi AS Alue, 
               CONCAT(c.etunimi, ' ', c.sukunimi) AS Asiakas,
               m.mokkinimi AS Tuote,
               v.varattu_alkupvm AS Paivamaara,
               m.hinta AS Hinta
        FROM varaus v
        JOIN mokki m ON v.mokki_id = m.mokki_id
        JOIN alue a ON m.alue_id = a.alue_id
        JOIN asiakas c ON v.asiakas_id = c.asiakas_id
        WHERE m.alue_id = {0}
          AND v.varattu_alkupvm BETWEEN '{1}' AND '{2}'
        ORDER BY v.varattu_alkupvm";
        query = string.Format(query, alueId, alku.ToString("yyyy-MM-dd"), loppu.ToString("yyyy-MM-dd"));

        var dt = await dbHelper.GetDataAsync(query);
        var list = new List<RaporttiRivi>();
        foreach (DataRow row in dt.Rows)
        {
            list.Add(new RaporttiRivi
            {
                Alue = row["Alue"].ToString() ?? "",
                Asiakas = row["Asiakas"].ToString() ?? "",
                Tuote = row["Tuote"].ToString() ?? "",
                Paivamaara = Convert.ToDateTime(row["Paivamaara"]).ToString("dd.MM.yyyy"),
                Hinta = row["Hinta"].ToString() ?? ""
            });
        }
        return list;
    }

    private async Task<List<RaporttiRivi>> HaePalvelutRaportti(int alueId, DateTime alku, DateTime loppu) // Fetch service report data based on selected area and dates
    {
        string query = @"
        SELECT a.nimi AS Alue,
               CONCAT(c.etunimi, ' ', c.sukunimi) AS Asiakas,
               p.nimi AS Tuote,
               v.varattu_alkupvm AS Paivamaara,
               p.hinta AS Hinta
        FROM varaus v
        JOIN mokki m ON v.mokki_id = m.mokki_id
        JOIN alue a ON m.alue_id = a.alue_id
        JOIN asiakas c ON v.asiakas_id = c.asiakas_id
        JOIN varauksen_palvelut vp ON v.varaus_id = vp.varaus_id
        JOIN palvelu p ON vp.palvelu_id = p.palvelu_id
        WHERE m.alue_id = {0}
          AND v.varattu_alkupvm BETWEEN '{1}' AND '{2}'
        ORDER BY v.varattu_alkupvm";
        query = string.Format(query, alueId, alku.ToString("yyyy-MM-dd"), loppu.ToString("yyyy-MM-dd"));

        var dt = await dbHelper.GetDataAsync(query);
        var list = new List<RaporttiRivi>();
        foreach (DataRow row in dt.Rows)
        {
            list.Add(new RaporttiRivi
            {
                Alue = row["Alue"].ToString() ?? "",
                Asiakas = row["Asiakas"].ToString() ?? "",
                Tuote = row["Tuote"].ToString() ?? "",
                Paivamaara = Convert.ToDateTime(row["Paivamaara"]).ToString("dd.MM.yyyy"),
                Hinta = row["Hinta"].ToString() ?? ""
            });
        }
        return list;
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

    private async void OnPalvelutClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Views.PalvelutPage());
    }

    private async void OnVarauksetClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new VarauksetViewPage());
    }

    private async void OnAsiakkaatClicked(object sender, EventArgs e)
    {
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

    //MENU - sidebar END ///////////////////
}