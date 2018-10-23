using Asm.Emtity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Asm.Sevices;
using System.Net.Http.Headers;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Asm.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class information : Page
    {
        Information infor = new Information();
        public information()
        {
            this.InitializeComponent();
            CheckCredential();
            GetToken();
            GetInfo();
           
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

        public async void GetInfo()
        {
            string text = await CheckCredential();
            if (text != "")
            {
            string token = await GetToken();
            
            if (token != "")
            {
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);
                var response = httpClient.GetAsync(URL.API_INFORMATION);
                var content = await response.Result.Content.ReadAsStringAsync();
                Debug.WriteLine(content);
                infor = JsonConvert.DeserializeObject<Information>(content);
                
                Debug.WriteLine(infor.email);
                this.txt_fullname.Text = infor.firstName + " " + infor.lastName;
                this.txt_birthday.Text = infor.birthday;
                this.txt_email.Text = infor.email;
                this.txt_address.Text = infor.address;
                
                this.img_avatar.ProfilePicture = new BitmapImage(new Uri(infor.avatar, UriKind.Absolute));
            }
            }
            else
            {
               
                
            }
            Debug.WriteLine(text);
        }
    }
}
