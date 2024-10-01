using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Firebase.Database;
using Firebase.Database.Query;
using Firebase.Storage;

namespace AirBnB
{
    public partial class frmList : Form
    {
        // List to hold the selected file paths from the file explorer
        private List<string> selectedFiles;

        // Firebase client to interact with the Realtime Database
        private FirebaseClient firebaseClient;

        public frmList()
        {
            InitializeComponent();

            // Initialize the selectedFiles list to store file paths later
            selectedFiles = new List<string>();
            InitializeFirebase(); // Initialize Firebase on form load
        }

        // Method to initialize Firebase
        public void InitializeFirebase()
        {
            // Get the current directory and construct the path to the Firebase JSON credentials file
            string basePath = Directory.GetCurrentDirectory();
            string pathToJson = Path.Combine(basePath, "FireBase", "airbnb-d4964-firebase-adminsdk-cbzm0-e68a4950cf.json");

            // Initialize Firebase Realtime Database client using the JSON key file
            firebaseClient = new FirebaseClient("https://airbnb-d4964-default-rtdb.europe-west1.firebasedatabase.app/");
        }

        private void uploadButton_Click(object sender, EventArgs e)
        {
            string username = GlobalData.Username;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                // Set filter for file types to allow only images
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png|All Files|*.*";
                openFileDialog.Title = "Select Image Files";
                openFileDialog.Multiselect = true; // Allow multiple file selection

                // Show the file dialog and check if user selected files
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Add selected files to the list of selectedFiles
                    selectedFiles.AddRange(openFileDialog.FileNames);
                    // Call the method to upload the images to Firebase
                    UploadImagesToFirebase();

                    this.Close();
                }
            }
        }

        // Method to upload images to Firebase Storage
        private async void UploadImagesToFirebase()
        {
            string username = GlobalData.Username;

            // List to hold the download URLs of the uploaded images
            List<string> imageUrls = new List<string>();

            // Loop through each file in the selectedFiles list
            foreach (var filePath in selectedFiles)
            {
                // Open a stream to read the file
                var stream = File.OpenRead(filePath);
                // Create a Firebase Storage reference to upload the file
                var storage = new FirebaseStorage("airbnb-d4964.appspot.com")
                    .Child(username)
                    .Child("images") // Folder name in Firebase Storage
                    .Child(Path.GetFileName(filePath)); // File name

                // Upload the file and get the download URL
                var downloadUrl = await storage.PutAsync(stream);
                // Add the download URL to the list
                imageUrls.Add(downloadUrl);
            }

            // Store the image URLs in the Realtime Database under the user's credentials
            await AddImageUrlsToDatabase(username, imageUrls);
        }

        // Method to add image URLs to the Firebase Realtime Database
        private async Task AddImageUrlsToDatabase(string username, List<string> newImageUrls)
        {
            // Reference to the "Users" node in the Realtime Database
            var userCredsRef = firebaseClient
                .Child("Users")
                .Child(username) // Use the username as the key
                .Child("Listed Property");

            // Retrieve existing image URLs
            var existingImageUrls = await userCredsRef
                .Child("ImageUrls")
                .OnceSingleAsync<List<string>>();

            // If there are existing URLs, add the new ones to the list
            if (existingImageUrls != null)
            {
                existingImageUrls.AddRange(newImageUrls);
            }
            else
            {
                // If no existing URLs, initialize the list with the new URLs
                existingImageUrls = newImageUrls;
            }

            // Store the updated list of image URLs in the database
            await userCredsRef.Child("ImageUrls").PutAsync(existingImageUrls);

            await firebaseClient
                .Child("Available Properties")
                .Child(username)
                .Child("ImageUrls")
                .PutAsync(existingImageUrls);

            await firebaseClient
                .Child("Users")
                .Child(username)
                .Child("isListing")
                .PutAsync(true);

            // Show a success message to the user
            MessageBox.Show("Images uploaded successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
