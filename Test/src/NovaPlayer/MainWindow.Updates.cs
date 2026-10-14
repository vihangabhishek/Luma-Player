using System;
using MessageBox = System.Windows.MessageBox;
using System.ComponentModel;
using System.Windows;

namespace NovaPlayer;

public partial class MainWindow
{
    private PlayerUpdate? _availableUpdate;
    private bool _updating;

    private async void CheckUpdates_Click(object sender, RoutedEventArgs e)
    {
        if (_updating) return;
        _updating = true;
        CheckUpdatesButton.IsEnabled = false;
        InstallUpdateButton.Visibility = Visibility.Collapsed;
        UpdateStatus.Text = "Checking official GitHub releases…";
        try
        {
            _availableUpdate = await PlayerUpdater.CheckAsync();
            if (_availableUpdate == null)
            {
                UpdateStatus.Text = "You're up to date. No newer verified releases.";
            }
            else
            {
                UpdateStatus.Text = "Update available: v" + _availableUpdate.Version;
                InstallUpdateButton.Content = "↓ Download & install v" + _availableUpdate.Version;
                InstallUpdateButton.Visibility = Visibility.Visible;
            }
        }
        catch (Exception ex)
        {
            UpdateStatus.Text = "Could not check updates. Are you connected to the internet?";
            StartupDiagnostics.Write("Update check failed.", ex);
        }
        finally { CheckUpdatesButton.IsEnabled = true; _updating = false; }
    }

    private async void InstallUpdate_Click(object sender, RoutedEventArgs e)
    {
        if (_updating || _availableUpdate is not { } update) return;
        if (MessageBox.Show(this,
            "Download Nova Player v" + update.Version + " from the official GitHub release?\n\n" +
            "The file's SHA-256 hash will be verified before opening the normal Windows installer.\n\n" +
            "Note: This is an unsigned preview and Windows SmartScreen may display a warning. Never disable Defender.",
            "Update Nova Player", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;

        _updating = true;
        InstallUpdateButton.IsEnabled = false;
        CheckUpdatesButton.IsEnabled = false;
        UpdateProgress.Value = 0;
        UpdateProgress.Visibility = Visibility.Visible;
        UpdateStatus.Text = "Downloading update…";
        try
        {
            var progress = new Progress<double>(percent =>
            {
                UpdateProgress.Value = percent;
                UpdateStatus.Text = $"Downloading verified update… {percent:0}%";
            });
            string file = await PlayerUpdater.DownloadVerifiedAsync(update, progress);
            UpdateStatus.Text = "Download verified. Ready for installation.";
            if (MessageBox.Show(this,
                "Verified update downloaded. Open the setup wizard and close Nova Player?\n" +
                "The installer will guide you through the update.",
                "Install update", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                PlayerUpdater.LaunchInstaller(file);
                Close();
                return;
            }
        }
        catch (Exception ex)
        {
            StartupDiagnostics.Write("In-app update failed.", ex);
            UpdateStatus.Text = "Update failed. Please use GitHub Releases instead.";
            MessageBox.Show(this, "The update was NOT installed.\n\n" + ex.Message,
                "Update error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            _updating = false;
            UpdateProgress.Visibility = Visibility.Collapsed;
            InstallUpdateButton.IsEnabled = true;
            CheckUpdatesButton.IsEnabled = true;
        }
    }
}
