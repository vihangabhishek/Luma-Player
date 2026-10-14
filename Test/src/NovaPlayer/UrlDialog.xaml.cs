using MessageBox = System.Windows.MessageBox;
using System.Windows;
namespace NovaPlayer;
public partial class UrlDialog : Window
{
    public string? Url { get; private set; }
    public UrlDialog() { InitializeComponent(); Address.SelectAll(); Address.Focus(); }
    private void Open_Click(object sender, RoutedEventArgs e)
    {
        var value = Address.Text.Trim();
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https"))
        { MessageBox.Show(this,"Please enter a valid HTTP or HTTPS address.","Invalid address",MessageBoxButton.OK,MessageBoxImage.Warning);return; }
        Url = value; DialogResult = true;
    }
    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
