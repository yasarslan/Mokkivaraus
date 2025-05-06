using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Mokkivaraus.Views
{
    public partial class AlueetPage : ContentPage
    {
        private ObservableCollection<Alue> alueetLista = new ObservableCollection<Alue>();
        DatabaseHelper dbHelper = new DatabaseHelper(); // Instance of the DatabaseHelper class to manage database operations
        private Alue? _editingAlue = null; // null means adding, not null means editing

        public AlueetPage()
        {
            InitializeComponent();
            AlueListaView.ItemsSource = alueetLista; // Bind the ObservableCollection to a ListView for displaying
            LoadAlueet(); // Call method to load areas
            BindingContext = this; // Set this page as the binding context
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

        // Method to fetch areas data from the database
        private async Task<List<Alue>> GetAlueetAsync()
        {
            const string query = "SELECT alue_id, nimi FROM alue";
            var alueet = new List<Alue>(); // An empty list to hold the areas

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

        //Method to load areas and add them to the alueetLista collection
        private async void LoadAlueet()
        {
            alueetLista.Clear(); // Clear the existing list
            var alueetFromDb = await GetAlueetAsync(); // Fetch areas
            if (alueetFromDb != null)
            {
                foreach (var alue in alueetFromDb) // If areas were fetched successfully
                {
                    alueetLista.Add(alue); // Add each area to the list
                }
            }
        }


        private  void OnAddRegionClicked(object sender, EventArgs e)
        {
            // Reset the editing area state
            _editingAlue = null;

            // Set popup title for adding a new area
            PopupTitleLabel.Text = "Lis‰‰ alue";

            // Clear the input fields
            AlueNimiEntry.Text = "";

            // Show the popup for adding a new area
            PopupOverlay.IsVisible = true;

        }       


        private void OnAlueRowTapped(object sender, TappedEventArgs e)
        {
            var frame = sender as Frame;
            if (frame == null) return;

            var alue = frame.BindingContext as Alue;
            if (alue == null) return;

            _editingAlue = alue;

            // Set popup title for editing
            PopupTitleLabel.Text = "Muokkaa aluetta";

            // Populate fields with the area data
            AlueNimiEntry.Text = alue.AlueNimi ?? string.Empty;

            // Show popup
            PopupOverlay.IsVisible = true;

        }

        private async void OnSaveAlueClicked(object sender, EventArgs e)
        {
            var alueNimi = AlueNimiEntry.Text; // Get the entered area name

            if (string.IsNullOrWhiteSpace(alueNimi)) // Check if area name is valid
            {
                await DisplayAlert("Virhe", "Alueen nimi ei saa olla tyhj‰.", "OK");
                return;
            }

            var uusiAlue = new Alue // Create a new Alue object with the input values
            {
                AlueNimi = alueNimi
            };

            if (_editingAlue != null) // If editing an existing area, update it in the database
            {
                var updatedAlue = new Alue
                {
                    AlueId = _editingAlue.AlueId,
                    AlueNimi = alueNimi
                };

                bool successUpdate = await UpdateAlueInDatabase(updatedAlue); // Use updatedAlue for DB

                if (successUpdate)
                {
                    var index = alueetLista.IndexOf(_editingAlue);
                    if (index >= 0)
                    {
                        alueetLista[index] = updatedAlue; // Replace with new instance to trigger UI update
                    }

                    await DisplayAlert("Onnistui", "Alue p‰ivitetty onnistuneesti!", "OK");
                    PopupOverlay.IsVisible = false;
                }
                else
                {
                    await DisplayAlert("Virhe", "Alueen p‰ivitt‰minen ep‰onnistui.", "OK");
                }
            }
            else // If editing is not happening, add a new area to the database
            {
                bool success = await InsertAlueToDatabase(uusiAlue); // Insert the new area into the database

                if (success)
                {
                    alueetLista.Add(uusiAlue); // Add the new area to the list

                    await DisplayAlert("Onnistui", "Alue lis‰tty onnistuneesti!", "OK");

                    // Clear fields for new entry
                    AlueNimiEntry.Text = "";

                    PopupOverlay.IsVisible = false; // Hide the popup
                }
                else
                {
                    await DisplayAlert("Virhe", "Alueen lis‰‰minen ep‰onnistui.", "OK");
                }
            }
        }

        // Method to insert a new area into the database
        private async Task<bool> InsertAlueToDatabase(Alue alue)
        {
            const string insertQuery = @"
                                        INSERT INTO alue (nimi)
                                        VALUES (@nimi);
                                        SELECT LAST_INSERT_ID();";

            var parameters = new Dictionary<string, object> // Parameters for the SQL query
        {
            {"@nimi", alue.AlueNimi!}
        };

            try
            {
                object? result = await dbHelper.ExecuteScalarAsync(insertQuery, parameters);  // Execute the query and get the new area ID
                if (result != null && int.TryParse(result.ToString(), out int newId))
                {
                    alue.AlueId = newId; // Assign the new ID to the area object
                    return true; // Successfully inserted
                }
                else
                {
                    return false; // Insertion failed
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Virhe lis‰tt‰ess‰ aluetta tietokantaan: " + ex.Message);
                return false; // Handle database exception
            }
        }

        // Method to update an existing area in the database
        private async Task<bool> UpdateAlueInDatabase(Alue alue)
        {
            const string updateQuery = @"
                                        UPDATE alue 
                                        SET nimi = @nimi
                                        WHERE alue_id = @alue_id";

            var parameters = new Dictionary<string, object> // Parameters for the SQL query
        {
            { "@alue_id", alue.AlueId },
            { "@nimi", alue.AlueNimi! }
        };

            try
            {
                int result = await dbHelper.ExecuteNonQueryAsync(updateQuery, parameters); // Execute the update query
                return result > 0; // Return true if the update was successful
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Virhe p‰ivitt‰ess‰ aluetta tietokantaan: " + ex.Message);
                return false; // Handle database exception
            }
        }

        // Event handler for the cancel button in the popup
        private void OnCancelPopupClicked(object sender, EventArgs e)
        {
            PopupOverlay.IsVisible = false;
        }

        private async void OnPoistaClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var alueToDelete = button?.CommandParameter as Alue;

            if (alueToDelete != null)
            {
                // Ask for confirmation before deleting the area
                bool isConfirmed = await DisplayAlert("Vahvista poisto",
                                                       $"Oletko varma, ett‰ haluat poistaa alueen: {alueToDelete.AlueNimi}?",
                                                       "Kyll‰",
                                                       "Ei");

                if (isConfirmed)
                {
                    // Call the delete method to remove the area from the database
                    bool success = await DeleteAlueFromDatabase(alueToDelete);

                    if (success)
                    {
                        // If successful, remove the area from the list and update the UI
                        alueetLista.Remove(alueToDelete);

                        await DisplayAlert("Onnistui", "Alue poistettu onnistuneesti!", "OK");
                    }
                    else
                    {
                        await DisplayAlert("Virhe", "Alueen poistaminen ep‰onnistui.", "OK");
                    }
                }
            }
        }

        // Method to delete an area from the database
        private async Task<bool> DeleteAlueFromDatabase(Alue alue)
        {
            const string deleteQuery = "DELETE FROM alue WHERE alue_id = @alue_id";

            var parameters = new Dictionary<string, object> // Parameters for the SQL query
        {
            { "@alue_id", alue.AlueId }
        };

            try
            {
                int result = await dbHelper.ExecuteNonQueryAsync(deleteQuery, parameters); // Execute the delete query
                return result > 0; // Return true if deletion was successful
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Virhe poistaessa aluetta tietokannasta: " + ex.Message);
                return false; // Handle database exception
            }
        }

        private void OnSearchRegion(object sender, TextChangedEventArgs e)
        {
            var searchText = RegionSearchBar.Text?.ToLower() ?? string.Empty;
            var filteredList = alueetLista.Where(a =>
                (a.AlueNimi?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false)                
            ).ToList();

            AlueListaView.ItemsSource = new ObservableCollection<Alue>(filteredList);
        }
    }


}

