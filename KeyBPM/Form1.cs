using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KeyBPM
{
    public partial class Form1 : Form
    {
        private SpotifyWebAPI spotify;
        private PrivateProfile profile;
        private OpenFileDialog ofd = new OpenFileDialog();
        private string[] keyDict = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" }; //these are all wrong for some reason
        //private string filePath = "";

        public Form1()
        {
            InitializeComponent();
        }

        private void authButton_Click(object sender, EventArgs e)
        {
           Task.Run(() => RunAuthentication());
        }

        private async void RunAuthentication()
        {
            WebAPIFactory webApiFactory = new WebAPIFactory(
                "http://localhost", 8000, "78c190180d5e4e79baf28a7ad4c04018",
                Scope.UserReadPrivate | Scope.UserReadEmail | Scope.PlaylistReadPrivate | Scope.UserLibraryRead |
                Scope.PlaylistModifyPublic | Scope.UserFollowRead | Scope.UserReadBirthdate | Scope.UserTopRead | Scope.PlaylistReadCollaborative |
                Scope.UserReadRecentlyPlayed | Scope.UserReadPlaybackState | Scope.UserModifyPlaybackState);

            try
            {
                spotify = await webApiFactory.GetWebApi();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            if (spotify == null)
                return;

            InitialSetup();
        }

        private async void InitialSetup()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(InitialSetup));
                return;
            }

            authButton.Enabled = false;
            profile = await spotify.GetPrivateProfileAsync();
            nameLabel.Text = profile.DisplayName;
            countryLabel.Text = profile.Country;
            emailLabel.Text = profile.Email;
            accountLabel.Text = profile.Product;

            if (profile.Images != null && profile.Images.Count > 0)
            {
                using (WebClient wc = new WebClient())
                {
                    byte[] imageBytes = await wc.DownloadDataTaskAsync(new Uri(profile.Images[0].Url));
                    using (MemoryStream stream = new MemoryStream(imageBytes))
                        pictureBox.Image = System.Drawing.Image.FromStream(stream);
                }
            }
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            SearchItem song = new SearchItem();
            ErrorResponse response = new ErrorResponse();
            List<FullTrack> tracks = new List<FullTrack>();
            XSSFWorkbook workbook = new XSSFWorkbook();

            MessageBox.Show("Starting");

            System.Windows.Forms.Cursor.Current = Cursors.WaitCursor;
            

            var playlist = spotify.GetPlaylistTracks(profile.Id, "6crTEjprqpnhmF04kjWEWu"); // Hardcoded for testing. Needs graceful update
            playlist.Items.ForEach(track => tracks.Add(track.Track));

            var sheet = workbook.CreateSheet("Sheet1");
            sheet.CreateRow(0).CreateCell(0).SetCellValue("This Is A Test");

            for (int i = 0; i < tracks.Count; i++) //row
            {
                var analysis = spotify.GetAudioAnalysis(tracks[i].Id);
                var row = sheet.CreateRow(i);
                string mod = "";

                if (analysis.Track.Mode == 1)
                    mod = "maj";
                else
                    mod = "min";

                row.CreateCell(0).SetCellValue(tracks[i].Name);
                row.CreateCell(1).SetCellValue(Math.Round(analysis.Track.Tempo));
                row.CreateCell(2).SetCellValue(keyDict[analysis.Track.Key] + " " + mod);
         
            }

            var sw = File.Create("test.xlsx");

            workbook.Write(sw);
            sw.Close();
            System.Windows.Forms.Cursor.Current = Cursors.Default;
            MessageBox.Show("Finished");
        }

        private void exitButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
