namespace Mokkivaraus.Views;
using Mokkivaraus.Models;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using Microsoft.Maui.Storage;
using System.Collections.ObjectModel;

public partial class LaskutPage : ContentPage
{
    DatabaseHelper dbHelper = new DatabaseHelper();
    private ObservableCollection<Lasku> LaskutLista = new ObservableCollection<Lasku>();
    public LaskutPage()
	{
		InitializeComponent();


        LoadLaskutFromDb();
    }


    private async void LoadLaskutFromDb()
    {
        LaskutLista = await GetLaskutFromDatabaseAsync();
        LaskutCollectionView.ItemsSource = LaskutLista;
    }

    public async Task<ObservableCollection<Lasku>> GetLaskutFromDatabaseAsync()
    {
        const string query = "SELECT lasku_id, laskunumero, asiakas, summa, tuote, tila, paivamaara FROM laskut ORDER BY CAST(laskunumero AS UNSIGNED)";

        var laskut = new ObservableCollection<Lasku>();

        try
        {
            var table = await dbHelper.GetDataAsync(query);

            foreach (System.Data.DataRow row in table.Rows)
            {
                string paivamaara = "-";
                if (DateTime.TryParse(row["paivamaara"]?.ToString(), out var parsedDate) &&
                    parsedDate != DateTime.MinValue &&
                    parsedDate.Year > 1900)
                {
                    paivamaara = parsedDate.ToString("dd.MM.yyyy");
                }
                var lasku = new Lasku
                {
                    LaskuNumero = row["laskunumero"]?.ToString() ?? string.Empty,
                    Asiakas = row["asiakas"]?.ToString() ?? string.Empty,
                    Summa = (row["summa"]?.ToString() ?? string.Empty) + " €",
                    Tuote = row["tuote"]?.ToString() ?? string.Empty,
                    Tila = row["tila"]?.ToString() ?? string.Empty,
                    Paivamaara = paivamaara
                };
                laskut.Add(lasku);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Virhe haettaessa laskuja: " + ex.Message);
        }

        return laskut;
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
        PaivamaaraPicker.Date = DateTime.Now;
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
            if (DateTime.TryParse(valittu.Paivamaara, out var parsedDate))
            {
                PaivamaaraPicker.Date = parsedDate;
            }

            //näytä buttonit
            PoistaLaskuButton.IsVisible = true;
            PdfLaskuButton.IsVisible = true;
            PdfLaskuButton.CommandParameter = valittu;

            LaskuPopupOverlay.IsVisible = true;
        }
    }


    private async void OnSaveLaskuClicked(object sender, EventArgs e)
    {
        string laskuNumero = LaskuNumeroEntry.Text;

        var uusi = new Lasku
        {
            LaskuNumero = laskuNumero,
            Asiakas = AsiakasEntry.Text,
            Summa = SummaEntry.Text,
            Tuote = TuoteEntry.Text,
            Tila = TilaEntry.Text,
            Paivamaara = PaivamaaraPicker.Date.ToString("yyyy-MM-dd")
        };

        try
        {
            string tarkistaQuery = "SELECT COUNT(*) FROM laskut WHERE laskunumero = @laskunumero";
            var checkParams = new Dictionary<string, object>
        {
            { "@laskunumero", laskuNumero }
        };

            var countResult = await dbHelper.ExecuteScalarAsync(tarkistaQuery, checkParams);
            bool onJoOlemassa = Convert.ToInt32(countResult) > 0;

            
            string sqlQuery;
            var parameters = new Dictionary<string, object>
        {
            { "@laskunumero", uusi.LaskuNumero },
            { "@asiakas", uusi.Asiakas },
            { "@summa", uusi.Summa.Replace("€", "").Trim() },
            { "@tuote", uusi.Tuote },
            { "@tila", uusi.Tila },
            { "@paivamaara", uusi.Paivamaara }
        };

            
            if (onJoOlemassa)
            {
                sqlQuery = @"UPDATE laskut SET asiakas = @asiakas, summa = @summa, tuote = @tuote, tila = @tila, paivamaara = @paivamaara 
                         WHERE laskunumero = @laskunumero";
            }
            else
            {
                sqlQuery = @"INSERT INTO laskut (laskunumero, asiakas, summa, tuote, tila, paivamaara) 
                         VALUES (@laskunumero, @asiakas, @summa, @tuote, @tila, @paivamaara)";
            }

            
            int result = await dbHelper.ExecuteNonQueryAsync(sqlQuery, parameters);

            if (result > 0)
            {
                await DisplayAlert("Onnistui", onJoOlemassa ? "Lasku päivitetty!" : "Lasku lisätty!", "OK");
                LoadLaskutFromDb(); // päivitä
            }
            else
            {
                await DisplayAlert("Virhe", "Tietokantatoiminto epäonnistui.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Virhe", "Poikkeus: " + ex.Message, "OK");
        }

        LaskuPopupOverlay.IsVisible = false;
        ClearLaskuPopupFields();
    }


    private async void OnPoistaLaskuClicked(object sender, EventArgs e)
    {
        var lasku = PdfLaskuButton.CommandParameter as Lasku;

        if (lasku != null && !string.IsNullOrEmpty(lasku.LaskuNumero)) // Ensure LaskuNumero is not null
        {
            bool vastaus = await DisplayAlert("Vahvistus", "Haluatko varmasti poistaa tämän laskun?", "Kyllä", "Peruuta");
            if (!vastaus) return;

            // poistaa db:Sta
            string deleteQuery = "DELETE FROM laskut WHERE laskunumero = @laskunumero";
            var parameters = new Dictionary<string, object>
            {
                { "@laskunumero", lasku.LaskuNumero } // LaskuNumero is guaranteed to be non-null here
            };

            try
            {
                int result = await dbHelper.ExecuteNonQueryAsync(deleteQuery, parameters);
                if (result > 0)
                {
                    await DisplayAlert("Onnistui", "Lasku poistettu!", "OK");
                    LoadLaskutFromDb();
                }
                else
                {
                    await DisplayAlert("Virhe", "Laskun poistaminen epäonnistui.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Virhe", "Poikkeus: " + ex.Message, "OK");
            }

            LaskuPopupOverlay.IsVisible = false;
            ClearLaskuPopupFields();
        }
        else
        {
            await DisplayAlert("Virhe", "LaskuNumero on tyhjä tai null.", "OK");
        }
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

    
}