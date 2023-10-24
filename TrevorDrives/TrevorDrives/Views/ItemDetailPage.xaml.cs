using System.ComponentModel;
using TrevorDrives.ViewModels;
using Xamarin.Forms;

namespace TrevorDrives.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}