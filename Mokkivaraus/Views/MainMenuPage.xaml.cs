using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Mokkivaraus.Views;

public partial class MainMenuPage : ContentPage, INotifyPropertyChanged
{
    DatabaseHelper dbHelper = new DatabaseHelper();

    public MainMenuPage()
	{
		InitializeComponent();       
        LoadDashboardData();
        BindingContext = this; 

    }   
    protected override  void OnAppearing()
    {
        base.OnAppearing(); // Ensure the page is fully loaded before executing any code
        NavigationPage.SetHasBackButton(this, false); // Hide back button        
    }

    // Dashboard properties
    private int _totalCabins; // Total number of cabins
    public int TotalCabins 
    {
        get => _totalCabins;
        set { _totalCabins = value; OnPropertyChanged(); }
    }

    private int _activeReservations; // Total number of active reservations
    public int ActiveReservations
    {
        get => _activeReservations;
        set { _activeReservations = value; OnPropertyChanged(); }
    }

    private int _totalInvoices; // Total number of invoices
    public int TotalInvoices
    {
        get => _totalInvoices;
        set { _totalInvoices = value; OnPropertyChanged(); }
    }

    private int _openInvoices; // Total number of open invoices
    public int OpenInvoices
    {
        get => _openInvoices;
        set { _openInvoices = value; OnPropertyChanged(); }
    }

    private int _paidInvoices; // Total number of paid invoices
    public int PaidInvoices
    {
        get => _paidInvoices;
        set { _paidInvoices = value; OnPropertyChanged(); }
    }

    private async void LoadDashboardData() // Load data for the dashboard
    {
        TotalCabins = await GetTotalCabins();
        ActiveReservations = await GetActiveReservations();
        TotalInvoices = await GetTotalInvoices();
        OpenInvoices = await GetOpenInvoices();
        PaidInvoices = await GetPaidInvoices();
    }

    private async Task<int> GetTotalCabins() // Get total number of cabins
    {
        string query = "SELECT COUNT(*) FROM mokki";
        var dt = await dbHelper.GetDataAsync(query);
        if (dt.Rows.Count > 0)
            return Convert.ToInt32(dt.Rows[0][0]);
        return 0;
    }

    private async Task<int> GetActiveReservations() // Get total number of active reservations
    {
        string query = "SELECT COUNT(*) FROM varaus WHERE varattu_alkupvm <= CURDATE() AND varattu_loppupvm >= CURDATE() ";
        var dt = await dbHelper.GetDataAsync(query);
        if (dt.Rows.Count > 0)
            return Convert.ToInt32(dt.Rows[0][0]);
        return 0;
    }

    private async Task<int> GetTotalInvoices() // Get total number of invoices
    {
        string query = "SELECT COUNT(*) FROM laskut";
        var dt = await dbHelper.GetDataAsync(query);
        if (dt.Rows.Count > 0)
            return Convert.ToInt32(dt.Rows[0][0]);
        return 0;
    }

    private async Task<int> GetOpenInvoices() // Get total number of open invoices
    {
        string query = "SELECT COUNT(*) FROM laskut WHERE tila = 'avoin'";
        var dt = await dbHelper.GetDataAsync(query);
        if (dt.Rows.Count > 0)
            return Convert.ToInt32(dt.Rows[0][0]);
        return 0;
    }

    private async Task<int> GetPaidInvoices() // Get total number of paid invoices
    {
        string query = "SELECT COUNT(*) FROM laskut WHERE tila = 'maksettu'";
        var dt = await dbHelper.GetDataAsync(query);
        if (dt.Rows.Count > 0)
            return Convert.ToInt32(dt.Rows[0][0]);
        return 0;
    }

    public new event PropertyChangedEventHandler? PropertyChanged;
    protected new void OnPropertyChanged([CallerMemberName] string? propertyName = null) // Notify property change
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); 
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

    private async void OnPalvelutClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new PalvelutPage());
    }

    private async void OnVarauksetClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Views.VarauksetViewPage());
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
   
}