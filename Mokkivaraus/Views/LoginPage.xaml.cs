using System.Data;

namespace Mokkivaraus.Views;

public partial class LoginPage : ContentPage
{
    DatabaseHelper dbHelper = new DatabaseHelper();
    //en saa yhdistää tietokantaan..

    public LoginPage()
    {
        InitializeComponent();
        
    }

    
    private async void OnLoginClicked(object sender, EventArgs e)
    {
        string username = UsernameEntry.Text;
        string password = PasswordEntry.Text;

        if (username == "ryhma16" && password == "1234")
        {
            await Navigation.PushAsync(new MainMenuPage());
        }
        else
        {
            await DisplayAlert("Virhe", "Väärä käyttäjätunnus tai salasana.", "OK");
        }
    }

    private async void OnForgotPasswordTapped(object sender, EventArgs e)
    {
        await DisplayAlert("Unohtuiko salasana?", "Ota yhteyttä järjestelmänvalvojaan salasanan palauttamiseksi.", "OK");
    }
}