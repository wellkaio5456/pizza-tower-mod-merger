using System.Windows;
using SugarySpireGMLoader.Services;

namespace SugarySpireGMLoader.Gui;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        GameFolderBox.Text = ".";
        ModsFolderBox.Text = System.IO.Path.Combine(".", "mods");

        DetectButton.Click += DetectButton_Click;
        InstallButton.Click += InstallButton_Click;
        LaunchButton.Click += LaunchButton_Click;
        BackupButton.Click += BackupButton_Click;
        BrowseButton.Click += BrowseButton_Click;
        OpenModsButton.Click += OpenModsButton_Click;
        CloseButton.Click += (_, _) => Close();
    }

    private void DetectButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var profile = StartupChecks.ValidateGameFolder(GameFolderBox.Text);
            StatusText.Text = $"Detected: {profile.Name}\nExecutable: {profile.ExecutableName}\nData file: {profile.DataFileName}";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Error: {ex.Message}";
        }
    }

    private void BackupButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var profile = StartupChecks.ValidateGameFolder(GameFolderBox.Text);
            BackupManager.CreateBackup(GameFolderBox.Text, profile.DataFileName, profile.BackupFileName);
            StatusText.Text = $"Backup created for {profile.DataFileName}";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Error: {ex.Message}";
        }
    }

    private void InstallButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var gameDir = GameFolderBox.Text;
            var modDir = ModsFolderBox.Text;

            if (!System.IO.Directory.Exists(modDir))
            {
                StatusText.Text = $"Mod directory not found: {modDir}";
                return;
            }

            StartupChecks.ValidateGameFolder(gameDir);
            ModInstaller.Install(gameDir, modDir);

            StatusText.Text = $"Installed mod from: {modDir}";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Error: {ex.Message}";
        }
    }

    private void LaunchButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var profile = StartupChecks.ValidateGameFolder(GameFolderBox.Text);
            Launcher.LaunchExecutable(GameFolderBox.Text, profile.ExecutableName);
            StatusText.Text = $"Launching {profile.ExecutableName}";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Error: {ex.Message}";
        }
    }

    private void BrowseButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new System.Windows.Forms.FolderBrowserDialog();
        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            GameFolderBox.Text = dialog.SelectedPath;
        }
    }

    private void OpenModsButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new System.Windows.Forms.FolderBrowserDialog();
        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            ModsFolderBox.Text = dialog.SelectedPath;
        }
    }
}
