using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Mokkivaraus.Views;

public partial class PalvelutPage : ContentPage
{

    DatabaseHelper dbHelper = new DatabaseHelper(); // Instance of the DatabaseHelper to manage db operations
    private ObservableCollection<Palvelu> palveluLista = new ObservableCollection<Palvelu>(); // ObservableCollection to hold the list of services
    private List<Alue> alueetLista = new List<Alue>(); // A list to hold the areas 
    private Palvelu? _editingPalvelu = null; // null means adding, not null means editing


    public PalvelutPage()
	{
		InitializeComponent();             
        LoadPalvelut(); // Call method to load services 
        LoadAlueet(); // Call method to load areas 
        PalveluListaView.ItemsSource = palveluLista;
        BindingContext = this; // Set this page as the binding context
    }


    public async Task<ObservableCollection<Palvelu>> GetPalvelutAsync() // Async method to fetch services data from the database
    {
        const string GetPalvelutQuery = "SELECT palvelu_id, palvelu.nimi AS palvelun_nimi,  alue.nimi AS animi, kuvaus, hinta, alv FROM palvelu INNER JOIN alue ON palvelu.alue_id = alue.alue_id ORDER BY palvelu.nimi ASC"; // SQL query to retrieve services data

        var palvelut = new ObservableCollection<Palvelu>(); // Collection to store services fetched from the database
        try
        {
            var dataTable = await dbHelper.GetDataAsync(GetPalvelutQuery); // Asynchronous call to fetch data from the database
            if (dataTable?.Rows != null) //Ensure there are rows in the fetched data
            {
                foreach (System.Data.DataRow row in dataTable.Rows)
                {
                    var palvelu = new Palvelu  // Create a Palvelu object and populate it with data from each row
                    {
                        PalveluID = Convert.ToInt32(row["palvelu_id"]),
                        PalveluNimi = row["palvelun_nimi"].ToString(),
                        AlueNimi = row["animi"].ToString(),
                        Kuvaus = row["kuvaus"].ToString(),
                        Hinta = Convert.ToDecimal(row["hinta"]),
                        Alv = Convert.ToDecimal(row["alv"])
                    };
                    palvelut.Add(palvelu); // Add the created Palvelu object to the ObservableCollection
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Virhe haettaessa mökkejä: {ex.Message}");
            return new ObservableCollection<Palvelu>(); // Return an empty collection to avoid null issues
        }
        return palvelut; // Return the populated collection of services

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

    private async void LoadPalvelut()  // Method to load services and add them to the palveluLista collection
    {
        palveluLista.Clear(); // clear old list
        var palvelutFromDb = await GetPalvelutAsync(); // Fetch services 
        if (palvelutFromDb != null) // If services were fetched successfully
        {
            foreach (var palvelu in palvelutFromDb)
            {
                palveluLista.Add(palvelu); // Add each service to the list
            }
        }

    }

    private async void LoadAlueet() // Method to load areas and add them to the alueetLista collection
    {
        var alueetFromDb = await GetAlueetAsync(); // Fetch areas 
        if (alueetFromDb != null)
        {
            foreach (var alue in alueetFromDb) // If areas were fetched successfully
            {
                alueetLista.Add(alue); // Add each area to the list
            }
            AluePicker.ItemsSource = alueetLista; // Set the ItemsSource of the Picker to the list of areas
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

    ////////
    ///
    private void OnAddPalvelutClicked(object sender, EventArgs e)
    {
        // Reset the editing service state
        _editingPalvelu = null;

        // Set popup title for adding a new service
        PopupTitleLabel.Text = "Lisää palvelu";

        // Clear the input fields
        PalveluNimiEntry.Text = "";
        KuvausEntry.Text = "";
        HintaEntry.Text = "";
        AluePicker.SelectedItem = null;
        AlvEntry.Text = "";


        // Show the popup for adding a new service
        PopupOverlay.IsVisible = true;

    }

  
    private void OnPalveluRowTapped(object sender, TappedEventArgs e)
    {
        // Get the tapped Palvelu object
        var frame = sender as Frame;
        if (frame == null) return;

        var palvelu = frame.BindingContext as Palvelu;
        if (palvelu == null) return;

        _editingPalvelu = palvelu;

        // Set popup title for editing
        PopupTitleLabel.Text = "Muokkaa palvelu";

        // Populate fields with the service's data
        PalveluNimiEntry.Text = palvelu.PalveluNimi ?? string.Empty;
        KuvausEntry.Text = palvelu.Kuvaus ?? string.Empty;
        HintaEntry.Text = palvelu.Hinta.ToString("0.##");
        AlvEntry.Text = palvelu.Alv.ToString("0.##");       

        // Set Picker selected item for Alue 
        if (AluePicker.ItemsSource != null && palvelu.AlueNimi != null)
        {
            var matchingAlue = alueetLista.FirstOrDefault(a => a.AlueNimi == palvelu.AlueNimi);
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

    private async void OnPoistaClicked(object sender, EventArgs e)
    {
        // Get the service to delete from the command parameter 
        var button = sender as Button;
        var palveluToDelete = button?.CommandParameter as Palvelu;

        if (palveluToDelete != null)
        {
            // Ask the user for confirmation before deleting
            bool isConfirmed = await DisplayAlert("Vahvista poisto",
                                                   $"Oletko varma, että haluat poistaa palvelu: {palveluToDelete.PalveluNimi}?",
                                                   "Kyllä",
                                                   "Ei");

            if (isConfirmed)
            {
                // Call the delete method to remove the service from the database
                bool success = await DeletePalveluFromDatabase(palveluToDelete);

                if (success)
                {
                    // If successful, remove the service from the list and update the UI
                    palveluLista.Remove(palveluToDelete);
                   
                    await DisplayAlert("Onnistui", "Palvelu poistettu onnistuneesti!", "OK");
                }
                else
                {
                    await DisplayAlert("Virhe", "Palvelun poistaminen epäonnistui.", "OK");
                }
            }
        }
    }

    private async Task<bool> DeletePalveluFromDatabase(Palvelu palveluToDelete)
    {
        try
        {
            // SQL query to delete the service from the database
            string query = "DELETE FROM palvelu WHERE palvelu_id = @palvelu_id";

            // Use the dbHelper to execute the query
            var parameters = new Dictionary<string, object>
        {
            { "@palvelu_id", palveluToDelete.PalveluID }
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

    private async void OnSavePalveluClicked(object sender, EventArgs e)
    {
        var selectedAlue = AluePicker.SelectedItem as Alue; // Get the selected area from the Picker
        if (selectedAlue == null) // Check if an area is selected, show an error if not
        {
            await DisplayAlert("Virhe", "Valitse alue ennen tallennusta.", "OK");
            return;
        }

        // Validate PalveluNimi
        if (string.IsNullOrWhiteSpace(PalveluNimiEntry.Text))
        {
            await DisplayAlert("Virhe", "Nimi ei saa olla tyhjä.", "OK");
            return;
        } 
        
        // Validate Kuvaus
        if (string.IsNullOrWhiteSpace(KuvausEntry.Text))
        {
            await DisplayAlert("Virhe", "Kuvaus ei saa olla tyhjä.", "OK");
            return;
        }        

        // Validate Hinta
        if (!decimal.TryParse(HintaEntry.Text, out decimal hinta) || hinta < 0)
        {
            await DisplayAlert("Virhe", "Syötä kelvollinen hinta.", "OK");
            return;
        }
        // Validate ALV
        if (!decimal.TryParse(AlvEntry.Text, out decimal alv) || alv < 0)
        {
            await DisplayAlert("Virhe", "Syötä kelvollinen ALV.", "OK");
            return;
        }

        var palveluu = new Palvelu // Create a new Palvelu object with the input values
        {
            AlueNimi = selectedAlue.AlueNimi, // Use AlueNimi for display in the UI
            AlueID = selectedAlue.AlueId, // Use AlueId for database operations
            PalveluNimi = PalveluNimiEntry.Text,
            Kuvaus = KuvausEntry.Text,
            Hinta = hinta,
            Alv = alv
        };


        if (_editingPalvelu != null) // If editing an existing cabin, update it in the database
        {
            _editingPalvelu.AlueNimi = selectedAlue.AlueNimi;
            _editingPalvelu.AlueID = selectedAlue.AlueId;
            _editingPalvelu.PalveluNimi = palveluu.PalveluNimi;
            _editingPalvelu.Kuvaus = palveluu.Kuvaus;
            _editingPalvelu.Hinta = palveluu.Hinta;
            _editingPalvelu.Alv = palveluu.Alv;
            

            bool successupdate = await UpdatePalveluInDatabase(_editingPalvelu); // Update the service in the database

            if (successupdate)
            {
                var index = palveluLista.IndexOf(_editingPalvelu); // Find the index of the updated service and replace it in the list
                if (index >= 0)
                {
                    var updatedPalvelu = new Palvelu
                    {
                        PalveluID = _editingPalvelu.PalveluID,
                        AlueNimi = selectedAlue.AlueNimi,
                        AlueID = selectedAlue.AlueId,
                        PalveluNimi = palveluu.PalveluNimi,
                        Kuvaus = palveluu.Kuvaus,
                        Hinta = palveluu.Hinta,
                        Alv = palveluu.Alv
                    };

                    palveluLista[index] = updatedPalvelu;
                }              

                // Notify the UI to refresh 
                OnPropertyChanged(nameof(palveluLista));

                await DisplayAlert("Onnistui", "Palvelu päivitetty onnistuneesti!", "OK");
                PopupOverlay.IsVisible = false; // Hide the popup
            }
            else
            {
                await DisplayAlert("Virhe", "Palvelun päivittäminen epäonnistui.", "OK");
            }
        }
        else
        {
            bool success = await InsertPalveluToDatabase(palveluu); // If editing is not happening, add a new service to the database

            if (success)
            {
                palveluLista.Add(palveluu);
                
                await DisplayAlert("Onnistui", "Palvelu lisätty onnistuneesti!", "OK");

                // Clear fields for new entry
                PalveluNimiEntry.Text = "";
                HintaEntry.Text = "";
                AluePicker.SelectedItem = null;
                KuvausEntry.Text = "";
                AlvEntry.Text = "";

                PopupOverlay.IsVisible = false; // Hide the popup
            }
            else
            {
                await DisplayAlert("Virhe", "Palvelun lisääminen epäonnistui.", "OK");
            }
        }

    }

    private async Task<bool> UpdatePalveluInDatabase(Palvelu palvelu) // Method to update an existing service in the database
    {
        const string updateQuery = @"
            UPDATE palvelu 
            SET nimi = @nimi, kuvaus = @kuvaus,
                hinta = @hinta,  alv = @alv, alue_id = @alue_id
            WHERE palvelu_id = @palvelu_id";   // SQL query to update an existing service in the database

        var parameters = new Dictionary<string, object> // Parameters for the SQL query
        {
            { "@palvelu_id", palvelu.PalveluID },
            { "@nimi", palvelu.PalveluNimi! },
            { "@alue_id", palvelu.AlueID },
            { "@kuvaus", palvelu.Kuvaus! },        
            { "@hinta", palvelu.Hinta },
            { "@alv", palvelu.Alv },
        };

        try
        {
            int result = await dbHelper.ExecuteNonQueryAsync(updateQuery, parameters); // Execute the update query
            return result > 0; // Return true if the update was successful
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Virhe päivittäessä palvelua tietokantaan: " + ex.Message);
            return false; // Handle database exception
        }
    }

    private async Task<bool> InsertPalveluToDatabase(Palvelu palvelu) // Method to insert a new service into the database
    {
        const string insertQuery = @"
        INSERT INTO palvelu (alue_id, nimi, kuvaus, hinta, alv)
        VALUES (@alue_id, @nimi, @kuvaus, @hinta, @alv);
        SELECT LAST_INSERT_ID(); ";  // SQL query to insert a new service into the database


        var parameters = new Dictionary<string, object> // Parameters for the SQL query
    {
        {"@nimi", palvelu.PalveluNimi!},
        {"@alue_id", palvelu.AlueID},
        {"@kuvaus", palvelu.Kuvaus!},
        {"@hinta", palvelu.Hinta},
        {"@alv", palvelu.Alv}
    };

        DatabaseHelper dbHelper = new DatabaseHelper();

        try
        {
            object result = await dbHelper.ExecuteScalarAsync(insertQuery, parameters);  // Execute the query and get the new service ID
            if (result != null && int.TryParse(result.ToString(), out int newId))
            {
                palvelu.PalveluID = newId; // Assign the new ID to the service object
                return true; // Successfully inserted
            }
            else
            {
                return false; // Insertion failed
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Virhe lisättäessä palvelua tietokantaan: " + ex.Message);
            return false; // Handle database exception
        }
    }

    private void OnCancelPopupClicked(object sender, EventArgs e)
    {
        PopupOverlay.IsVisible = false;
    }
    

    private void OnSearchRegion(object sender, TextChangedEventArgs e)
    {
        var searchText = PalveluSearchBar.Text?.ToLower() ?? string.Empty;
        var filteredList = palveluLista.Where(p =>
            (p.PalveluNimi?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
            (p.AlueNimi?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
            (p.Kuvaus?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
            (p.Hinta.ToString().Contains(searchText, StringComparison.OrdinalIgnoreCase) || 
            p.Alv.ToString().Contains(searchText, StringComparison.OrdinalIgnoreCase))
        ).ToList();

        PalveluListaView.ItemsSource = new ObservableCollection<Palvelu>(filteredList);
    }
}