namespace Mokkivaraus.Views;
using Mokkivaraus.Models;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using Microsoft.Maui.Storage;
using System.Collections.ObjectModel;

public partial class LaskutPage : ContentPage
{
    private ObservableCollection<Lasku> Laskut = new ObservableCollection<Lasku>();
    private ObservableCollection<Lasku> LaskutLista; //Esimerkki list//
    public LaskutPage()
	{
		InitializeComponent();


        // Esimerkiksi Tiedot (Laskut) -luokka
        LaskutLista = new ObservableCollection<Lasku>
        {
            new Lasku { LaskuNumero = "001", Asiakas = "Matti Meikäläinen", Summa = "150 €", Tuote = "Mökkivuokra", Tila = "Maksettu", Paivamaara = "01.06.2025" },
            new Lasku { LaskuNumero = "002", Asiakas = "Anna Esimerkki", Summa = "200 €", Tuote = "Saunavuokra", Tila = "Avoin", Paivamaara = "03.06.2025" }
        };

        LaskutCollectionView.ItemsSource = LaskutLista;
        //esimerkki////
    }

    //popup////////////////////////////
    private void OnAddLaskuClicked(object sender, EventArgs e)
    {
        LaskuPopupTitle.Text = "Lisää uusi lasku";
        ClearLaskuPopupFields();

        PoistaLaskuButton.IsVisible = false;
        PdfLaskuButton.IsVisible = false;
        LaskuPopupOverlay.IsVisible = true;
    }

    private void ClearLaskuPopupFields()
    {
        LaskuNumeroEntry.Text = "";
        AsiakasEntry.Text = "";
        SummaEntry.Text = "";
        TuoteEntry.Text = "";
        TilaEntry.Text = "";
        PaivamaaraEntry.Text = "";
    }

    private void OnCancelLaskuPopupClicked(object sender, EventArgs e)
    {
        LaskuPopupOverlay.IsVisible = false;
    }


    private void OnLaskuRowTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is Lasku valittu)
        {
            LaskuPopupTitle.Text = "Muokkaa laskua";

            
            LaskuNumeroEntry.Text = valittu.LaskuNumero;
            AsiakasEntry.Text = valittu.Asiakas;
            SummaEntry.Text = valittu.Summa;
            TuoteEntry.Text = valittu.Tuote;
            TilaEntry.Text = valittu.Tila;
            PaivamaaraEntry.Text = valittu.Paivamaara;
            
            //näytä buttonit
            PoistaLaskuButton.IsVisible = true;
            PdfLaskuButton.IsVisible = true;
            PdfLaskuButton.CommandParameter = valittu;

            LaskuPopupOverlay.IsVisible = true;
        }
    }


    private void OnSaveLaskuClicked(object sender, EventArgs e)
    {
        string laskuNumero = LaskuNumeroEntry.Text;
        var olemassaOleva = Laskut.FirstOrDefault(l => l.LaskuNumero == laskuNumero);

        if (olemassaOleva != null)
        {
            // Päivitä olemassa oleva lasku
            olemassaOleva.Asiakas = AsiakasEntry.Text;
            olemassaOleva.Summa = SummaEntry.Text;
            olemassaOleva.Tuote = TuoteEntry.Text;
            olemassaOleva.Tila = TilaEntry.Text;
            olemassaOleva.Paivamaara = PaivamaaraEntry.Text;
        }
        else
        {
            // Lisää uusi lasku
            var uusi = new Lasku
            {
                LaskuNumero = LaskuNumeroEntry.Text,
                Asiakas = AsiakasEntry.Text,
                Summa = SummaEntry.Text,
                Tuote = TuoteEntry.Text,
                Tila = TilaEntry.Text,
                Paivamaara = PaivamaaraEntry.Text
            };
            Laskut.Add(uusi);
        }

        LaskuPopupOverlay.IsVisible = false;
        ClearLaskuPopupFields();
    }


    private void OnPoistaLaskuClicked(object sender, EventArgs e)
    {
        var lasku = PdfLaskuButton.CommandParameter as Lasku;

        if (lasku != null && Laskut.Contains(lasku))
        {
            Laskut.Remove(lasku);
        }

        LaskuPopupOverlay.IsVisible = false;
        ClearLaskuPopupFields();
    }
    //popup///////






    ////PDF asetukset/////

    private async Task GenerateInvoicePdfAsync(Lasku lasku)
    {

        var document = new PdfDocument();
        var page = document.AddPage();
        var gfx = XGraphics.FromPdfPage(page);

        var font = new XFont("Verdana", 12);
        var boldFont = new XFont("Verdana", 14, XFontStyle.Bold);
        var headerFont = new XFont("Verdana", 24, XFontStyle.Bold);
        var totalfont = new XFont("Verdana", 10);
        var totalboldFont = new XFont("Verdana", 10, XFontStyle.Bold);

        double y = 50;

        
        gfx.DrawString("LASKU", headerFont, XBrushes.Black, new XRect(0, y, page.Width, 0), XStringFormats.TopCenter);
        y += 40;

        gfx.DrawLine(XPens.DarkGray, 40, y, page.Width - 40, y);
        y += 20;

        gfx.DrawString("Village Newbies Oy", boldFont, XBrushes.Black, new XRect(40, y, page.Width, 0), XStringFormats.TopLeft);
        y += 20;
        gfx.DrawString("Siilokatu 1", font, XBrushes.Black, new XRect(40, y, page.Width, 0), XStringFormats.TopLeft);
        y += 20;
        gfx.DrawString("90700 Oulu", font, XBrushes.Black, new XRect(40, y, page.Width, 0), XStringFormats.TopLeft);
        y += 50;

        //  Lasku tiedot
        gfx.DrawString($"Laskunro: {lasku.LaskuNumero}", font, XBrushes.Black, new XRect(40, y, page.Width, 0), XStringFormats.TopLeft); y += 25;
        gfx.DrawString($"Asiakas: {lasku.Asiakas}", font, XBrushes.Black, new XRect(40, y, page.Width, 0), XStringFormats.TopLeft); y += 25;
        gfx.DrawString($"Tila: {lasku.Tila}", font, XBrushes.Black, new XRect(40, y, page.Width, 0), XStringFormats.TopLeft);

        // Taulu
        y += 60;
        
       

        
        gfx.DrawString("Kuvaus", boldFont, XBrushes.Black, new XRect(50, y, 200, 0), XStringFormats.TopLeft);
        gfx.DrawString("Yhteensä", boldFont, XBrushes.Black, new XRect(page.Width - 150, y, 100, 0), XStringFormats.TopLeft);
        y += 15;

        
        gfx.DrawLine(XPens.Black, 40, y, page.Width - 40, y);
        y += 15;

        
        gfx.DrawString(lasku.Tuote, font, XBrushes.Black, new XRect(50, y, 200, 0), XStringFormats.TopLeft);
        gfx.DrawString(lasku.Summa, font, XBrushes.Black, new XRect(page.Width - 150, y, 100, 0), XStringFormats.TopLeft);
        y += 15;

        // ALV laske

        y += 30;

        //box asetukset
        var boxBrush = new XSolidBrush();
        double boxX = page.Width - 300;
        double boxWidth = 230;
        double lineHeight = 25;

        // background
        gfx.DrawRectangle(boxBrush, boxX, y, boxWidth, lineHeight * 3);

        // Vero 
        double netto = double.Parse(lasku.Summa.Replace("€", "").Trim());
        double vat = Math.Round(netto * 0.24, 2);
        double total = Math.Round(netto + vat, 2);

        
        gfx.DrawString("Hinta ilman ALV 24%:", totalfont, XBrushes.Black, new XRect(boxX + 10, y + 5, 200, 0), XStringFormats.TopLeft);
        gfx.DrawString($"{netto} €", font, XBrushes.Black, new XRect(boxX + 155, y + 5, 80, 0), XStringFormats.TopLeft);
        y += lineHeight;

        gfx.DrawString("ALV 24%:", totalfont, XBrushes.Black, new XRect(boxX + 10, y + 5, 200, 0), XStringFormats.TopLeft);
        gfx.DrawString($"{vat} €", font, XBrushes.Black, new XRect(boxX + 155, y + 5, 80, 0), XStringFormats.TopLeft);
        y += lineHeight;

        gfx.DrawString("Maksettava yhteensä:", totalboldFont, XBrushes.Black, new XRect(boxX + 10, y + 5, 200, 0), XStringFormats.TopLeft);
        gfx.DrawString($"{total} €", boldFont, XBrushes.Black, new XRect(boxX + 155, y + 5, 80, 0), XStringFormats.TopLeft);


        // Päivämäärä 
        gfx.DrawString($"Päivämäärä: {lasku.Paivamaara}", font, XBrushes.Black, new XRect(0, 120, page.Width - 40, 0), XStringFormats.TopRight);

        


        // Mihin tallennetaan
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        string fileName = $"Lasku_{lasku.LaskuNumero}.pdf";
        string path = Path.Combine(desktopPath, fileName);
        
        document.Save(path);
        

        await DisplayAlert("PDF Tallennettu", $"Tiedosto tallennettu:\n{path}", "OK");
    }


    //PDF buton
    private async void OnDownloadPdfClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;
        var lasku = (Lasku)button.CommandParameter;

        await GenerateInvoicePdfAsync(lasku);
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
        //Navigoi Asiakkaat-sivulle
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
}