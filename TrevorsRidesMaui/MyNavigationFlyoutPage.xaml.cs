using System.Windows.Input;
using TrevorsRidesHelpers;

namespace TrevorsRidesMaui;

public partial class MyNavigationFlyoutPage : ContentPage
{
	public Command command;
	public MyNavigationFlyoutPage()
	{
		InitializeComponent();
		BindingContext = this;
		ToolbarItem toolbarItem = new ToolbarItem();
		toolbarItem.Text = "ToolbarItem";

		toolbarItem.IconImageSource = ImageSource.FromResource("contacts.png");
		//ToolbarItems.Add(toolbarItem);
		MenuBarItem menuBarItem = new MenuBarItem();
		menuBarItem.Text = "File";
		
		command = new Command();
		
	}
	public class Command : ICommand
	{
		
		public bool CanExecute(object? obj)
		{
			return true;
		}
		public void Execute(object? obj)
		{
			Log.Debug("ICOMMAND", "EXECUTE"); 
		}
		public event EventHandler? CanExecuteChanged;
				
	}

    private void ToolbarItem_Clicked(object sender, EventArgs e)
    {

    }

    private void ImageButton_Clicked(object sender, EventArgs e)
    {

    }
}