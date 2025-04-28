namespace Mokkivaraus.Views;
using MySql.Data.MySqlClient;
using Mysqlx.Crud;
using System.Collections.ObjectModel;
using System.Diagnostics;

public partial class MokitPage : ContentPage
{
    private List<Alue> alueetLista = new List<Alue>(); // A list to hold the areas
    private ObservableCollection<Mokki> mokkiLista = new ObservableCollection<Mokki>(); // ObservableCollection to hold the list of cabins
    DatabaseHelper dbHelper = new DatabaseHelper(); // Instance of the DatabaseHelper class to manage database operations
    private Mokki _editingCabin = null; // null means adding, not null means editing
    private int totalCabins;
    private int availableCabins;

    public async Task<ObservableCollection<Mokki>>GetCabinsAsync() // Async method to fetch cabins data from the database
    {
        const string GetCabinsQuery = "SELECT mokki_id, mokkinimi, alue.nimi AS nimi, katuosoite, kuvaus, postinro, henkilomaara, hinta, varustelu FROM mokki INNER JOIN alue ON mokki.alue_id = alue.alue_id ORDER BY mokkinimi ASC"; // SQL query to retrieve cabin data

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
                        Mokki_id = Convert.ToInt32(row["mokki_id"]),
                        MokkiNimi = row["mokkinimi"].ToString(),
                        Alue = row["nimi"].ToString(),
                        Katuosoite = row["katuosoite"].ToString(),
                        Kuvaus = row["kuvaus"].ToString(),
                        Hinta = Convert.ToDecimal(row["hinta"]),
                        Varustelu = row["varustelu"].ToString(),
                        Postinumero = row["postinro"].ToString(),
                        Henkilomaara = Convert.ToInt32(row["henkilomaara"])
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

    public async Task<(int totalCabins, int availableCabins)> GetCabinsCountAsync() // Method to get the count of total and available cabins from the database
    {
        const string GetCabinsCountQuery = @"
        SELECT COUNT(*) AS TotalCabins 
        FROM mokki";

        const string GetAvailableCabinsQuery = @"
        SELECT COUNT(*) AS AvailableCabins
        FROM mokki
        WHERE varustelu = 'yes'"; // Consider cabins with 'varustelu' = 'yes' as available.

        int totalCabins = 0;
        int availableCabins = 0;

        try
        {
            // Fetch total number of cabins
            var totalDataTable = await dbHelper.GetDataAsync(GetCabinsCountQuery);
            if (totalDataTable?.Rows.Count > 0)
            {
                totalCabins = Convert.ToInt32(totalDataTable.Rows[0]["TotalCabins"]);
            }

            // Fetch available cabins
            var availableDataTable = await dbHelper.GetDataAsync(GetAvailableCabinsQuery);
            if (availableDataTable?.Rows.Count > 0)
            {
                availableCabins = Convert.ToInt32(availableDataTable.Rows[0]["AvailableCabins"]);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error fetching cabin count: {ex.Message}");
        }

        return (totalCabins, availableCabins);
    }

    public int TotalCabins // Property to hold the total number of cabins
    {
        get => totalCabins;
        set
        {
            totalCabins = value;
            OnPropertyChanged(); // Notify the UI that the value has changed
        }
    }

    public int AvailableCabins // Property to hold the total available number of cabins
    {
        get => availableCabins;
        set
        {
            availableCabins = value;
            OnPropertyChanged(); // Notify the UI that the value has changed
        }
    }

    protected override async void OnAppearing() // Method that is called when the page appears to fetch and display cabin counts
    {
        base.OnAppearing();

        var (total, available) = await GetCabinsCountAsync();

        TotalCabins = total; // Bind to the "Mökit yhteensä" label
        AvailableCabins = available; // Bind to the "Vapaat mökit" label
    }


    private async Task<List<Alue>> GetAlueetAsync() // Async method to fetch areas data from the database
    {
        const string query = "SELECT alue_id, nimi FROM alue"; // SQL query to get area IDs and names
        var alueet = new List<Alue>(); // Initialize an empty list to hold the areas

        try
        {
            var dataTable = await dbHelper.GetDataAsync(query); // Execute the query and get the results as a data table

            if (dataTable?.Rows != null) // Check if the DataTable has any rows
            {
                foreach (System.Data.DataRow row in dataTable.Rows) // Loop through each row in the DataTable
                {
                    var alue = new Alue
                    {
                        AlueId = Convert.ToInt32(row["alue_id"]), // Convert the 'alue_id' field to an integer
                        AlueNimi = row["nimi"].ToString() // Get the 'nimi' field as a string
                    };
                    alueet.Add(alue); // Add the created Alue object to the list
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Virhe haettaessa alueita: " + ex.Message);
        }

        return alueet; // Return the filled list or empty list if something failed
    }



    public MokitPage()
	{
		InitializeComponent();
        MokkiListaView.ItemsSource = mokkiLista; // Bind the ObservableCollection to a ListView for displayin
        LoadMokit(); // Call method to load cabins 
        LoadAlueet(); // Call method to load areas 
        BindingContext = this; // Set this page as the binding context
    }

    private async void LoadMokit()  // Async method to load cabins and add them to the mokkiLista collection
    {
        var cabinsFromDb = await GetCabinsAsync(); // Fetch cabins 
        if (cabinsFromDb != null) // If cabins were fetched successfully
        {
            foreach (var mokki in cabinsFromDb)
            {
                mokkiLista.Add(mokki); // Add each cabin to the list
            }
        }
    }

    private async void LoadAlueet() // Async method to load areas and add them to the alueetLista collection
    {
        var alueetFromDb = await GetAlueetAsync(); // Fetch areas asynchronously
        if (alueetFromDb != null)
        {
            foreach (var alue in alueetFromDb) // If areas were fetched successfully
            {
                alueetLista.Add(alue); // Add each area to the list
            }
            AluePicker.ItemsSource = alueetLista; // Set the ItemsSource of the Picker to the list of areas
        }
    }

    private void OnCabinRowTapped(object sender, TappedEventArgs e) // Event handler for when a cabin row is tapped to edit a cabin
    {
        // Get the tapped Mokki object
        var frame = sender as Frame;
        if (frame == null) return;

        var cabin = frame.BindingContext as Mokki;
        if (cabin == null) return;

        _editingCabin = cabin;

        // Set popup title for editing
        PopupTitleLabel.Text = "Muokkaa mökkiä";

        // Populate fields with the cabin's data
        NimiEntry.Text = cabin.MokkiNimi ?? string.Empty;
        SijaintiEntry.Text = cabin.Katuosoite ?? string.Empty;
        HintaEntry.Text = cabin.Hinta.ToString("0.##");
        VarusteluEntry.Text = cabin.Varustelu ?? string.Empty;
        PostinroEntry.Text = cabin.Postinumero ?? string.Empty;
        KuvausEntry.Text = cabin.Kuvaus ?? string.Empty;
        KapasiteettiEntry.Text = cabin.Henkilomaara.ToString();

        // Set Picker selected item for Alue 
        if (AluePicker.ItemsSource != null && cabin.Alue != null)
        {
            var matchingAlue = alueetLista.FirstOrDefault(a => a.AlueNimi == cabin.Alue);
            if (matchingAlue != null)
            {
                AluePicker.SelectedItem = matchingAlue; // Set the selected area in the Picker
            }
            else
            {
                AluePicker.SelectedItem = null; // If no match found, set to null
            }
        }
        else
        {
            AluePicker.SelectedItem = null; // If no items in the Picker, set to null
        }

        // Show popup
        PopupOverlay.IsVisible = true;
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

    //lisäämökki popuplist
    private void OnAddCabinClicked(object sender, EventArgs e)
    {
        // Reset the editing cabin state
        _editingCabin = null;

        // Set popup title for adding a new cabin
        PopupTitleLabel.Text = "Lisää mökki";

        // Clear the input fields
        NimiEntry.Text = "";
        SijaintiEntry.Text = "";
        HintaEntry.Text = "";
        KapasiteettiEntry.Text = "";
        AluePicker.SelectedItem = null;
        VarusteluEntry.Text = "";
        PostinroEntry.Text = "";
        KuvausEntry.Text = "";


        // Show the popup for adding a new cabin
        PopupOverlay.IsVisible = true;
    }


    private async void OnSavePopupClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Tallenna", "Mökki lisätty (esimerkki)", "OK");
        PopupOverlay.IsVisible = false;
    }

    private async void OnSaveCabinClicked(object sender, EventArgs e) // Event handler for the "Save Cabin" button click
    {
        var selectedAlue = AluePicker.SelectedItem as Alue; // Get the selected area from the Picker (dropdown)
        if (selectedAlue == null) // Check if an area is selected, show an error if not
        {
            await DisplayAlert("Virhe", "Valitse alue ennen tallennusta.", "OK");
            return;
        }

        // Validate Postinumero
        if (string.IsNullOrWhiteSpace(PostinroEntry.Text))
        {
            await DisplayAlert("Virhe", "Postinumero ei saa olla tyhjä.", "OK");
            return;
        }

        // Validate Henkilömäärä
        if (!int.TryParse(KapasiteettiEntry.Text, out int kapasiteetti) || kapasiteetti <= 0)
        {
            await DisplayAlert("Virhe", "Syötä kelvollinen henkilömäärä (vähintään 1).", "OK");
            return;
        }

        // Validate Hinta
        if (!decimal.TryParse(HintaEntry.Text, out decimal hinta) || hinta < 0)
        {
            await DisplayAlert("Virhe", "Syötä kelvollinen hinta.", "OK");
            return;
        }

        var yeniMokki = new Mokki // Create a new cabin object with the input values
        {
            Alue = selectedAlue.AlueNimi, // Use AlueNimi for display in the UI
            AlueID = selectedAlue.AlueId, // Use AlueId for database operations
            Postinumero = string.IsNullOrWhiteSpace(PostinroEntry.Text) ? null : PostinroEntry.Text, // Handle empty postal code,
            MokkiNimi = NimiEntry.Text,
            Katuosoite = SijaintiEntry.Text,
            Hinta = hinta,
            Kuvaus = KuvausEntry.Text,
            Henkilomaara = kapasiteetti,
            Varustelu = VarusteluEntry.Text
        };       


        if (_editingCabin != null) // If editing an existing cabin, update it in the database
        {
            _editingCabin.Alue = selectedAlue.AlueNimi;
            _editingCabin.AlueID = selectedAlue.AlueId;
            _editingCabin.Postinumero = yeniMokki.Postinumero;
            _editingCabin.MokkiNimi = yeniMokki.MokkiNimi;
            _editingCabin.Katuosoite = yeniMokki.Katuosoite;
            _editingCabin.Hinta = yeniMokki.Hinta;
            _editingCabin.Kuvaus = yeniMokki.Kuvaus;
            _editingCabin.Henkilomaara = yeniMokki.Henkilomaara;
            _editingCabin.Varustelu = yeniMokki.Varustelu;

            bool successupdate = await UpdateCabinInDatabase(_editingCabin); // Update the cabin in the database

            if (successupdate)
            {                
                var index = mokkiLista.IndexOf(_editingCabin); // Find the index of the updated cabin and replace it in the list
                if (index >= 0)
                {
                    mokkiLista[index] = _editingCabin; // Replace the old cabin with the updated one
                }

                // Update the total and available cabin counts
                var (total, available) = await GetCabinsCountAsync();
                TotalCabins = total;
                AvailableCabins = available;

                // Notify the UI to refresh (if using ObservableCollection, it happens automatically)
                OnPropertyChanged(nameof(mokkiLista)); // Make sure your page is bound to the ObservableCollection

                await DisplayAlert("Onnistui", "Mökki päivitetty onnistuneesti!", "OK");                
                PopupOverlay.IsVisible = false; // Hide the popup
            }
            else
            {
                await DisplayAlert("Virhe", "Mökin päivittäminen epäonnistui.", "OK");
            }
        }
        else
        {            
            bool success = await InsertCabinToDatabase(yeniMokki); // If editing is not happening, add a new cabin to the database

            if (success)
            {
                mokkiLista.Add(yeniMokki);

                // Update the total and available cabin counts
                var (total, available) = await GetCabinsCountAsync();
                TotalCabins = total;
                AvailableCabins = available;

                await DisplayAlert("Onnistui", "Mökki lisätty onnistuneesti!", "OK");

                // Clear fields for new entry
                NimiEntry.Text = "";
                SijaintiEntry.Text = "";
                HintaEntry.Text = "";
                KapasiteettiEntry.Text = "";
                AluePicker.SelectedItem = null;
                VarusteluEntry.Text = "";
                PostinroEntry.Text = "";
                KuvausEntry.Text = "";

                PopupOverlay.IsVisible = false; // Hide the popup
            }
            else
            {
                await DisplayAlert("Virhe", "Mökin lisääminen epäonnistui.", "OK");
            }
        }

    }

    private async Task<bool> InsertCabinToDatabase(Mokki mokki) // Method to insert a new cabin into the database
    {
        const string insertQuery = @"
        INSERT INTO mokki (alue_id, postinro, mokkinimi, katuosoite, hinta, kuvaus, henkilomaara, varustelu)
        VALUES (@alue_id, @postinro, @mokkinimi, @katuosoite, @hinta, @kuvaus, @henkilomaara, @varustelu);
        SELECT LAST_INSERT_ID(); ";  // SQL query to insert a new cabin into the database


        var parameters = new Dictionary<string, object> // Parameters for the SQL query
    {
        {"@mokkinimi", mokki.MokkiNimi!},
        {"@alue_id", mokki.AlueID},
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
            object result = await dbHelper.ExecuteScalarAsync(insertQuery, parameters);  // Execute the query and get the new cabin ID
            if (result != null && int.TryParse(result.ToString(), out int newId))
            {
                mokki.Mokki_id = newId; // Assign the new ID to the cabin object
                return true; // Successfully inserted
            }
            else
            {
                return false; // Insertion failed
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Virhe lisättäessä mökkiä tietokantaan: " + ex.Message);
            return false; // Handle database exception
        }
    }

    private async Task<bool> UpdateCabinInDatabase(Mokki mokki) // Method to update an existing cabin in the database
    {
        const string updateQuery = @"
            UPDATE mokki 
            SET alue_id = @alue_id, postinro = @postinro, mokkinimi = @mokkinimi, katuosoite = @katuosoite,
                hinta = @hinta, kuvaus = @kuvaus, henkilomaara = @henkilomaara, varustelu = @varustelu
            WHERE mokki_id = @mokki_id";   // SQL query to update an existing cabin in the database

        var parameters = new Dictionary<string, object> // Parameters for the SQL query
        {
            { "@mokki_id", mokki.Mokki_id },
            { "@mokkinimi", mokki.MokkiNimi! },
            { "@alue_id", mokki.AlueID },
            { "@hinta", mokki.Hinta },
            { "@henkilomaara", mokki.Henkilomaara },
            { "@katuosoite", mokki.Katuosoite! },
            { "@varustelu", mokki.Varustelu! },
            { "@postinro", mokki.Postinumero! },
            { "@kuvaus", mokki.Kuvaus! }
        };

        try
        {
            int result = await dbHelper.ExecuteNonQueryAsync(updateQuery, parameters); // Execute the update query
            return result > 0; // Return true if the update was successful
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Virhe päivittäessä mökkiä tietokantaan: " + ex.Message);
            return false; // Handle database exception
        }
    }

    private void OnCancelPopupClicked(object sender, EventArgs e)
    {
        PopupOverlay.IsVisible = false;
    }

    private async void OnPoistaClicked(object sender, EventArgs e)
    {
        // Get the cabin to delete from the command parameter 
        var button = sender as Button;
        var mokkiToDelete = button?.CommandParameter as Mokki;

        if (mokkiToDelete != null)
        {
            // Ask the user for confirmation before deleting
            bool isConfirmed = await DisplayAlert("Vahvista poisto",
                                                   $"Oletko varma, että haluat poistaa mökin: {mokkiToDelete.MokkiNimi}?",
                                                   "Kyllä",
                                                   "Ei");

            if (isConfirmed)
            {
                // Call the delete method to remove the cabin from the database
                bool success = await DeleteCabinFromDatabase(mokkiToDelete);

                if (success)
                {
                    // If successful, remove the cabin from the list and update the UI
                    mokkiLista.Remove(mokkiToDelete);

                    // Update the total and available cabin counts
                    var (total, available) = await GetCabinsCountAsync();
                    TotalCabins = total;
                    AvailableCabins = available;

                    await DisplayAlert("Onnistui", "Mökki poistettu onnistuneesti!", "OK");
                }
                else
                {
                    await DisplayAlert("Virhe", "Mökin poistaminen epäonnistui.", "OK");
                }
            }
        }
    }

    private async Task<bool> DeleteCabinFromDatabase(Mokki mokkiToDelete)
    {
        try
        {
            // SQL query to delete the cabin from the database
            string query = "DELETE FROM mokki WHERE mokki_id = @mokki_id";

            // Use the dbHelper to execute the query
            var parameters = new Dictionary<string, object>
        {
            { "@mokki_id", mokkiToDelete.Mokki_id }
        };

            // Execute the query and return true if successful
            int rowsAffected = await dbHelper.ExecuteNonQueryAsync(query, parameters);

            return rowsAffected > 0; // If rows were affected, deletion was successful
        }
        catch (MySql.Data.MySqlClient.MySqlException ex)
        {
            Debug.WriteLine("MySQL error occurred: " + ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("General error occurred: " + ex.Message);
            return false;
        }
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e) // Seach event handler to filter the cabin list based on the search text
    {
        var searchText = CabinSearchBar.Text?.ToLower() ?? string.Empty;
        var filteredList = mokkiLista.Where(m =>
            (m.MokkiNimi?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
            (m.Katuosoite?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
            (m.Alue?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
            (m.Postinumero?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
            (m.Kuvaus?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
            (m.Varustelu?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false)
        ).ToList();

        MokkiListaView.ItemsSource = new ObservableCollection<Mokki>(filteredList);
    }
}