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
        private string[] keyDict = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
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

            var playlist = spotify.GetPlaylistTracks(profile.Id, "6crTEjprqpnhmF04kjWEWu"); //this ID is hardcoded for testing. Please fix this
            playlist.Items.ForEach(track => tracks.Add(track.Track));

            var analysis = spotify.GetAudioAnalysis(tracks[0].Id);
            MessageBox.Show(tracks[0].Name);
            //analysis.Track.ForEach(single => Console.WriteLine(single.Tempo));
            //analysis.Track.ForEach(single => Console.WriteLine(single.Key));

            //MessageBox.Show(analysis.Track.Key.ToString());
            //MessageBox.Show(analysis.Track.Tempo.ToString());
            //MessageBox.Show(analysis.Track.Mode.ToString());

            var workbook = new XSSFWorkbook();
            var sheet = workbook.CreateSheet("Sheet1");

            sheet.CreateRow(0).CreateCell(0).SetCellValue("This Is A Test");

            int x = 1;

            for (int i = 1; i <= 15; i++) //row
            {
                IRow row = sheet.CreateRow(i);

                for (int j = 0; j < 15; j++) //column
                {
                    row.CreateCell(j).SetCellValue(x++);
                }
            }

            var sw = File.Create("test.xlsx");

            workbook.Write(sw);
            sw.Close();
        }
    }
}
