using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace ContosoMoments.Views
{
	public partial class ImagesList : ContentPage
	{
		public ImagesList ()
		{
			InitializeComponent ();
		}

        public async void OnRefresh(object sender, EventArgs e)
        {
        }

        public async void OnSelected(object sender, SelectedItemChangedEventArgs e)
        {
        }
    }
}
