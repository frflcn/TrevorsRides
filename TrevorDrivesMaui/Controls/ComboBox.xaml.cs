namespace TrevorDrivesMaui.Controls;

public partial class ComboBox : ContentView
{
	public List<ListViewEntry> ListOfEntries { get; set; }
	public ComboBox()
	{
		InitializeComponent();
		this.BindingContext = this;
	}
	public class ListViewEntry
	{
		public string MainText { get; set; }
		public string SubText { get; set; }
	}
}