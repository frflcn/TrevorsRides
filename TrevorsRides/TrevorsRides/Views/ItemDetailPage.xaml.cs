using System.ComponentModel;
using TrevorsRides.ViewModels;
using Xamarin.Forms;

namespace TrevorsRides.Views
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