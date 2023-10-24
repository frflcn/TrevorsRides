using System;
using System.Collections.Generic;
using System.ComponentModel;
using TrevorDrives.Models;
using TrevorDrives.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TrevorDrives.Views
{
    public partial class NewItemPage : ContentPage
    {
        public Item Item { get; set; }

        public NewItemPage()
        {
            InitializeComponent();
            BindingContext = new NewItemViewModel();
        }
    }
}