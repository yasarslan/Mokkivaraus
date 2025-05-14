using MySql.Data.MySqlClient;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Mokkivaraus.Models;
using System.Threading.Tasks;

namespace Mokkivaraus.Views;

public partial class VarauksetViewPage : ContentPage
{
    private List<Alue> alueetLista = new List<Alue>(); // A list to hold the areas
    private List<Asiakas> asiakkaatLista = new List<Asiakas>(); // A list to hold the customers
    private List<Palvelu> palvelutLista = new List<Palvelu>(); // A list to hold the services
    private List<Mokki> mokitLista = new List<Mokki>(); // A list to hold the cabins
    private Varaukset? _editingVaraus = null; // null means adding, not null means editing
    public ObservableCollection<Varaukset> VarausLista = new ObservableCollection<Varaukset>();
    DatabaseHelper dbHelper = new DatabaseHelper();


    public VarauksetViewPage()
    {
        InitializeComponent();
        VarausListaView.ItemsSource = VarausLista;
        LoadVaraus();
        LoadAlueet();
        LoadAsiakkaat();
        LoadPalvelut();
        LoadMokit();
        BindingContext = this; // BindingContext to this page
    }

    public async Task<ObservableCollection<Varaukset>> GetVarausAsync()
    {
        const string GetVarauksetQuery = @"SELECT
                                            varaus.varaus_id,
                                            alue.nimi AS aluenimi,
                                            mokki.mokkinimi AS mokkinimi,
                                            varaus.varattu_pvm,
                                            varaus.varattu_alkupvm,
                                            varaus.varattu_loppupvm,
                                            asiakas.etunimi,
                                            asiakas.sukunimi,
                                            asiakas.puhelinnro,
                                            GROUP_CONCAT(palvelu.nimi, ' (', varauksen_palvelut.lkm, ' kpl)') AS palvelutVarattu,
                                            SUM(palvelu.hinta * varauksen_palvelut.lkm) AS kokonaisKustannus
                                          FROM varaus
                                          INNER JOIN mokki ON varaus.mokki_id = mokki.mokki_id
                                          INNER JOIN alue ON mokki.alue_id = alue.alue_id
                                          LEFT JOIN asiakas ON varaus.asiakas_id = asiakas.asiakas_id
                                          LEFT JOIN varauksen_palvelut ON varaus.varaus_id = varauksen_palvelut.varaus_id
                                          LEFT JOIN palvelu ON varauksen_palvelut.palvelu_id = palvelu.palvelu_id
                                          GROUP BY varaus.varaus_id
                                          ORDER BY varaus.varaus_id ASC;"; // SQL query to retrieve the data for making reservation

        var varaukset = new ObservableCollection<Varaukset>(); // Collection to store reservations fetched from the database
        try
        {
            var dataTable = await dbHelper.GetDataAsync(GetVarauksetQuery); // Fetch data from the database
            if (dataTable?.Rows != null) // Ensure there are rows in the fetched data
            {
                foreach (System.Data.DataRow row in dataTable.Rows)
                {
                    var varaus = new Varaukset
                    {
                        varausID = Convert.ToInt32(row["varaus_id"]),
                        asiakasVarattu = new Asiakas
                        {
                            etunimi = row["etunimi"]?.ToString(),
                            sukunimi = row["sukunimi"]?.ToString(),
                            puhelin = row["puhelinnro"]?.ToString(),
                        },
                        mokkiVarattu = new Mokki
                        {
                            MokkiNimi = row["mokkinimi"]?.ToString()
                        },
                        Alue = row["aluenimi"]?.ToString(),
                        palvelutVarattu = row["palvelutVarattu"]?.ToString(),
                        kokonaisKustannus = row["kokonaisKustannus"] != DBNull.Value
                                            ? Convert.ToDouble(row["kokonaisKustannus"])
                                            : null,
                        varausPaiva = Convert.ToDateTime(row["varattu_pvm"]),
                        varausAlku = Convert.ToDateTime(row["varattu_alkupvm"]),
                        varausLoppu = Convert.ToDateTime(row["varattu_loppupvm"]),
                    };
                    varaukset.Add(varaus); // Add the created reservation object to the ObservableCollection
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Virhe haettaessa varauksia: {ex.Message}");
            return new ObservableCollection<Varaukset>(); // Return an empty collection to avoid null issues
        }
        return varaukset ; // Return the populated collection of reservations
    }   

    

    
    private async void LoadVaraus()  // Load and add reservation to collection
    {
        var varausFromDb = await GetVarausAsync(); // Fetch booking
        if (varausFromDb != null) 
        {
            foreach (var varaus in varausFromDb)
            {
                VarausLista.Add(varaus); // Add each booking to the list                
            }
            Debug.WriteLine($"Loaded {VarausLista.Count} reservations into the list."); // Log the number of bookings loaded
        }
    }

    private async void LoadAlueet() // Load areas and add them to the alueetLista collection
    {
        var alueetFromDb = await GetAlueetAsync(); // Fetch areas 
        if (alueetFromDb != null)
        {
            foreach (var alue in alueetFromDb) // If areas were fetched successfully
            {
                alueetLista.Add(alue); // Add each area to the list
            }
            AluePicker.ItemsSource = alueetLista; // Set the ItemsSource of the Picker to the list of areas
            Debug.WriteLine($"Loaded {alueetLista.Count} areas into the Picker."); // Log the number of areas loaded
        }
    }

    private async Task<List<Alue>> GetAlueetAsync() // Mmethod to fetch areas data from the database
    {
        const string query = "SELECT alue_id, nimi FROM alue"; // SQL query to get area IDs and names
        var alueet = new List<Alue>(); // An empty list to hold the areas

        try
        {
            var dataTable = await dbHelper.GetDataAsync(query); // Get the results as a data table

            if (dataTable?.Rows != null) // Check if the data table has any rows
            {
                foreach (System.Data.DataRow row in dataTable.Rows) // Loop through each row in the data table
                {
                    var alue = new Alue
                    {
                        AlueId = Convert.ToInt32(row["alue_id"]), // Convert the alue_id field to an int
                        AlueNimi = row["nimi"].ToString() // Get the nimi field as a string
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

    private async void LoadAsiakkaat() // Method to load customers and add them to the customers collection
    {
        var asiakkaatFromDb = await GetAsiakkaatAsync(); // Fetch customers
        if (asiakkaatFromDb != null)
        {
            asiakkaatLista.Clear();
            foreach (var asiakas in asiakkaatFromDb) // If customers were fetched successfully
            {
                asiakkaatLista.Add(asiakas); // Add each customer to the list
            }
            AsiakasPicker.ItemsSource = asiakkaatLista; // Set the ItemsSource of the Picker to the list of customers
            Debug.WriteLine($"Loaded {asiakkaatLista.Count} customers into the Picker."); // Log the number of customers loaded
        }
    }

    private async Task<List<Asiakas>> GetAsiakkaatAsync() // Fetch customers data from the database
    {
        const string query = "SELECT asiakas_id, etunimi, sukunimi, puhelinnro FROM asiakas"; // SQL query to get area IDs and names
        var asiakkaat = new List<Asiakas>(); // An empty list to hold the areas

        try
        {
            var dataTable = await dbHelper.GetDataAsync(query); // Get the results as a data table

            if (dataTable?.Rows != null) // Check if the DataTable has any rows
            {
                foreach (System.Data.DataRow row in dataTable.Rows) // Loop through each row in the DataTable
                {
                    var asiakas = new Asiakas
                    {
                        asiakasID = Convert.ToInt32(row["asiakas_id"]), // Convert the 'asiakas_id' field to an int
                        etunimi = row["etunimi"].ToString(), // Get the etunimi field as a string
                        sukunimi = row["sukunimi"].ToString(), // Get the sukunimi field as a string
                        puhelin = row["puhelinnro"].ToString() // Get the puhelinnro field as a string
                    };
                    asiakkaat.Add(asiakas); // Add the created customer object to the list
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Virhe haettaessa asiakas: " + ex.Message);
        }

        return asiakkaat; // Return the filled list or empty list if something failed
    }

    private async void LoadPalvelut() // Load services and add them to the collection
    {
        var palvelutFromDb = await GetPalvelutAsync(); // Fetch services
        if (palvelutFromDb != null)
        {
            palvelutLista.Clear(); // Clear the existing list of services
            foreach (var palvelu in palvelutFromDb)
            {
                palvelutLista.Add(palvelu);
            }
            PalvelutPicker.ItemsSource = palvelutLista; // Set the ItemsSource of the Picker to the list of services

            Debug.WriteLine($"Loaded {palvelutLista.Count} services into memory."); // Log the number of services loaded
        }
    }

    private async Task<List<Palvelu>> GetPalvelutAsync() // Fetch services data from the database
    {
        const string query = "SELECT palvelu_id, alue_id, nimi, hinta FROM palvelu"; // SQL query to get services ids, names, areas they belong to and prices
        var palvelut = new List<Palvelu>(); // Empty list to hold the services
        try
        {
            var dataTable = await dbHelper.GetDataAsync(query); // Get the results as a data table
            if (dataTable?.Rows != null) // Check if the data table has any rows
            {
                foreach (System.Data.DataRow row in dataTable.Rows) // Loop through each row in the data table
                {
                    var palvelu = new Palvelu
                    {
                        PalveluID = Convert.ToInt32(row["palvelu_id"]), // Convert the service id field to an int
                        AlueID = Convert.ToInt32(row["alue_id"]), // Convert the area id field to an int
                        PalveluNimi = row["nimi"].ToString(), // Get the service name field as a string
                        Hinta = (decimal)Convert.ToDouble(row["hinta"]) // Get the price field as a string
                    };
                    palvelut.Add(palvelu); // Add the created service object to the list
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Virhe haettaessa asiakas: " + ex.Message);
        }
        return palvelut; // Return the filled list or empty list if something failed
    }


    private async void LoadMokit() // Load cabins and add them to the collection
    {
        var mokkitFromDb = await GetMokitAsync(); // Fetch areas 
        if (mokkitFromDb != null)
        {
            mokitLista.Clear();
            foreach (var mokki in mokkitFromDb) // If areas were fetched successfully
            {
                mokitLista.Add(mokki); // Add each area to the list
            }
            Debug.WriteLine($"Loaded {mokitLista.Count} cabins into the Picker."); // Log the number of areas loaded
        }
    }

    private async Task<List<Mokki>> GetMokitAsync() // Fetch cabins data from the database
    {
        const string query = "SELECT mokki_id, mokkinimi, alue_id FROM mokki "; 
        var mokit = new List<Mokki>(); // List to hold the areas

        try
        {
            var dataTable = await dbHelper.GetDataAsync(query); // Get the results as a data table

            if (dataTable?.Rows != null) // Check if the  data table has any rows
            {
                foreach (System.Data.DataRow row in dataTable.Rows) // Loop through each row in the  data table
                {
                    var mokki = new Mokki // Create a new Mokki object
                    {
                        Mokki_id = Convert.ToInt32(row["mokki_id"]), // Convert the mokki_id field to an int
                        MokkiNimi = row["mokkinimi"].ToString(), // Get the mokkinimi field as a string
                        AlueID = Convert.ToInt32(row["alue_id"]) // Convert the alue_id field to an int
                    };
                    mokit.Add(mokki); // Add the created cabin object to the list
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Virhe haettaessa asiakas: " + ex.Message);
        }

        return mokit; // Return the filled list or empty list if something failed
    }

    private void AluePicker_SelectedIndexChanged(object sender, EventArgs e) // Event handler for when the area picker selection changes
    {
        var selectedAlue = AluePicker.SelectedItem as Alue; // Get the selected area
        if (selectedAlue != null)
        {
            // Filter mokitLista in memory by selected alue
            var filteredMokit = mokitLista.Where(m => m.AlueID == selectedAlue.AlueId).ToList();
            MokkiPicker.SelectedItem = null; // Clear the selected item
            MokkiPicker.ItemsSource = filteredMokit; // Set the ItemsSource of the Picker to the filtered list


            // Filter palvelutLista in memory by selected alue
            var filteredPalvelut = palvelutLista.Where(p => p.AlueID == selectedAlue.AlueId).ToList(); //  Set the ItemsSource of the Picker to the filtered list
            PalvelutPicker.SelectedItem = null; // Clear the selected item
            PalvelutPicker.ItemsSource = filteredPalvelut; // Set the ItemsSource of the Picker to the filtered list

        }
        else
        {
            MokkiPicker.SelectedItem = null; // Clear the selected item
            MokkiPicker.ItemsSource = null; // Clear the ItemsSource of the Picker
            PalvelutPicker.SelectedItem = null; // Clear the selected item
            PalvelutPicker.ItemsSource = null; // Clear the ItemsSource of the Picker
        }
    }

    private void AsiakasPicker_SelectedIndexChanged(object sender, EventArgs e) // Event handler for when the customer picker selection changes
    {
        var selectedAsiakas = AsiakasPicker.SelectedItem as Asiakas; // Get the selected Asiakas
        if (selectedAsiakas != null)
        {
            AsiakasPuhelinEntry.Text = selectedAsiakas.puhelin; // Set the phone number of the selected customer in the Entry
        }
    }

    
    private void OnSearchReservation(object sender, EventArgs e) // Event handler for when the search text changes. SEARCH BAR
    {
        var searchText = ReservationSearchBar.Text?.ToLower() ?? string.Empty; // Convert the search text to lowercase for case-insensitive comparison

        // If search is empty, show all
        if (string.IsNullOrWhiteSpace(searchText))
        {
            VarausListaView.ItemsSource = VarausLista; // Show all reservations
            return;
        }

        var filteredList = VarausLista.Where(v => // Filter the list based on the search text
            (v.asiakasVarattu?.etunimi?.ToLower().Contains(searchText) ?? false) ||
            (v.asiakasVarattu?.sukunimi?.ToLower().Contains(searchText) ?? false) ||
            (v.asiakasVarattu?.puhelin?.ToLower().Contains(searchText) ?? false) ||
            (v.mokkiVarattu?.MokkiNimi?.ToLower().Contains(searchText) ?? false) ||
            (v.Alue?.ToLower().Contains(searchText) ?? false)
        ).ToList(); // Convert the filtered list to a List<Varaukset>

        VarausListaView.ItemsSource = new ObservableCollection<Varaukset>(filteredList); // Set the filtered list as the ItemsSource of the ListView
    }

    //MENU - sidebar/////////////////////////////////////////////////////////////////////////////

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
        await Navigation.PushAsync(new LaskutPage());
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

    //MENU - sidebar  END /////////////////////////////////////////////////////////////////////////////

    private async void OnSaveVarausClicked(object sender, EventArgs e) // Event handler for when the save button is clicked
    {
        // VALIDATION 
        var selectedAsiakas = AsiakasPicker.SelectedItem as Asiakas; // Get the selected customer
        var selectedAlue = AluePicker.SelectedItem as Alue; // Get the selected area
        var selectedMokki = MokkiPicker.SelectedItem as Mokki; // Get the selected cabin
        var selectedPalvelu = PalvelutPicker.SelectedItem as Palvelu; // Get the selected service

        if (selectedAsiakas == null) // Check if a customer is selected
        {
            await DisplayAlert("Virhe", "Valitse asiakas ennen tallennusta.", "OK");
            return;
        }
        if (selectedAlue == null) // Check if an area is selected
        {
            await DisplayAlert("Virhe", "Valitse alue ennen tallennusta.", "OK");
            return;
        }
        if (selectedMokki == null) // Check if a cabin is selected
        {
            await DisplayAlert("Virhe", "Valitse mökki ennen tallennusta.", "OK");
            return;
        }
        // Palvelu can be null

        var varattuPvm = VarattupvmDatePicker.Date + VarattupvmTimePicker.Time; // Get the selected reservation made date and time
        var alkuPvm = VarattuAlkuPvmDatePicker.Date + VarattuAlkuPvmTimePicker.Time; // Get the selected start date and time
        var loppuPvm = VarattuLoppuPvmDatePicker.Date + VarattuLoppuPvmTimePicker.Time; // Get the selected end date and time

        if (alkuPvm < varattuPvm) // Check if the start date is before the reservation made date
        {
            await DisplayAlert("Virhe", "Varauksen alku ei voi olla ennen varattua päivää.", "OK");
            return;
        }
        if (loppuPvm <= alkuPvm) // Check if the end date is before or equal to the start date
        {
            await DisplayAlert("Virhe", "Varauksen loppu pitää olla varauksen alun jälkeen.", "OK");
            return;
        }

                                                                                // EDITING PART
        if (_editingVaraus != null)
        {
            // Update Varaus
            string updateQuery = @"
            UPDATE varaus
            SET asiakas_id = @asiakas_id,
                mokki_id = @mokki_id,
                varattu_pvm = @varattu_pvm,
                varattu_alkupvm = @varattu_alkupvm,
                varattu_loppupvm = @varattu_loppupvm
            WHERE varaus_id = @varaus_id";

            if (selectedAsiakas == null || selectedMokki == null || _editingVaraus == null) // Check if data is null
            {
                await DisplayAlert("Error", "Missing selected data.", "OK");
                return;
            }

            var updateParams = new Dictionary<string, object> // Parameters for the update query
        {
            { "@asiakas_id", selectedAsiakas.asiakasID },
            { "@mokki_id", selectedMokki.Mokki_id },
            { "@varattu_pvm", varattuPvm },
            { "@varattu_alkupvm", alkuPvm },
            { "@varattu_loppupvm", loppuPvm },
            { "@varaus_id", _editingVaraus.varausID ?? 0 }
        };
            int rows = await dbHelper.ExecuteNonQueryAsync(updateQuery, updateParams); // Execute the update query

            // Update Palvelu            
            string deletePalvelutQuery = "DELETE FROM varauksen_palvelut WHERE varaus_id = @varaus_id"; // Remove old palvelut for this varaus
            
            if (_editingVaraus == null) // Check if the varaus is null
            {
                await DisplayAlert("Error", "No reservation selected for update.", "OK");
                return;
            }
            var delParams = new Dictionary<string, object> { { "@varaus_id", _editingVaraus.varausID ?? 0 } }; // Parameters for the delete query
            await dbHelper.ExecuteNonQueryAsync(deletePalvelutQuery, delParams); // Execute the delete query

            
            if (selectedPalvelu != null) // Insert new palvelu if selected
            {
                string insertPalveluQuery = @"INSERT INTO varauksen_palvelut (varaus_id, palvelu_id, lkm)
                                          VALUES (@varaus_id, @palvelu_id, @lkm)";

                if (_editingVaraus == null) // Check if the varaus is null
                {
                    await DisplayAlert("Error", "No reservation selected for update.", "OK");
                    return;
                }

                var palveluParams = new Dictionary<string, object>
            {
                { "@varaus_id", _editingVaraus.varausID ?? 0 },
                { "@palvelu_id", selectedPalvelu.PalveluID },
                { "@lkm", 1 }
            };
                await dbHelper.ExecuteNonQueryAsync(insertPalveluQuery, palveluParams); // Execute the insert query
            }

            if (rows > 0) // Check if any rows were affected
            {                
                VarausLista.Clear(); // Clear the list
                LoadVaraus(); // Reload the list so palvelu and costs update
                await DisplayAlert("Onnistui", "Varaus päivitetty onnistuneesti!", "OK");
                PopupOverlay.IsVisible = false;
                _editingVaraus = null; // Clear the editing varaus
            }
            else
            {
                await DisplayAlert("Virhe", "Varauksen päivitys epäonnistui.", "OK");
            }
        }
        else
        {
                                                                                    // ADD NEW VARAUS
            string insertQuery = @"
            INSERT INTO varaus (asiakas_id, mokki_id, varattu_pvm, varattu_alkupvm, varattu_loppupvm)
            VALUES (@asiakas_id, @mokki_id, @varattu_pvm, @varattu_alkupvm, @varattu_loppupvm);
            SELECT LAST_INSERT_ID();";

            var insertParams = new Dictionary<string, object> // Parameters for the insert query
        {
            { "@asiakas_id", selectedAsiakas.asiakasID },
            { "@mokki_id", selectedMokki.Mokki_id },
            { "@varattu_pvm", varattuPvm },
            { "@varattu_alkupvm", alkuPvm },
            { "@varattu_loppupvm", loppuPvm }
        };

            
            object? result = await dbHelper.ExecuteScalarAsync(insertQuery, insertParams); // Insert query and get the last inserted ID
            int newVarausId = Convert.ToInt32(result); // Convert the result to an int

            // Insert Palvelu
            if (selectedPalvelu != null) // Insert new palvelu if selected
            {
                string insertPalveluQuery = @"INSERT INTO varauksen_palvelut (varaus_id, palvelu_id, lkm)
                                          VALUES (@varaus_id, @palvelu_id, @lkm)";

                var palveluParams = new Dictionary<string, object> // Parameters for the insert query
            {
                { "@varaus_id", newVarausId },
                { "@palvelu_id", selectedPalvelu.PalveluID },
                { "@lkm", 1 }
            };
                await dbHelper.ExecuteNonQueryAsync(insertPalveluQuery, palveluParams); // Execute the insert query
            }
            
            VarausLista.Clear(); // Clear the list
            LoadVaraus(); // Reload the list so palvelu and costs update
            await DisplayAlert("Onnistui", "Varaus lisätty onnistuneesti!", "OK");
            PopupOverlay.IsVisible = false;
        }
    }

    
    private void OnCancelPopupClicked(object sender, EventArgs e) // Event handler for when the cancel button is clicked   
    {
        PopupOverlay.IsVisible = false;
        _editingVaraus = null; // Clear the editing varaus
    }

    private void OnAddVarausClicked(object sender, EventArgs e) // Event handler for when the add button is clicked
    {
        _editingVaraus = null;

        DateTime today = DateTime.Today;

        // Set values directly using your actual control names
        VarattupvmDatePicker.Date = DateTime.Now.Date;
        VarattupvmTimePicker.Time = DateTime.Now.TimeOfDay;

        VarattuAlkuPvmDatePicker.Date = today;
        VarattuAlkuPvmTimePicker.Time = new TimeSpan(14, 0, 0); // 14:00

        VarattuLoppuPvmDatePicker.Date = today.AddDays(1);
        VarattuLoppuPvmTimePicker.Time = new TimeSpan(12, 0, 0); // 12:00

        // Show the popup
        PopupOverlay.IsVisible = true;
        PopupTitleLabel.Text = "Lisää varaus";

        // Clear pickers
        AsiakasPicker.SelectedItem = null;
        AsiakasPuhelinEntry.Text = string.Empty;
        AluePicker.SelectedItem = null;
        MokkiPicker.SelectedItem = null;
        PalvelutPicker.SelectedItem = null;

    }

    private async void OnVarausRowTapped(object sender, TappedEventArgs e) // Event handler for when a reservation row is tapped
    {
        // Get the tapped Varaukset object
        var frame = sender as Frame;
        if (frame == null) return;

        var varaus = frame.BindingContext as Varaukset; // Get the Varaukset object from the BindingContext
        if (varaus == null) return; // Check if varaus is null

        _editingVaraus = varaus; // Set the editing varaus to the tapped varaus

        PopupTitleLabel.Text = "Muokkaa varausta"; // Set popup title for editing

        // Populate fields with the reservation's data

        // Set customer picker
        AsiakasPicker.SelectedItem = asiakkaatLista.FirstOrDefault(a => // Find the customer in the list
            a.etunimi == varaus.asiakasVarattu?.etunimi && // Compare first name
            a.sukunimi == varaus.asiakasVarattu?.sukunimi); // Compare last name

        AsiakasPuhelinEntry.Text = varaus.asiakasVarattu?.puhelin ?? string.Empty; // Set the phone number in the Entry
        
        AluePicker.SelectedItem = alueetLista.FirstOrDefault(al => al.AlueNimi == varaus.Alue); // Set area picker

        await Task.Delay(50); // Sometimes needed for UI to catch up


        // Filter and set Mokki picker
        var selectedAlue = AluePicker.SelectedItem as Alue; // Get the selected area
        if (selectedAlue != null) // Check if an area is selected
        {
            var filteredMokit = mokitLista.Where(m => m.AlueID == selectedAlue.AlueId).ToList(); // Filter the cabins by the selected area
            MokkiPicker.ItemsSource = filteredMokit; // Set the ItemsSource of the Picker to the filtered list
            MokkiPicker.SelectedItem = filteredMokit.FirstOrDefault(m => m.MokkiNimi == varaus.mokkiVarattu?.MokkiNimi); // Set the selected cabin
        }
        else
        {
            MokkiPicker.ItemsSource = null; // Clear the ItemsSource of the Picker
            MokkiPicker.SelectedItem = null; // Clear the selected item
        }

        var filteredPalvelut = palvelutLista.Where(p => p.AlueID == selectedAlue?.AlueId).ToList(); // Filter the services by the selected area
        PalvelutPicker.ItemsSource = filteredPalvelut; // Set the ItemsSource of the Picker to the filtered list
                                                       // 
        if (!string.IsNullOrEmpty(varaus.palvelutVarattu)) // Check if there are any services booked
        {
            var palveluNames = varaus.palvelutVarattu.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries); // Split the booked services by comma
            PalvelutPicker.SelectedItem = filteredPalvelut.FirstOrDefault(p => palveluNames.Any(n => n.Contains(p.PalveluNimi))); // Set the selected service
        }


        // Set date and time pickers
        VarattupvmDatePicker.Date = DateTime.Now.Date; // Set Varattu pvm to now
        VarattupvmTimePicker.Time = DateTime.Now.TimeOfDay; // Set Varattu pvm time to now
        VarattuAlkuPvmDatePicker.Date = varaus.varausAlku.HasValue ? varaus.varausAlku.Value.Date : DateTime.Now.Date; // Set Varattu alku pvm to now
        VarattuAlkuPvmTimePicker.Time = varaus.varausAlku.HasValue ? varaus.varausAlku.Value.TimeOfDay : new TimeSpan(14, 0, 0); // Set Varattu alku time to 14:00
        VarattuLoppuPvmDatePicker.Date = varaus.varausLoppu.HasValue ? varaus.varausLoppu.Value.Date : DateTime.Now.Date.AddDays(1); // Set Varattu loppu pvm to tomorrow
        VarattuLoppuPvmTimePicker.Time = varaus.varausLoppu.HasValue ? varaus.varausLoppu.Value.TimeOfDay : new TimeSpan(12, 0, 0); // Set Varattu loppu time to 12:00

        PopupOverlay.IsVisible = true;
    }

    private async void OnDeleteClicked(object sender, EventArgs e) // Event handler for when the delete button is clicked
    {
        if (_editingVaraus == null) // Check if the editing varaus is null
        {
            await DisplayAlert("Virhe", "Et voi poistaa varausta, jota ei ole valittu.", "OK");
            return;
        }

        bool isConfirmed = await DisplayAlert( // Confirm deletion
            "Vahvista poisto",
            $"Oletko varma, että haluat poistaa varauksen mökistä '{_editingVaraus?.mokkiVarattu?.MokkiNimi}'? \nAsiakas: {_editingVaraus?.asiakasVarattu?.kokonaisNimi} ",
            "Kyllä", "Ei");

        if (!isConfirmed) // Check if the user confirmed deletion
            return; // Exit if not confirmed

        if (_editingVaraus == null) // Check if the varaus is null
        {
            await DisplayAlert("Error", "No reservation selected for update.", "OK");
            return;
        }

        bool success = await DeleteVarausFromDatabase(_editingVaraus); // Delete the varaus from the database

        if (success)
        {
            VarausLista.Remove(_editingVaraus); // Remove the varaus from the list
            _editingVaraus = null; // Clear the editing varaus
            PopupOverlay.IsVisible = false;
            await DisplayAlert("Onnistui", "Varaus poistettu onnistuneesti!", "OK");
        }
        else
        {
            await DisplayAlert("Virhe", "Varauksen poistaminen epäonnistui.", "OK");
        }
    }

    private async Task<bool> DeleteVarausFromDatabase(Varaukset varaus) // Method to delete a reservation from the database
    {
        try
        {
            string deletePalvelutQuery = "DELETE FROM varauksen_palvelut WHERE varaus_id = @varaus_id"; // Delete from varauksen_palvelut first, if exists

            var palvelutParams = new Dictionary<string, object> // Parameters for the delete query
        {
            { "@varaus_id", varaus.varausID ?? 0} // Use the varausID from the selected varaus
        };
            await dbHelper.ExecuteNonQueryAsync(deletePalvelutQuery, palvelutParams); // Execute the delete query

            string deleteVarausQuery = "DELETE FROM varaus WHERE varaus_id = @varaus_id"; // Then delete the varaus itself

            var varausParams = new Dictionary<string, object> // Parameters for the delete query
        {
            { "@varaus_id", varaus.varausID ?? 0} // Use the varausID from the selected varaus
        };
            int rowsAffected = await dbHelper.ExecuteNonQueryAsync(deleteVarausQuery, varausParams); // Execute the delete query

            return rowsAffected > 0; // Check if any rows were affected
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Virhe poistettaessa varausta: {ex.Message}");
            return false;
        }
    }   


}