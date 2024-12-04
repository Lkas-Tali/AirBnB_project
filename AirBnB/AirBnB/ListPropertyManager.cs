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

        public ListPropertyManager(FirebaseClient client)
        {
            firebaseClient = client;
            selectedFiles = new List<string>();
        }

        public async Task UploadProperty(string username, string address, string city, string title, string price, string description)
        {
            if (string.IsNullOrWhiteSpace(address) || string.IsNullOrWhiteSpace(city) ||
                string.IsNullOrWhiteSpace(description) || string.IsNullOrWhiteSpace(price) ||
                string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException("Please fill up all the fields!");
            }

            await UploadImagesToFirebase(username);

            //Capitalise city
            city = char.ToUpper(city[0]) + city.Substring(1).ToLower();

            // Create the Address node data
            var addressDetails = new Dictionary<string, object>
            {
                { "Address", address },
                { "City", city },
                { "Title", title },
                { "Description", description }
            };

            // First update the Address node
            await firebaseClient
                .Child("Available Properties")
                .Child(username)
                .Child("Address")
                .PutAsync(addressDetails);

            // Then update PricePerNight at the root level
            await firebaseClient
                .Child("Available Properties")
                .Child(username)
                .Child("PricePerNight")
                .PutAsync(price);

            await firebaseClient
                .Child("Users")
                .Child(username)
                .Child("isListing")
                .PutAsync(true);
        }

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