using Firebase.Database;
using Firebase.Database.Query;
using Firebase.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AirBnB
{
    public class ListPropertyManager
    {
        private FirebaseClient firebaseClient;
        private List<string> selectedFiles;

        // Constructor that initializes the ListPropertyManager with a FirebaseClient
        public ListPropertyManager(FirebaseClient client)
        {
            firebaseClient = client;
            selectedFiles = new List<string>();
        }

        // Asynchronous method to upload a property to Firebase
        public async Task UploadProperty(string username, string address, string city, string title, string price, string description)
        {
            // Check if any of the required fields are empty or whitespace
            if (string.IsNullOrWhiteSpace(address) || string.IsNullOrWhiteSpace(city) ||
                string.IsNullOrWhiteSpace(description) || string.IsNullOrWhiteSpace(price) ||
                string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException("Please fill up all the fields!");
            }

            // Upload the selected images to Firebase
            await UploadImagesToFirebase(username);

            // Capitalize the first letter of the city
            city = char.ToUpper(city[0]) + city.Substring(1).ToLower();

            // Create a dictionary to store the address details
            var addressDetails = new Dictionary<string, object>
            {
                { "Address", address },
                { "City", city },
                { "Title", title },
                { "Description", description }
            };

            // Update the Address node in Firebase with the address details
            await firebaseClient
                .Child("Available Properties")
                .Child(username)
                .Child("Address")
                .PutAsync(addressDetails);

            // Update the PricePerNight node in Firebase with the price
            await firebaseClient
                .Child("Available Properties")
                .Child(username)
                .Child("PricePerNight")
                .PutAsync(price);

            // Update the isListing node in Firebase to indicate that the user is listing a property
            await firebaseClient
                .Child("Users")
                .Child(username)
                .Child("isListing")
                .PutAsync(true);
        }

        // Method to open a file dialog and select image files
        public void SelectFiles()
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png|All Files|*.*";
                openFileDialog.Title = "Select Image Files";
                openFileDialog.Multiselect = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    selectedFiles.AddRange(openFileDialog.FileNames);
                }
            }
        }

        // Asynchronous method to upload the selected images to Firebase Storage
        private async Task UploadImagesToFirebase(string username)
        {
            List<string> imageUrls = new List<string>();

            foreach (var filePath in selectedFiles)
            {
                var stream = File.OpenRead(filePath);
                var storage = new FirebaseStorage("airbnb-d4964.appspot.com")
                    .Child(username)
                    .Child("images")
                    .Child(Path.GetFileName(filePath));

                var downloadUrl = await storage.PutAsync(stream);
                imageUrls.Add(downloadUrl);
            }

            await AddImageUrlsToDatabase(username, imageUrls);
        }

        // Asynchronous method to add the image URLs to the Firebase Database
        private async Task AddImageUrlsToDatabase(string username, List<string> newImageUrls)
        {
            var userCredsRef = firebaseClient
                .Child("Users")
                .Child(username)
                .Child("Listed Property");

            var existingImageUrls = await userCredsRef
                .Child("ImageUrls")
                .OnceSingleAsync<List<string>>();

            if (existingImageUrls != null)
            {
                existingImageUrls.AddRange(newImageUrls);
            }
            else
            {
                existingImageUrls = newImageUrls;
            }

            await userCredsRef.Child("ImageUrls").PutAsync(existingImageUrls);

            await firebaseClient
                .Child("Available Properties")
                .Child(username)
                .Child("ImageUrls")
                .PutAsync(existingImageUrls);
        }

        // Asynchronous method to upload a front image for the property
        public async Task UploadFrontImage(string username, string email)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png|All Files|*.*";
                openFileDialog.Title = "Select Front Image";
                openFileDialog.Multiselect = false;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedFile = openFileDialog.FileName;
                    await UploadFrontImageToFirebase(username, email, selectedFile);
                }
            }
        }

        // Asynchronous method to upload the front image to Firebase Storage
        private async Task UploadFrontImageToFirebase(string username, string email, string filePath)
        {
            var stream = File.OpenRead(filePath);
            var storage = new FirebaseStorage("airbnb-d4964.appspot.com")
                .Child(username)
                .Child("front image")
                .Child(Path.GetFileName(filePath));

            var imageUrl = await storage.PutAsync(stream);

            await AddFrontImageUrlToDatabase(username, email, imageUrl);
        }

        // Asynchronous method to add the front image URL to the Firebase Database
        private async Task AddFrontImageUrlToDatabase(string username, string email, string frontImageUrl)
        {
            var userCredsRef = firebaseClient
                .Child("Available Properties")
                .Child(username);

            var existingData = await userCredsRef.OnceSingleAsync<Dictionary<string, object>>();

            if (existingData == null)
            {
                existingData = new Dictionary<string, object>();
            }

            existingData["Front Image"] = frontImageUrl;
            existingData["Name"] = username;
            existingData["Email"] = email;

            await userCredsRef.PutAsync(existingData);
        }
    }
}