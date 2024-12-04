using Firebase.Database;
using Firebase.Database.Query;
using System;
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
            try
            {
                var userCredsRef = firebaseClient
                    .Child("Users")
                    .Child(username)
                    .Child("Listed Property")
                    .Child("ImageUrls");

                return await userCredsRef.OnceSingleAsync<string>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error retrieving image: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        public void DisplayImages(string imageUrl, FlowLayoutPanel flowPanelImages)
        {
            try
            {
                flowPanelImages.Controls.Clear();

                if (!string.IsNullOrEmpty(imageUrl))
                {
                    PictureBox pictureBox = new PictureBox
                    {
                        SizeMode = PictureBoxSizeMode.Zoom,
                        Size = new System.Drawing.Size(200, 200),
                        Margin = new Padding(5)
                    };
                    pictureBox.Load(imageUrl);
                    flowPanelImages.Controls.Add(pictureBox);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error displaying image: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}