using MySql.Data.MySqlClient;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Mokkivaraus.Models;
using System.Threading.Tasks;

namespace Mokkivaraus.Views;

public partial class VarauksetViewPage : ContentPage
{
    public ObservableCollection<Varaukset> VarausLista = new ObservableCollection<Varaukset>();
    DatabaseHelper dbHelper = new DatabaseHelper();
    public async Task<ObservableCollection<Varaukset>> GetVarausAsync()
    {
        const string GetVarausQuery = "select a.etunimi,a.sukunimi,a.puhelinnro,v.varattu_pvm,al.nimi,m.mokkinimi,p.nimi as palvelunimi,date(v.varattu_alkupvm) as alku,date(v.varattu_loppupvm)as loppu from vn.varaus v join vn.asiakas a on v.asiakas_id=a.asiakas_id join vn.mokki m on v.mokki_id=m.mokki_id join vn.alue al on v.mokki_id=al.alue_id join vn.palvelu p on al.alue_id=p.alue_id";

        var varaukset = new ObservableCollection<Varaukset>();

        try
        {
            var dataTable = await dbHelper.GetDataAsync(GetVarausQuery);
            if (dataTable.Rows != null)
            {
                foreach (System.Data.DataRow row in dataTable.Rows)
                {
                    var varaus = new Varaukset
                    {
                        asiakasVarattu= new Asiakas{etunimi=row["etunimi"].ToString(),sukunimi=row["sukunimi"].ToString(),puhelin=row["puhelinnro"].ToString()},
                        varausPaiva=Convert.ToDateTime(row["varattu_pvm"]),
                        mokkiVarattu=new Mokki{MokkiNimi=row["mokkinimi"].ToString()},
                        Alue=row["nimi"].ToString(),
                        palvelutVarattu=row["palvelunimi"].ToString(),
                        varausAlku=Convert.ToDateTime(row["alku"]),
                        varausLoppu=Convert.ToDateTime(row["loppu"])
                        
                    };
                    varaukset.Add(varaus);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Virhe haettaessa varauksia: {ex.Message}");
            return new ObservableCollection<Varaukset>(); // Return an empty collection to avoid null issues
        }
        return varaukset;
    }


    public VarauksetViewPage()
	{
		InitializeComponent();
        VarausListaView.ItemsSource = VarausLista;
        LoadVaraus();
	}

    

    
    private async void LoadVaraus()  // Load and add customers to collection
    {
        var varausFromDb = await GetVarausAsync(); // Fetch customers
        if (varausFromDb != null) // 
        {
            foreach (var varaus in varausFromDb)
            {
                VarausLista.Add(varaus); // Add each customer to the list
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

    
}