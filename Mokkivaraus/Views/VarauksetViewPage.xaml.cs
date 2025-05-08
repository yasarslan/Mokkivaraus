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
        BindingContext = this; // Set the BindingContext to this page
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
                                          ORDER BY varaus.varaus_id ASC;"; // SQL query to retrieve services data

        var varaukset = new ObservableCollection<Varaukset>(); // Collection to store services fetched from the database
        try
        {
            var dataTable = await dbHelper.GetDataAsync(GetVarauksetQuery); // Asynchronous call to fetch data from the database
            if (dataTable?.Rows != null) //Ensure there are rows in the fetched data
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
                    varaukset.Add(varaus); // Add the created Palvelu object to the ObservableCollection
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Virhe haettaessa varauksia: {ex.Message}");
            return new ObservableCollection<Varaukset>(); // Return an empty collection to avoid null issues
        }
        return varaukset ; // Return the populated collection of services
    }   

    

    
    private async void LoadVaraus()  // Load and add booking to collection
    {
        var varausFromDb = await GetVarausAsync(); // Fetch booking
        if (varausFromDb != null) 
        {
            foreach (var varaus in varausFromDb)
            {
                VarausLista.Add(varaus); // Add each booking to the list
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

    private async void LoadAsiakkaat() // Async method to load areas and add them to the alueetLista collection
    {
        var asiakkaatFromDb = await GetAsiakkaatAsync(); // Fetch areas asynchronously
        if (asiakkaatFromDb != null)
        {
            asiakkaatLista.Clear();
            foreach (var asiakas in asiakkaatFromDb) // If areas were fetched successfully
            {
                asiakkaatLista.Add(asiakas); // Add each area to the list
            }
            AsiakasPicker.ItemsSource = asiakkaatLista; // Set the ItemsSource of the Picker to the list of areas
            Debug.WriteLine($"Loaded {asiakkaatLista.Count} customers into the Picker.");
        }
    }

    private async Task<List<Asiakas>> GetAsiakkaatAsync() // Async method to fetch areas data from the database
    {
        const string query = "SELECT asiakas_id, etunimi, sukunimi, puhelinnro FROM asiakas"; // SQL query to get area IDs and names
        var asiakkaat = new List<Asiakas>(); // Initialize an empty list to hold the areas

        try
        {
            var dataTable = await dbHelper.GetDataAsync(query); // Execute the query and get the results as a data table

            if (dataTable?.Rows != null) // Check if the DataTable has any rows
            {
                foreach (System.Data.DataRow row in dataTable.Rows) // Loop through each row in the DataTable
                {
                    var asiakas = new Asiakas
                    {
                        asiakasID = Convert.ToInt32(row["asiakas_id"]), // Convert the 'alue_id' field to an integer
                        etunimi = row["etunimi"].ToString(), // Get the 'nimi' field as a string
                        sukunimi = row["sukunimi"].ToString(), // Get the 'nimi' field as a string
                        puhelin = row["puhelinnro"].ToString() // Get the 'nimi' field as a string
                    };
                    asiakkaat.Add(asiakas); // Add the created Alue object to the list
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Virhe haettaessa asiakas: " + ex.Message);
        }

        return asiakkaat; // Return the filled list or empty list if something failed
    }

    private async void LoadPalvelut() // Async method to load areas and add them to the alueetLista collection
    {
        var palvelutFromDb = await GetPalvelutAsync();
        if (palvelutFromDb != null)
        {
            palvelutLista.Clear();
            foreach (var palvelu in palvelutFromDb)
            {
                palvelutLista.Add(palvelu);
            }
        
            Debug.WriteLine($"Loaded {palvelutLista.Count} services into memory.");
        }
    }

    private async Task<List<Palvelu>> GetPalvelutAsync() // Async method to fetch areas data from the database
    {
        const string query = "SELECT palvelu_id, alue_id, nimi, hinta FROM palvelu"; // SQL query to get area IDs and names
        var palvelut = new List<Palvelu>(); // Initialize an empty list to hold the areas
        try
        {
            var dataTable = await dbHelper.GetDataAsync(query); // Execute the query and get the results as a data table
            if (dataTable?.Rows != null) // Check if the DataTable has any rows
            {
                foreach (System.Data.DataRow row in dataTable.Rows) // Loop through each row in the DataTable
                {
                    var palvelu = new Palvelu
                    {
                        PalveluID = Convert.ToInt32(row["palvelu_id"]), // Convert the 'alue_id' field to an integer
                        AlueID = Convert.ToInt32(row["alue_id"]), // Convert the 'alue_id' field to an integer
                        PalveluNimi = row["nimi"].ToString(), // Get the 'nimi' field as a string
                        Hinta = (decimal)Convert.ToDouble(row["hinta"]) // Get the 'nimi' field as a string
                    };
                    palvelut.Add(palvelu); // Add the created Alue object to the list
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Virhe haettaessa asiakas: " + ex.Message);
        }
        return palvelut; // Return the filled list or empty list if something failed
    }


    private async void LoadMokit()
    {
        var mokkitFromDb = await GetMokitAsync(); // Fetch areas asynchronously
        if (mokkitFromDb != null)
        {
            mokitLista.Clear();
            foreach (var mokki in mokkitFromDb) // If areas were fetched successfully
            {
                mokitLista.Add(mokki); // Add each area to the list
            }
            Debug.WriteLine($"Loaded {mokitLista.Count} customers into the Picker.");
        }
    }

    private async Task<List<Mokki>> GetMokitAsync()
    {
        const string query = "SELECT mokki_id, mokkinimi, alue_id FROM mokki "; 
        var mokit = new List<Mokki>(); // Initialize an empty list to hold the areas

        try
        {
            var dataTable = await dbHelper.GetDataAsync(query); // Execute the query and get the results as a data table

            if (dataTable?.Rows != null) // Check if the DataTable has any rows
            {
                foreach (System.Data.DataRow row in dataTable.Rows) // Loop through each row in the DataTable
                {
                    var mokki = new Mokki
                    {
                        Mokki_id = Convert.ToInt32(row["mokki_id"]),
                        MokkiNimi = row["mokkinimi"].ToString(),
                        AlueID = Convert.ToInt32(row["alue_id"])
                    };
                    mokit.Add(mokki); // Add the created Alue object to the list
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Virhe haettaessa asiakas: " + ex.Message);
        }

        return mokit; // Return the filled list or empty list if something failed
    }

    private void AluePicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        var selectedAlue = AluePicker.SelectedItem as Alue;
        if (selectedAlue != null)
        {
            // Filter mokitLista in memory by selected alue
            var filteredMokit = mokitLista.Where(m => m.AlueID == selectedAlue.AlueId).ToList();
            MokkiPicker.ItemsSource = filteredMokit;
            MokkiPicker.SelectedItem = null;

            // Filter palvelutLista in memory by selected alue
            var filteredPalvelut = palvelutLista.Where(p => p.AlueID == selectedAlue.AlueId).ToList();
            PalvelutPicker.ItemsSource = filteredPalvelut;
            PalvelutPicker.SelectedItem = null;
        }
        else
        {
            MokkiPicker.ItemsSource = null;
            PalvelutPicker.ItemsSource = null;
        }
    }

    private void AsiakasPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        var selectedAsiakas = AsiakasPicker.SelectedItem as Asiakas; // Get the selected Asiakas
        if (selectedAsiakas != null)
        {
            AsiakasPuhelinEntry.Text = selectedAsiakas.puhelin; // Set the phone number in the Entry
        }
    }


    // Search reservation
    private void OnSearchReservation(object sender, EventArgs e)
    {

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

    private async void OnSaveVarausClicked(object sender, EventArgs e)
    {
        // --- VALIDATION ---
        var selectedAsiakas = AsiakasPicker.SelectedItem as Asiakas;
        var selectedAlue = AluePicker.SelectedItem as Alue;
        var selectedMokki = MokkiPicker.SelectedItem as Mokki;
        var selectedPalvelu = PalvelutPicker.SelectedItem as Palvelu;

        if (selectedAsiakas == null)
        {
            await DisplayAlert("Virhe", "Valitse asiakas ennen tallennusta.", "OK");
            return;
        }
        if (selectedAlue == null)
        {
            await DisplayAlert("Virhe", "Valitse alue ennen tallennusta.", "OK");
            return;
        }
        if (selectedMokki == null)
        {
            await DisplayAlert("Virhe", "Valitse mökki ennen tallennusta.", "OK");
            return;
        }
        // You may want to require palvelu, or allow it to be null

        var varattuPvm = VarattupvmDatePicker.Date + VarattupvmTimePicker.Time;
        var alkuPvm = VarattuAlkuPvmDatePicker.Date + VarattuAlkuPvmTimePicker.Time;
        var loppuPvm = VarattuLoppuPvmDatePicker.Date + VarattuLoppuPvmTimePicker.Time;

        if (alkuPvm < varattuPvm)
        {
            await DisplayAlert("Virhe", "Varauksen alku ei voi olla ennen varattua päivää.", "OK");
            return;
        }
        if (loppuPvm <= alkuPvm)
        {
            await DisplayAlert("Virhe", "Varauksen loppu pitää olla varauksen alun jälkeen.", "OK");
            return;
        }

        // --- EDITING ---
        if (_editingVaraus != null)
        {
            // UPDATE VARAUS
            string updateQuery = @"
            UPDATE varaus
            SET asiakas_id = @asiakas_id,
                mokki_id = @mokki_id,
                varattu_pvm = @varattu_pvm,
                varattu_alkupvm = @varattu_alkupvm,
                varattu_loppupvm = @varattu_loppupvm
            WHERE varaus_id = @varaus_id";
            var updateParams = new Dictionary<string, object>
        {
            { "@asiakas_id", selectedAsiakas.asiakasID },
            { "@mokki_id", selectedMokki.Mokki_id },
            { "@varattu_pvm", varattuPvm },
            { "@varattu_alkupvm", alkuPvm },
            { "@varattu_loppupvm", loppuPvm },
            { "@varaus_id", _editingVaraus.varausID }
        };
            int rows = await dbHelper.ExecuteNonQueryAsync(updateQuery, updateParams);

            // --- UPDATE PALVELU ---
            // Remove old palvelut for this varaus
            string deletePalvelutQuery = "DELETE FROM varauksen_palvelut WHERE varaus_id = @varaus_id";
            var delParams = new Dictionary<string, object> { { "@varaus_id", _editingVaraus.varausID } };
            await dbHelper.ExecuteNonQueryAsync(deletePalvelutQuery, delParams);

            // Insert new palvelu if selected
            if (selectedPalvelu != null)
            {
                string insertPalveluQuery = @"INSERT INTO varauksen_palvelut (varaus_id, palvelu_id, lkm)
                                          VALUES (@varaus_id, @palvelu_id, @lkm)";
                var palveluParams = new Dictionary<string, object>
            {
                { "@varaus_id", _editingVaraus.varausID },
                { "@palvelu_id", selectedPalvelu.PalveluID },
                { "@lkm", 1 }
            };
                await dbHelper.ExecuteNonQueryAsync(insertPalveluQuery, palveluParams);
            }

            if (rows > 0)
            {
                // Reload the list so palvelu and costs update
                VarausLista.Clear();
                LoadVaraus();
                await DisplayAlert("Onnistui", "Varaus päivitetty onnistuneesti!", "OK");
                PopupOverlay.IsVisible = false;
                _editingVaraus = null;
            }
            else
            {
                await DisplayAlert("Virhe", "Varauksen päivitys epäonnistui.", "OK");
            }
        }
        else
        {
            // --- ADD NEW VARAUS ---
            string insertQuery = @"
            INSERT INTO varaus (asiakas_id, mokki_id, varattu_pvm, varattu_alkupvm, varattu_loppupvm)
            VALUES (@asiakas_id, @mokki_id, @varattu_pvm, @varattu_alkupvm, @varattu_loppupvm);
            SELECT LAST_INSERT_ID();";
            var insertParams = new Dictionary<string, object>
        {
            { "@asiakas_id", selectedAsiakas.asiakasID },
            { "@mokki_id", selectedMokki.Mokki_id },
            { "@varattu_pvm", varattuPvm },
            { "@varattu_alkupvm", alkuPvm },
            { "@varattu_loppupvm", loppuPvm }
        };

            // Get new varaus_id
            object? result = await dbHelper.ExecuteScalarAsync(insertQuery, insertParams);
            int newVarausId = Convert.ToInt32(result);

            // --- INSERT PALVELU ---
            if (selectedPalvelu != null)
            {
                string insertPalveluQuery = @"INSERT INTO varauksen_palvelut (varaus_id, palvelu_id, lkm)
                                          VALUES (@varaus_id, @palvelu_id, @lkm)";
                var palveluParams = new Dictionary<string, object>
            {
                { "@varaus_id", newVarausId },
                { "@palvelu_id", selectedPalvelu.PalveluID },
                { "@lkm", 1 }
            };
                await dbHelper.ExecuteNonQueryAsync(insertPalveluQuery, palveluParams);
            }

            // Reload the list so palvelu and costs update
            VarausLista.Clear();
            LoadVaraus();
            await DisplayAlert("Onnistui", "Varaus lisätty onnistuneesti!", "OK");
            PopupOverlay.IsVisible = false;
        }
    }

    private void OnCancelPopupClicked(object sender, EventArgs e)
    {
        PopupOverlay.IsVisible = false;
        _editingVaraus = null;
    }

    private void OnAddVarausClicked(object sender, EventArgs e)
    {
        _editingVaraus = null; // Clear the editing varaus

        PopupOverlay.IsVisible = true;

        PopupTitleLabel.Text = "Lisää varaus";

        // Set Varattu pvm to now
        var now = DateTime.Now;
        VarattupvmDatePicker.Date = now.Date;
        VarattupvmTimePicker.Time = now.TimeOfDay;

        VarattuAlkuPvmDatePicker.Date = now.Date;
        VarattuAlkuPvmTimePicker.Time = new TimeSpan(14, 0, 0); 
        VarattuLoppuPvmDatePicker.Date = now.Date.AddDays(1);
        VarattuLoppuPvmTimePicker.Time = new TimeSpan(12, 0, 0);


        // Clear the input fields
        AsiakasPicker.SelectedItem = null;
        AsiakasPuhelinEntry.Text = string.Empty;
        AluePicker.SelectedItem = null;
        MokkiPicker.SelectedItem = null;
        PalvelutPicker.SelectedItem = null;     

    }

    private void OnVarausRowTapped(object sender, TappedEventArgs e)
    {
        // Get the tapped Varaukset object
        var frame = sender as Frame;
        if (frame == null) return;

        var varaus = frame.BindingContext as Varaukset;
        if (varaus == null) return;

        _editingVaraus = varaus;

        // Set popup title for editing
        PopupTitleLabel.Text = "Muokkaa varausta";

        // Populate fields with the booking's data

        // Set customer picker
        AsiakasPicker.SelectedItem = asiakkaatLista.FirstOrDefault(a =>
            a.etunimi == varaus.asiakasVarattu?.etunimi &&
            a.sukunimi == varaus.asiakasVarattu?.sukunimi);

        // Set phone number
        AsiakasPuhelinEntry.Text = varaus.asiakasVarattu?.puhelin ?? string.Empty;

        // Set area picker
        AluePicker.SelectedItem = alueetLista.FirstOrDefault(al => al.AlueNimi == varaus.Alue);

        // Filter and set Mokki picker
        var selectedAlue = AluePicker.SelectedItem as Alue;
        if (selectedAlue != null)
        {
            var filteredMokit = mokitLista.Where(m => m.AlueID == selectedAlue.AlueId).ToList();
            MokkiPicker.ItemsSource = filteredMokit;
            MokkiPicker.SelectedItem = filteredMokit.FirstOrDefault(m => m.MokkiNimi == varaus.mokkiVarattu?.MokkiNimi);
        }
        else
        {
            MokkiPicker.ItemsSource = null;
            MokkiPicker.SelectedItem = null;
        }

        // Filter and set Palvelut picker
        var filteredPalvelut = palvelutLista.Where(p => p.AlueID == selectedAlue?.AlueId).ToList();
        PalvelutPicker.ItemsSource = filteredPalvelut;
        // To preselect palvelu,  need to parse palvelutVarattu string or fetch from DB

        // Set date and time pickers
        
        VarattupvmDatePicker.Date = DateTime.Now.Date;
        VarattupvmTimePicker.Time = DateTime.Now.TimeOfDay;
        VarattuAlkuPvmDatePicker.Date = varaus.varausAlku.HasValue ? varaus.varausAlku.Value.Date : DateTime.Now.Date;
        VarattuAlkuPvmTimePicker.Time = varaus.varausAlku.HasValue ? varaus.varausAlku.Value.TimeOfDay : TimeSpan.Zero;
        VarattuLoppuPvmDatePicker.Date = varaus.varausLoppu.HasValue ? varaus.varausLoppu.Value.Date : DateTime.Now.Date.AddDays(1);
        VarattuLoppuPvmTimePicker.Time = varaus.varausLoppu.HasValue ? varaus.varausLoppu.Value.TimeOfDay : new TimeSpan(12, 0, 0);
      

        // Show popup
        PopupOverlay.IsVisible = true;
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        if (_editingVaraus == null)
        {
            await DisplayAlert("Virhe", "Et voi poistaa varausta, jota ei ole valittu.", "OK");
            return;
        }

        bool isConfirmed = await DisplayAlert(
            "Vahvista poisto",
            $"Oletko varma, että haluat poistaa varauksen {_editingVaraus.varausID}?",
            "Kyllä", "Ei");

        if (!isConfirmed)
            return;

        bool success = await DeleteVarausFromDatabase(_editingVaraus);

        if (success)
        {
            VarausLista.Remove(_editingVaraus);
            _editingVaraus = null;
            PopupOverlay.IsVisible = false;
            await DisplayAlert("Onnistui", "Varaus poistettu onnistuneesti!", "OK");
        }
        else
        {
            await DisplayAlert("Virhe", "Varauksen poistaminen epäonnistui.", "OK");
        }
    }

    private async Task<bool> DeleteVarausFromDatabase(Varaukset varaus)
    {
        try
        {
            // Delete from varauksen_palvelut first (if exists)
            string deletePalvelutQuery = "DELETE FROM varauksen_palvelut WHERE varaus_id = @varaus_id";
            var palvelutParams = new Dictionary<string, object>
        {
            { "@varaus_id", varaus.varausID }
        };
            await dbHelper.ExecuteNonQueryAsync(deletePalvelutQuery, palvelutParams);

            // Then delete the varaus itself
            string deleteVarausQuery = "DELETE FROM varaus WHERE varaus_id = @varaus_id";
            var varausParams = new Dictionary<string, object>
        {
            { "@varaus_id", varaus.varausID }
        };
            int rowsAffected = await dbHelper.ExecuteNonQueryAsync(deleteVarausQuery, varausParams);

            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Virhe poistettaessa varausta: {ex.Message}");
            return false;
        }
    }



}