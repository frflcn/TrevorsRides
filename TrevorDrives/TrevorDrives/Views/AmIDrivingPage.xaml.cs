using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TrevorDrives.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AmIDrivingPage : ContentPage
    {
        public AmIDrivingPage()
        {
            InitializeComponent();
        }
        public void AmIDrivingToggled(object sender, EventArgs e)
        {

        }
        public void AmIDrivingForUberToggled(object sender, EventArgs e)
        {

        }
    }
}