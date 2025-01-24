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

        // Constructor that takes a FirebaseClient instance as a parameter
        public ListedPropertyViewer(FirebaseClient client)
        {
            firebaseClient = client;
        }

        // Asynchronous method to retrieve the image URL from Firebase database
        public async Task<string> GetImageUrlFromFirebase(string username)
        {
            try
            {
                // Get a reference to the user's "Listed Property" node in the Firebase database
                var userCredsRef = firebaseClient
                    .Child("Users")
                    .Child(username)
                    .Child("Listed Property")
                    .Child("ImageUrls");

                // Retrieve the image URL as a single value asynchronously
                return await userCredsRef.OnceSingleAsync<string>();
            }
            catch (Exception ex)
            {
                // Display an error message if an exception occurs while retrieving the image URL
                MessageBox.Show($"Error retrieving image: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        // Method to display the image in a FlowLayoutPanel
        public void DisplayImages(string imageUrl, FlowLayoutPanel flowPanelImages)
        {
            try
            {
                // Clear any existing controls in the FlowLayoutPanel
                flowPanelImages.Controls.Clear();

                // If the image URL is not null or empty, create a PictureBox and display the image
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
                // Display an error message if an exception occurs while displaying the image
                MessageBox.Show($"Error displaying image: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}