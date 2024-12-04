using Firebase.Database;
using Firebase.Database.Query;
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

        public async Task<string> GetImageUrlFromFirebase(string username)
        {
            var userCredsRef = firebaseClient
                .Child("Users")
                .Child(username)
                .Child("Listed Property")
                .Child("ImageUrls");

            return await userCredsRef.OnceSingleAsync<string>();
        }

        public void DisplayImages(string imageUrl, FlowLayoutPanel flowPanelImages)
        {
            flowPanelImages.Controls.Clear();

            if (!string.IsNullOrEmpty(imageUrl))
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