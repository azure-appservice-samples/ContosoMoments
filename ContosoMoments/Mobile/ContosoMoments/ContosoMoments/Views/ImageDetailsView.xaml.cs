using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace ContosoMoments.Views
{
	public partial class ImageDetailsView : ContentPage
	{
		public ImageDetailsView ()
		{
			InitializeComponent ();

            var tapUploadImage = new TapGestureRecognizer();
            tapUploadImage.Tapped += OnLike;
            imgLike.GestureRecognizers.Add(tapUploadImage);
        }

        public async void OnLike(object sender, EventArgs e)
        {
            //TODO
        }
    }
}
