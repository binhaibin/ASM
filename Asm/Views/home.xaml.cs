using Asm.Emtity;
using Asm.Sevices;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System;
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
    public sealed partial class home : Page
    {
        public home()
        {
            this.InitializeComponent();
            CheckCredential();
            GetToken();
            
            
        }
        private Type currentPage;

        // List of ValueTuple holding the Navigation Tag and the relative Navigation Page 
        private readonly IList<(string Tag, Type Page)> _pages = new List<(string Tag, Type Page)>
{
    ("home", typeof(Home1)),
    ("login", typeof(login)),
    ("register", typeof(register)),
    ("information", typeof(information)),
    ("music", typeof(music)),
};

        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            // You can also add items in code behind
            NavView.MenuItems.Add(new NavigationViewItemSeparator());
            NavView.MenuItems.Add(new NavigationViewItem
            {
                Content = "My content",
                Icon = new SymbolIcon(Symbol.Folder),
                Tag = "content"
            });
            _pages.Add(("content", typeof(music)));

            ContentFrame.Navigated += On_Navigated;

            // NavView doesn't load any page by default: you need to specify it
            NavView_Navigate("home");

            // Add keyboard accelerators for backwards navigation
            var goBack = new KeyboardAccelerator { Key = VirtualKey.GoBack };
            goBack.Invoked += BackInvoked;
            this.KeyboardAccelerators.Add(goBack);

            // ALT routes here
            var altLeft = new KeyboardAccelerator
            {
                Key = VirtualKey.Left,
                Modifiers = VirtualKeyModifiers.Menu
            };
            altLeft.Invoked += BackInvoked;
            this.KeyboardAccelerators.Add(altLeft);
        }

        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.InvokedItem == null)
                return;

            if (args.IsSettingsInvoked)
                ContentFrame.Navigate(typeof(music));
            else
            {
                // Getting the Tag from Content (args.InvokedItem is the content of NavigationViewItem)
                var navItemTag = NavView.MenuItems
                    .OfType<NavigationViewItem>()
                    .First(i => args.InvokedItem.Equals(i.Content))
                    .Tag.ToString();

                NavView_Navigate(navItemTag);
            }
        }

        private void NavView_Navigate(string navItemTag)
        {
            var item = _pages.First(p => p.Tag.Equals(navItemTag));
            if (currentPage == item.Page)
                return;
            ContentFrame.Navigate(item.Page);

            currentPage = item.Page;
        }

        private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args) => On_BackRequested();

        private void BackInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            On_BackRequested();
            args.Handled = true;
        }

        private bool On_BackRequested()
        {
            if (!ContentFrame.CanGoBack)
                return false;

            // Don't go back if the nav pane is overlayed
            if (NavView.IsPaneOpen &&
                (NavView.DisplayMode == NavigationViewDisplayMode.Compact ||
                NavView.DisplayMode == NavigationViewDisplayMode.Minimal))
                return false;

            ContentFrame.GoBack();
            return true;
        }

        private void On_Navigated(object sender, NavigationEventArgs e)
        {
            NavView.IsBackEnabled = ContentFrame.CanGoBack;

            if (ContentFrame.SourcePageType == typeof(music))
            {
                // SettingsItem is not part of NavView.MenuItems, and doesn't have a Tag
                NavView.SelectedItem = (NavigationViewItem)NavView.SettingsItem;
            }
            else
            {
                var item = _pages.First(p => p.Page == e.SourcePageType);

                NavView.SelectedItem = NavView.MenuItems
                    .OfType<NavigationViewItem>()
                    .First(n => n.Tag.Equals(item.Tag));
            }
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
        private async void bth_logout(object sender, RoutedEventArgs e)
        {
            string text = await CheckCredential();
            if (text != "")
            {
                string token = await GetToken();

                if (token != "")
                {
                    try
                    {
                        Windows.Storage.StorageFolder storageFolder =
                            Windows.Storage.ApplicationData.Current.LocalFolder;
                        Windows.Storage.StorageFile sampleFile =
                            await storageFolder.CreateFileAsync("token.txt",
                                Windows.Storage.CreationCollisionOption.ReplaceExisting);
                        await Windows.Storage.FileIO.WriteTextAsync(sampleFile, "");
                        this.ContentFrame.Navigate(typeof(Home1));
                        Debug.WriteLine(token);
                        ContentDialog noWifiDialog = new ContentDialog
                        {
                            Title = "Success",
                            Content = "Đăng xuất thành công",
                            CloseButtonText = "Ok"
                        };

                        ContentDialogResult result = await noWifiDialog.ShowAsync();
                        this.ContentFrame.Navigate(typeof(Home1));
                        
                    }
                    catch
                    {
                        Debug.WriteLine("123t");
                    }

                }
            }
            else
            {
                

                ContentDialog noWifiDialog = new ContentDialog
                {
                    Title = "warning",
                    Content = "Bạn chưa đăng nhập",
                    CloseButtonText = "Ok"
                };
               
                ContentDialogResult result = await noWifiDialog.ShowAsync();
                
            }


        }
       
        private async void check_login(object sender, TappedRoutedEventArgs e)
        {
            string text = await CheckCredential();
            if (text != "")
            {
                string token = await GetToken();

                if (token != "")
                {
                    
                    ContentDialog noWifiDialog = new ContentDialog
                    {
                        Title = "warning",
                        Content = "Bạn đã đăng nhập",
                        CloseButtonText = "Ok"
                    };
                    
                    ContentDialogResult result = await noWifiDialog.ShowAsync();
                    
                    this.ContentFrame.Navigate(typeof(Home1));

                }
            }
        }

        private async void taptap_tap(object sender, TappedRoutedEventArgs e)
        {
            string text = await CheckCredential();
            if (text != "")
            {
                string token = await GetToken();

                if (token != "")
                {
                    
                }
            }
            else
            {
                this.ContentFrame.Navigate(typeof(login));

                ContentDialog noWifiDialog = new ContentDialog
                {
                    Title = "warning",
                    Content = "Bạn chưa đăng nhập",

                    CloseButtonText = "Ok"

                };

                ContentDialogResult result = await noWifiDialog.ShowAsync();
                

            }
            Debug.WriteLine(text);
        }
    }
    }
    

