using Asm.Emtity;
using Asm.Sevices;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Asm.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class music : Page
    {
        private bool is_play = false;
        private int _currentIndex = 0;
        
        private ObservableCollection<Song> listSong;
        private ObservableCollection<Song> listSong2;
        internal ObservableCollection<Song> ListSong { get => listSong; set => listSong = value; }
        internal ObservableCollection<Song> ListSong2 { get => listSong2; set => listSong2 = value; }
        public MediaElement PlayMusic { get; private set; }

        MediaPlaybackList playbackList = new MediaPlaybackList();

        Song ss = new Song();
        
        public music()
        {
            CheckCredential();
            GetToken();
            
            this.ListSong = new ObservableCollection<Song>();
            bth_getsong();
            this.InitializeComponent();
            
            //load_song();
        }
        public async static Task<string> CheckCredential()
        {
            string text;
            StorageFolder localfolder = ApplicationData.Current.LocalFolder;
            if (await localfolder.TryGetItemAsync("token.txt") != null)
            {
                Windows.Storage.StorageFolder storageFolder =
                    Windows.Storage.ApplicationData.Current.LocalFolder;
                Windows.Storage.StorageFile sampleFile =
                    await storageFolder.GetFileAsync("token.txt");
                text = await Windows.Storage.FileIO.ReadTextAsync(sampleFile);
            }
            else
            {
                text = "";
            }
            return text;
        }
        public async Task<string> GetToken()
        {
            Windows.Storage.StorageFolder storageFolder =
                Windows.Storage.ApplicationData.Current.LocalFolder;
            Windows.Storage.StorageFile sampleFile = await storageFolder.GetFileAsync("token.txt");
            string text = await Windows.Storage.FileIO.ReadTextAsync(sampleFile);
            Credential credential = JsonConvert.DeserializeObject<Credential>(text);
            string token = credential.token;
            Debug.WriteLine(token);
            return token;
        }
        private async void bth_create(object sender, RoutedEventArgs e)
        {
            string text = await CheckCredential();
            if (text != "")
            {
                string token = await GetToken();

                if (token != "")
                {
                    ss.name = this.name.Text;
                    ss.singer = this.singer.Text;
                    ss.author = this.author.Text;
                    ss.thumbnail = this.thumbnail.Text;
                    ss.link = this.link.Text;
                    HttpClient httpClient = new HttpClient();
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);
                    var content = new StringContent(JsonConvert.SerializeObject(ss), System.Text.Encoding.UTF8, "application/json");
                    var response = httpClient.PostAsync(URL.API_CREATESONG,content);
                    var contents = await response.Result.Content.ReadAsStringAsync();
                   Debug.WriteLine(contents);

                   
                }
            }
            else
            {
               

                ContentDialog noWifiDialog = new ContentDialog
                {
                    Title = "warning",
                    Content = "Bạn cần phải đăng nhập để tạo bài hát",
                    CloseButtonText = "Ok"
                };

                ContentDialogResult result = await noWifiDialog.ShowAsync();
            }
            Debug.WriteLine(text);

        }

        private async void bth_getsong()
        {
            string text = await CheckCredential();
            if (text != "")
            {
                string token = await GetToken();

                if (token != "")
                {

                    HttpClient httpClient = new HttpClient();
                    
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);
                    var response = httpClient.GetAsync(URL.API_GETMYSONG1);
                    var content = await response.Result.Content.ReadAsStringAsync();
                    var ssss = JsonConvert.DeserializeObject<ObservableCollection<Song>>(content);
                    Debug.WriteLine(content);

                    foreach (var aaa in ssss)
                    {
                        ListSong.Insert(0,aaa);
                    }
                }
            }
        }

        private void tap_tap(object sender, TappedRoutedEventArgs e)
        {
            var stack = sender as StackPanel;
            Song song = stack.Tag as Song;
            _currentIndex = this.listSong.Count;
            Debug.WriteLine(song.name);
            PlayerElement.Source = new Uri(song.link);
           
            
        }

        //private void play_song()
        //{
        //    is_play = true;
        //    this.PlayerElement.Play();
        //    this.playbth.Symbol = Symbol.Pause;
        //}
        //private void pause_song()
        //{
        //    is_play = false;
        //    this.PlayerElement.Play();
        //    this.playbth.Symbol = Symbol.Play;
        //}

        

       
    }
}
