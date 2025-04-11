namespace Mokkivaraus.Views;
using System.Collections.ObjectModel;


public partial class MokitPage : ContentPage
{
    private ObservableCollection<Mokki> mokkiLista = new ObservableCollection<Mokki>();

    public MokitPage()
	{
		InitializeComponent();
        MokkiListaView.ItemsSource = mokkiLista;
    }

    //MENU - sidebar////////////////////

    private async void OnMokitClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new MokitPage());
    }

    private void OnAlueetClicked(object sender, EventArgs e)
    {
        // TODO: Navigoi Alueet-sivulle
    }

    private void OnPalvelutClicked(object sender, EventArgs e)
    {
        // TODO: Navigoi Palvelut-sivulle
    }

    private void OnVarauksetClicked(object sender, EventArgs e)
    {
        // TODO: Navigoi Varaukset-sivulle
    }

    private void OnAsiakkaatClicked(object sender, EventArgs e)
    {
        // TODO: Navigoi Asiakkaat-sivulle
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

    private void OnSaveCabinClicked(object sender, EventArgs e)
    {
        var yeniMokki = new Mokki
        {
            Nimi = NimiEntry.Text,
            Sijainti = SijaintiEntry.Text,
            Hinta = decimal.TryParse(HintaEntry.Text, out decimal hinta) ? hinta : 0,
            Kapasiteetti = int.TryParse(KapasiteettiEntry.Text, out int kapasiteetti) ? kapasiteetti : 0,
            Alue = AlueEntry.Text
        };

        mokkiLista.Add(yeniMokki);
        PopupOverlay.IsVisible = false;

        
        NimiEntry.Text = "";
        SijaintiEntry.Text = "";
        HintaEntry.Text = "";
        KapasiteettiEntry.Text = "";
        AlueEntry.Text = "";
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

}