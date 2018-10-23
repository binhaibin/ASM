using Asm.Emtity;
using Asm.Sevices;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Asm.Views
{

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class register : Page
    {
        Member ms = new Member();
        private StorageFile photo;
        private string currentUploadUrl;
        private bool validfn = false;
        private bool validln = false;
        private bool validemail = false;
        private bool validpass = false;
        private bool validphone = false;
        public register()
        {
            this.InitializeComponent();
        }

        private async void bth_signup(object sender, RoutedEventArgs e)
        {
            this.ms.email = this.t_email.Text;
            this.ms.password = this.t_password.Password;
            this.ms.firstName = this.t_firstname.Text;
            this.ms.lastName = this.t_lastname.Text;
            this.ms.address = this.t_address.Text;
            this.ms.introduction = this.t_introduction.Text;
            this.ms.phone = this.t_phone.Text;
            this.ms.avatar = this.UrlImage.Text;

            HttpClient httpClient = new HttpClient();
            var content = new StringContent(JsonConvert.SerializeObject(ms), System.Text.Encoding.UTF8,
                "application/json");
            var response = httpClient.PostAsync(URL.API_REGISTER, content);
            var contents = await response.Result.Content.ReadAsStringAsync();
            Debug.WriteLine(contents);
            if (response.Result.StatusCode == HttpStatusCode.Created)
            {
                ContentDialog noWifiDialog = new ContentDialog
                {
                    Title = "Success",
                    Content = "Đăng kí thành công",
                    CloseButtonText = "Ok"
                };
                ContentDialogResult result = await noWifiDialog.ShowAsync();
                email.Text = String.Empty;
                password.Text = String.Empty;
                firstName.Text = String.Empty;
                lastName.Text = String.Empty;
                address.Text = String.Empty;
                phone.Text = String.Empty;
                email.Text = String.Empty;
                avatar.Text = String.Empty;
                t_email.Text = String.Empty;
                t_password.Password = String.Empty;
                t_firstname.Text = String.Empty;
                t_lastname.Text = String.Empty;
                t_address.Text = String.Empty;
                t_phone.Text = String.Empty;
                t_email.Text = String.Empty;
                UrlImage.Text = String.Empty;
            }
            else
            {

                ErrorResponse errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(contents);
                //Debug.WriteLine(errorResponse.error["firstName"]);
                if (errorResponse.error.Count > 0)
                {
                    foreach (var key in errorResponse.error.Keys)
                    {
                        if (this.FindName(key) is TextBlock textBlock)
                        {
                            textBlock.Text = errorResponse.error[key];
                        }
                    }
                }
            }


        }

        private async void bth_capture(object sender, RoutedEventArgs e)
        {
            CameraCaptureUI captureUI = new CameraCaptureUI();
            captureUI.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;
            captureUI.PhotoSettings.CroppedSizeInPixels = new Size(200, 200);

            photo = await captureUI.CaptureFileAsync(CameraCaptureUIMode.Photo);

            if (photo == null)
            {
                // User cancelled photo capture
                return;
            }

            HttpClient httpClient = new HttpClient();
            currentUploadUrl = await httpClient.GetStringAsync(URL.GET_UPLOAD_IMAGE);
            Debug.WriteLine(currentUploadUrl);
            HttpUploadFile(currentUploadUrl, "myFile", "image/png");
        }

        private void date_change(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            this.ms.birthday = sender.Date.Value.ToString("yyyy-MM-dd");
        }

        public async void HttpUploadFile(string url, string paramName, string contentType)
        {
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            HttpWebRequest wr = (HttpWebRequest) WebRequest.Create(url);
            wr.ContentType = "multipart/form-data; boundary=" + boundary;
            wr.Method = "POST";

            Stream rs = await wr.GetRequestStreamAsync();
            rs.Write(boundarybytes, 0, boundarybytes.Length);

            string header =
                string.Format(
                    "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n",
                    paramName, "path_file", contentType);
            byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
            rs.Write(headerbytes, 0, headerbytes.Length);

            // write file.
            Stream fileStream = await this.photo.OpenStreamForReadAsync();
            byte[] buffer = new byte[4096];
            int bytesRead = 0;
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                rs.Write(buffer, 0, bytesRead);
            }

            byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            rs.Write(trailer, 0, trailer.Length);

            WebResponse wresp = null;
            try
            {
                wresp = await wr.GetResponseAsync();
                Stream stream2 = wresp.GetResponseStream();
                StreamReader reader2 = new StreamReader(stream2);
                //Debug.WriteLine(string.Format("File uploaded, server response is: @{0}@", reader2.ReadToEnd()));
                //string imgUrl = reader2.ReadToEnd();
                //Uri u = new Uri(reader2.ReadToEnd(), UriKind.Absolute);
                //Debug.WriteLine(u.AbsoluteUri);
                //ImageUrl.Text = u.AbsoluteUri;
                //MyAvatar.Source = new BitmapImage(u);
                //Debug.WriteLine(reader2.ReadToEnd());
                string imageUrl = reader2.ReadToEnd();
                ava_pic.ProfilePicture = new BitmapImage(new Uri(imageUrl, UriKind.Absolute));
                UrlImage.Text = imageUrl;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error uploading file", ex.StackTrace);
                Debug.WriteLine("Error uploading file", ex.InnerException);
                if (wresp != null)
                {
                    wresp = null;
                }
            }
            finally
            {
                wr = null;
            }
        }

        private void radio_checked(object sender, RoutedEventArgs e)
        {
            RadioButton radio = sender as RadioButton;
            this.ms.gender = Int32.Parse(radio.Tag.ToString());
        }

        private void aaaaa(object sender, TextChangedEventArgs e)
        {
            Regex ddd = new Regex("^[_A-Za-z0-9-\\+]+(\\.[_A-Za-z0-9-]+)*@"
                                  + "[A-Za-z0-9-]+(\\.[A-Za-z0-9]+)*(\\.[A-Za-z]{2,})$");
            if (ddd.IsMatch(t_email.Text))
            {
                email.Text = "valid:true";
                validemail = true;
            }
            else
            {
                email.Text = "valid:false";
            }
        }

       

        private void fn(object sender, TextChangedEventArgs e)
        {
            Regex ddd = new Regex(@"^[a-zA-Z_ÀÁÂÃÈÉÊÌÍÒÓÔÕÙÚĂĐĨŨƠàáâãèéêìíòóôõùúăđĩũơƯĂẠẢẤẦẨẪẬẮẰẲẴẶ" +
                                  "ẸẺẼỀỀỂưăạảấầẩẫậắằẳẵặẹẻẽềềểỄỆỈỊỌỎỐỒỔỖỘỚỜỞỠỢỤỦỨỪễệỉịọỏốồổỗộớờởỡợ" +
                                  "ụủứừỬỮỰỲỴÝỶỸửữựỳỵỷỹ\\s]+$");
            if (ddd.IsMatch(t_firstname.Text))
            {
                firstName.Text = "valid:true";
                validfn = true;
            }
            else
            {
                firstName.Text = "valid:false";
            }
        }

        private void ln(object sender, TextChangedEventArgs e)
        {
            Regex ddd = new Regex(@"^[a-zA-Z_ÀÁÂÃÈÉÊÌÍÒÓÔÕÙÚĂĐĨŨƠàáâãèéêìíòóôõùúăđĩũơƯĂẠẢẤẦẨẪẬẮẰẲẴẶ" +
                                  "ẸẺẼỀỀỂưăạảấầẩẫậắằẳẵặẹẻẽềềểỄỆỈỊỌỎỐỒỔỖỘỚỜỞỠỢỤỦỨỪễệỉịọỏốồổỗộớờởỡợ" +
                                  "ụủứừỬỮỰỲỴÝỶỸửữựỳỵỷỹ\\s]+$");
            if (ddd.IsMatch(t_lastname.Text))
            {
                lastName.Text = "valid:true";
                validln = true;
            }
            else
            {
                lastName.Text = "valid:false";
            }
        }

        private void pp(object sender, TextChangedEventArgs e)
        {
            Regex ddd = new Regex(@"^\s*\+?\s*([0-9][\s-]*){9,}$");
            if (ddd.IsMatch(t_phone.Text))
            {
                phone.Text = "valid:true";
                validphone = true;
            }
            else
            {
                phone.Text = "valid:false";
            }
        }

        private void t_password_PasswordChanged(object sender, RoutedEventArgs e)
        {
           
            if (t_password.Password.Length < 6 || t_password.Password.Length > 24)
            {
                password.Text = "valid:false";
                validpass = true;
            }
            else
            {
                password.Text = "valid:true";
            }
           
        }

      
    }
    }


