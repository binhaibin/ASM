using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
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
using Asm.Emtity;
using System.Net;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Asm.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class login : Page
    {
        public login()
        {
            this.InitializeComponent();
        }

        private async void bth_login1(object sender, RoutedEventArgs e)
        {
            string email = this.tb_email.Text;
            string password = this.pb_pass.Password;
            Dictionary<String, String> memberLogin = new Dictionary<string, string>();
            memberLogin.Add("email", email);
            memberLogin.Add("password", password);
            HttpClient httpClient = new HttpClient();
            var content = new StringContent(JsonConvert.SerializeObject(memberLogin), System.Text.Encoding.UTF8, "application/json");
            var response = httpClient.PostAsync(URL.API_LOGIN, content);
            Debug.WriteLine(response);
            var tokenJson = await response.Result.Content.ReadAsStringAsync();
            Credential tokenResponse = JsonConvert.DeserializeObject<Credential>(tokenJson);
            string jsonUser = JsonConvert.SerializeObject(tokenResponse);
            if (response.Result.StatusCode == HttpStatusCode.Created)
            {
                try
                {
                    Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                    Windows.Storage.StorageFile sampleFile =
                        await storageFolder.CreateFileAsync("token.txt",
                            Windows.Storage.CreationCollisionOption.ReplaceExisting);
                    await Windows.Storage.FileIO.WriteTextAsync(sampleFile, jsonUser);

                    Debug.WriteLine("Success: " + jsonUser);
                    ContentDialog noWifiDialog = new ContentDialog
                    {
                        Title = "Success",
                        Content = "Đăng nhập thành công",
                        CloseButtonText = "Ok"
                    };

                    ContentDialogResult result = await noWifiDialog.ShowAsync();

                }
                catch
                {
                    Debug.WriteLine("Fail: " + jsonUser);
                }

            }
             else
            {

                this.errorMessage.Text = "* Sai tài khoản hoặc mật khẩu";
            }

            
        }

        private void bth_regi(object sender, RoutedEventArgs e)
        {
            this.frLogin.Navigate(typeof(register));
        }
    }
}
