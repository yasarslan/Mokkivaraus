using MySql.Data.MySqlClient;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Mokkivaraus.Models;
using System.Threading.Tasks;

namespace Mokkivaraus.Views;

public partial class Asiakkaat : ContentPage
{
    public ObservableCollection<Asiakas> AsiakasLista = new ObservableCollection<Asiakas>();
    DatabaseHelper dbHelper = new DatabaseHelper();
    private Asiakas? _editingAsiakas = null;
    public async Task<ObservableCollection<Asiakas>> GetAsiakasAsync()
    {
        const string GetAsiakasQuery = "SELECT asiakas_id, etunimi, sukunimi, postinro, lahiosoite, email, puhelinnro FROM asiakas ORDER BY etunimi ASC";

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
                        asiakasID = Convert.ToInt32(row["asiakas_id"]),
                        etunimi = row["etunimi"].ToString(),
                        sukunimi = row["sukunimi"].ToString(),
                        postiNo = row["postinro"].ToString(),
                        lahiOsoite = row["lahiosoite"].ToString(),
                        email = row["email"].ToString(),
                        puhelin = row["puhelinnro"].ToString(),
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
        LoadAsiakas();
        AsiakasListaView.ItemsSource = AsiakasLista;
        BindingContext = this; // Set this page as the binding context
    }

    

    
    private async void LoadAsiakas()  // Load and add customers to collection
    {
        AsiakasLista.Clear(); // Clear the existing list to avoid duplicates       
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

    private async void OnPalvelutClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new PalvelutPage());
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

    private void OnAddCustomerClicked(object sender, EventArgs e)
    {
        // Reset the editing customer state
        _editingAsiakas = null;

        // Set popup title for adding a new customer
        PopupTitleLabel.Text = "Lis‰‰ asiakas";

        // Clear the input fields
        EtunimiEntry.Text = "";
        SukunimiEntry.Text = "";
        PostinroEntry.Text = "";
        LahiosoiteEntry.Text = "";
        EmailEntry.Text = "";
        PuhelinEntry.Text = "";

        PoistaButton.IsVisible = false; // Hide the delete button in the popup
        // Show the popup for adding a new customer
        PopupOverlay.IsVisible = true;
        
    }   

    private async void OnSaveAsiakasClicked(object sender, EventArgs e)
    {        

        // Validate EtuNimi
        if (string.IsNullOrWhiteSpace(EtunimiEntry.Text))
        { 
            await DisplayAlert("Virhe", "Nimi ei saa olla tyhj‰.", "OK");
            return;
        }

        // Validate SukuNimi
        if (string.IsNullOrWhiteSpace(SukunimiEntry.Text))
        {
            await DisplayAlert("Virhe", "Nimi ei saa olla tyhj‰.", "OK");
            return;
        }

        // Validate Post code
        if (!int.TryParse(PostinroEntry.Text, out int postinro) || postinro < 0)
        {
            await DisplayAlert("Virhe", "Syˆt‰ kelvollinen posti numero.", "OK");
            return;
        }
        // Validate Address
        if (string.IsNullOrWhiteSpace(LahiosoiteEntry.Text))
        {
            await DisplayAlert("Virhe", "Osoite ei saa olla tyhj‰t.", "OK");
            return;
        }
        // Validate Email
        if (string.IsNullOrWhiteSpace(EmailEntry.Text) || !EmailEntry.Text.Contains("@"))
        {
            await DisplayAlert("Virhe", "Syˆt‰ kelvollinen s‰hkˆposti.", "OK");
            return;
        }
        // Validate Phone number
        if (string.IsNullOrWhiteSpace(PuhelinEntry.Text) || PuhelinEntry.Text.Length < 5)
        {
            await DisplayAlert("Virhe", "Syˆt‰ kelvollinen puhelinnumero.", "OK");
            return;
        }


        var asiakas = new Asiakas // Create a new Asiakas object with the input values
        {
            etunimi = EtunimiEntry.Text,
            sukunimi = SukunimiEntry.Text,
            postiNo = PostinroEntry.Text,
            lahiOsoite = LahiosoiteEntry.Text,
            email = EmailEntry.Text,
            puhelin = PuhelinEntry.Text
        };


        if (_editingAsiakas!= null) // If editing an existing customer, update it in the database
        {
            _editingAsiakas.etunimi = asiakas.etunimi;
            _editingAsiakas.sukunimi = asiakas.sukunimi;
            _editingAsiakas.postiNo = asiakas.postiNo;
            _editingAsiakas.lahiOsoite = asiakas.lahiOsoite;
            _editingAsiakas.email = asiakas.email;
            _editingAsiakas.puhelin = asiakas.puhelin;

            bool successupdate = await UpdateAsiakasInDatabase(_editingAsiakas); // Update the service in the database

            if (successupdate)
            {
                var index = AsiakasLista.IndexOf(_editingAsiakas); // Find the index of the updated service and replace it in the list
                if (index >= 0)
                {
                    var updatedAsiakas = new Asiakas
                    {
                        asiakasID = _editingAsiakas.asiakasID,
                        etunimi = asiakas.etunimi,
                        sukunimi = asiakas.sukunimi,
                        postiNo = asiakas.postiNo,
                        lahiOsoite = asiakas.lahiOsoite,
                        email = asiakas.email,
                        puhelin = asiakas.puhelin,
                        varausID = _editingAsiakas.varausID
                    };                    
                    AsiakasLista[index] = updatedAsiakas;
                }

                // Notify the UI to refresh 
                OnPropertyChanged(nameof(AsiakasLista));

                await DisplayAlert("Onnistui", "Asiakas p‰ivitetty onnistuneesti!", "OK");
                PopupOverlay.IsVisible = false; // Hide the popup
            }
            else
            {
                await DisplayAlert("Virhe", "Asiakas p‰ivitt‰minen ep‰onnistui.", "OK");
            }
        }
        else
        {
            bool success = await InsertAsiakasToDatabase(asiakas); // If editing is not happening, add a new service to the database

            if (success)
            {
                AsiakasLista.Add(asiakas);

                await DisplayAlert("Onnistui", "Asiakas lis‰tty onnistuneesti!", "OK");

                // Clear fields for new entry
                EtunimiEntry.Text = "";
                SukunimiEntry.Text = "";
                PostinroEntry.Text = "";
                LahiosoiteEntry.Text = "";
                EmailEntry.Text = "";
                PuhelinEntry.Text = "";

                PopupOverlay.IsVisible = false; // Hide the popup
            }
            else
            {
                await DisplayAlert("Virhe", "Asiakas lis‰‰minen ep‰onnistui.", "OK");
            }
        }

    }

    private async Task<bool> UpdateAsiakasInDatabase(Asiakas asiakas) // Method to update an existing service in the database
    {
        const string updateQuery = @"
            UPDATE asiakas 
            SET etunimi = @etunimi, sukunimi = @sukunimi, postinro = @postinro, lahiosoite = @lahiosoite, email = @email, puhelinnro = @puhelinnro
            WHERE asiakas_id = @asiakas_id"; // SQL query to update an existing customer in the database

        var parameters = new Dictionary<string, object> // Parameters for the SQL query
        {
            { "@asiakas_id", asiakas.asiakasID },
            { "@etunimi", asiakas.etunimi! },
            { "@sukunimi", asiakas.sukunimi! },
            { "@postinro", asiakas.postiNo! },
            { "@lahiosoite", asiakas.lahiOsoite! },
            { "@email", asiakas.email! },
            { "@puhelinnro", asiakas.puhelin! }
        };

        try
        {
            int result = await dbHelper.ExecuteNonQueryAsync(updateQuery, parameters); // Execute the update query
            return result > 0; // Return true if the update was successful
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Virhe p‰ivitt‰ess‰ asiakas tietokantaan: " + ex.Message);
            return false; // Handle database exception
        }
    }

    private async Task<bool> InsertAsiakasToDatabase(Asiakas asiakas) // Method to insert a new service into the database
    {
        const string insertQuery = @"
        INSERT INTO asiakas (etunimi, sukunimi, postinro, lahiosoite, email, puhelinnro)
        VALUES (@etunimi, @sukunimi, @postiNo, @lahiOsoite, @email, @puhelin);
        SELECT LAST_INSERT_ID(); ";  // SQL query to insert a new service into the database


        var parameters = new Dictionary<string, object> // Parameters for the SQL query
    {
        {"@etunimi", asiakas.etunimi!},
        {"@sukunimi", asiakas.sukunimi!},
        {"@postiNo", asiakas.postiNo!},
        {"@lahiOsoite", asiakas.lahiOsoite!},
        {"@email", asiakas.email!},
        {"@puhelin", asiakas.puhelin!}

    };

        DatabaseHelper dbHelper = new DatabaseHelper();

        try
        {
            object? result = await dbHelper.ExecuteScalarAsync(insertQuery, parameters);  // Execute the query and get the new customer ID
            if (result != null && int.TryParse(result.ToString(), out int newId))
            {
                asiakas.asiakasID = newId; // Assign the new ID to the customer object
                return true; // Successfully inserted
            }
            else
            {
                return false; // Insertion failed
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Virhe lis‰tt‰ess‰ asiakas tietokantaan: " + ex.Message);
            return false; // Handle database exception
        }
    }

    private async Task<bool> DeleteAsiakasFromDatabase(Asiakas asiakasToDelete)
    {
        try
        {
            // SQL query to delete the service from the database
            string query = "DELETE FROM asiakas WHERE asiakas_id = @asiakas_id";

            // Use the dbHelper to execute the query
            var parameters = new Dictionary<string, object>
        {
            { "@asiakas_id", asiakasToDelete.asiakasID }
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

    private void OnCancelPopupClicked(object sender, EventArgs e)
    {
        PopupOverlay.IsVisible = false;
    }

    private async void OnPoistaClicked(object sender, EventArgs e)
    {
        // Get the service to delete from the command parameter 
        var button = sender as Button;
        var asiakasToDelete = button?.CommandParameter as Asiakas;

        if (asiakasToDelete != null)
        {
            // Ask the user for confirmation before deleting
            bool isConfirmed = await DisplayAlert("Vahvista poisto",
                                                   $"Oletko varma, ett‰ haluat poistaa asiakas: {asiakasToDelete.kokonaisNimi}?",
                                                   "Kyll‰",
                                                   "Ei");

            if (isConfirmed)
            {
                // Call the delete method to remove the service from the database
                bool success = await DeleteAsiakasFromDatabase(asiakasToDelete);

                if (success)
                {
                    // If successful, remove the service from the list and update the UI
                    AsiakasLista.Remove(asiakasToDelete);
                    PopupOverlay.IsVisible = false;

                    await DisplayAlert("Onnistui", "Asiakas poistettu onnistuneesti!", "OK");
                }
                else
                {
                    await DisplayAlert("Virhe", "Asiakas poistaminen ep‰onnistui.", "OK");
                }
            }
        }
    }

    private void OnAsiakasRowTapped(object sender, TappedEventArgs e)
    {
        PoistaButton.IsVisible = true; // Show the delete button in the popup
        // Get the tapped Palvelu object
        var frame = sender as Frame;
        if (frame == null) return;

        var asiakas = frame.BindingContext as Asiakas;
        if (asiakas == null) return;

        _editingAsiakas = asiakas;

        PoistaButton.IsVisible = true; // Show the delete button in the popup
        PoistaButton.CommandParameter = asiakas; // <-- Set command parameter!


        // Set popup title for editing
        PopupTitleLabel.Text = "Muokkaa asiakas";

        // Populate fields with the service's data
        EtunimiEntry.Text = asiakas.etunimi ?? string.Empty;
        SukunimiEntry.Text = asiakas.sukunimi ?? string.Empty;
        PostinroEntry.Text = asiakas.postiNo ?? string.Empty;
        LahiosoiteEntry.Text = asiakas.lahiOsoite ?? string.Empty;
        EmailEntry.Text = asiakas.email ?? string.Empty;
        PuhelinEntry.Text = asiakas.puhelin ?? string.Empty;

        // Show popup
        PopupOverlay.IsVisible = true;
        
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
        ).ToList();

        AsiakasListaView.ItemsSource = new ObservableCollection<Asiakas>(filteredList);
    }



}