using Firebase.Database;
using Firebase.Database.Query;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AirBnB
{
    public class ListedPropertyViewer
    {
        private FirebaseClient firebaseClient;

        public ListedPropertyViewer(FirebaseClient client)
        {
            firebaseClient = client;
        }

        public async Task<List<string>> GetImageUrlsFromFirebase(string username)
        {
            var userCredsRef = firebaseClient
                .Child("Users")
                .Child(username)
                .Child("Listed Property")
                .Child("ImageUrls");

            return await userCredsRef.OnceSingleAsync<List<string>>();
        }

        public void DisplayImages(List<string> imageUrls, FlowLayoutPanel flowPanelImages)
        {
            flowPanelImages.Controls.Clear();

            foreach (var imageUrl in imageUrls)
            {
                PictureBox pictureBox = new PictureBox
                {
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Size = new System.Drawing.Size(200, 200)
                };
                pictureBox.Load(imageUrl);

                flowPanelImages.Controls.Add(pictureBox);
            }
        }
    }
}