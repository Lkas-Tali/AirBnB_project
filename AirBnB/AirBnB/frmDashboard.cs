using Firebase.Database;
using Firebase.Database.Query;
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
using Firebase.Storage;
using System.Net;
using Google.Cloud.Firestore;
using System.Web.UI.Design.WebControls;
using Google.Api;

namespace AirBnB
{
    public partial class frmDashboard : Form
    {
        // List to hold the selected file paths from the file explorer
        private List<string> selectedFiles;

        // String to hold the selected file path from the file explorer
        private string selectedFile;

        // Firebase client to interact with the Realtime Database
        private FirebaseClient firebaseClient;

        public frmDashboard()
        {
            InitializeComponent();
            InitializeFirebase(); // Initialize Firebase when the form loads

            // Initialize the selectedFiles list to store file paths later
            selectedFiles = new List<string>();
        }

        public void InitializeFirebase()
        {
            string basePath = Directory.GetCurrentDirectory();
            string pathToJson = Path.Combine(basePath, "FireBase", "airbnb-d4964-firebase-adminsdk-cbzm0-e68a4950cf.json");

            // Initialize Firebase Realtime Database client
            firebaseClient = new FirebaseClient("https://airbnb-d4964-default-rtdb.europe-west1.firebasedatabase.app/");
        }

        // Load the home panel
        private void frmListed_Load(object sender, EventArgs e)
        {
            usernameLabel.Text = GlobalData.Username;
            ShowPanel(panelHome);
        }

        // load the list property panel
        private void listButton_Click(object sender, EventArgs e)
        {
            ShowPanel(panelList);
        }

        // Load the available property panel
        private async void bookButton_Click(object sender, EventArgs e)
        {
            ShowPanel(panelBook);

            // Retrieve available properties from Firebase
            var properties = await GetAvailablePropertiesFromFirebase();

            if (properties != null && properties.Count > 0)
            {
                // Display the front images and addresses in a FlowLayoutPanel
                DisplayAvailableProperties(properties);
            }
            else
            {
                MessageBox.Show("No available properties found.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // Load the listed property panel
        private async void listedButton_Click(object sender, EventArgs e)
        {
            ShowPanel(panelListed);

            string username = GlobalData.Username;

            // Retrieve image URLs from Firebase Realtime Database
            var imageUrls = await GetImageUrlsFromFirebase(username);

            if (imageUrls != null && imageUrls.Count > 0)
            {
                // Display the images in a FlowLayoutPanel
                DisplayImages(imageUrls);
            }
            else
            {
                MessageBox.Show("No listed property images found.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // Logout method
        private void button3_Click(object sender, EventArgs e)
        {
            new frmLogin().Show();
            this.Close();
        }

        // Get image URLs from the database
        private async Task<List<string>> GetImageUrlsFromFirebase(string username)
        {
            var userCredsRef = firebaseClient
                .Child("Users")
                .Child(username)
                .Child("Listed Property")
                .Child("ImageUrls");

            // Fetch the list of image URLs from Firebase
            var imageUrls = await userCredsRef.OnceSingleAsync<List<string>>();

            return imageUrls;
        }

        // Display the listed property images
        private void DisplayImages(List<string> imageUrls)
        {
            // Clear any existing images in the FlowLayoutPanel
            flowPanelImages.Controls.Clear();

            foreach (var imageUrl in imageUrls)
            {
                // Create a PictureBox for each image
                PictureBox pictureBox = new PictureBox();
                pictureBox.Load(imageUrl); // Load the image from the URL
                pictureBox.SizeMode = PictureBoxSizeMode.Zoom; // Adjust size mode as needed
                pictureBox.Size = new Size(200, 200); // Set size for the PictureBox

                // Add the PictureBox to the FlowLayoutPanel
                flowPanelImages.Controls.Add(pictureBox);
            }
        }

        // load panel method
        private void ShowPanel(Panel panel)
        {
            // Hide all panels and then show the desired one.
            panelList.Visible = false;
            panelListed.Visible = false;
            panelBook.Visible = false;
            panelHome.Visible = false;
            searchPanel.Visible = false;

            panel.Visible = true;
        }

        // Method to upload images to Firebase Storage
        private async Task UploadImagesToFirebase()
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
            var userCredsRef = firebaseClient
                .Child("Users")
                .Child(username)
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

            // Update the Available Properties node with the image URLs
            await firebaseClient
                .Child("Available Properties")
                .Child(username)
                .Child("ImageUrls")
                .PutAsync(existingImageUrls);
        }

        // Upload images to the database
        private async void uploadButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtAddress.Text) || string.IsNullOrWhiteSpace(txtCity.Text) ||
                string.IsNullOrWhiteSpace(txtDescription.Text) || string.IsNullOrWhiteSpace(txtPrice.Text) ||
                string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                MessageBox.Show("Please fill up all the fields!", "Action Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                string username = GlobalData.Username;

                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png|All Files|*.*";
                    openFileDialog.Title = "Select Image Files";
                    openFileDialog.Multiselect = true;

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        selectedFiles.AddRange(openFileDialog.FileNames);
                        await UploadImagesToFirebase();

                        // Create a dictionary to hold all property details
                        var propertyDetails = new Dictionary<string, object>
                {
                    { "Address", txtAddress.Text },
                    { "City", txtCity.Text },
                    { "Title", txtTitle.Text },
                    { "PricePerNight", txtPrice.Text },
                    { "Description", txtDescription.Text }
                };

                        // Update the Available Properties node with all details
                        await firebaseClient
                            .Child("Available Properties")
                            .Child(username)
                            .Child("Address")
                            .PutAsync(propertyDetails);

                        // Update the user's listing status
                        await firebaseClient
                            .Child("Users")
                            .Child(username)
                            .Child("isListing")
                            .PutAsync(true);

                        MessageBox.Show("Property details and images uploaded successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        // Front image upload Button 
        private void frontImageButton_Click(object sender, EventArgs e)
        {
            string username = GlobalData.Username;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                // Set filter for file types to allow only images
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png|All Files|*.*";
                openFileDialog.Title = "Select Image Files";
                openFileDialog.Multiselect = false; // Disallow multiple file selection

                // Show the file dialog and check if user selected a file
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Add selected files to the list of selectedFiles
                    selectedFile = openFileDialog.FileName;
                    // Call the method to upload the images to Firebase
                    UploadFrontImageToFirebase();
                }
            }
        }

        // Upload front image
        private async void UploadFrontImageToFirebase()
        {
            string username = GlobalData.Username;

            // Open a stream to read the file
            var stream = File.OpenRead(selectedFile);
            // Create a Firebase Storage reference to upload the file
            var storage = new FirebaseStorage("airbnb-d4964.appspot.com")
                .Child(username)
                .Child("front image") // Folder name in Firebase Storage
                .Child(Path.GetFileName(selectedFile)); // File name

            // Upload the file and get the download URL
            var imageUrl = await storage.PutAsync(stream);

            // Store the image URLs in the Realtime Database under the user's credentials
            await AddFrontImageUrlsToDatabase(username, imageUrl);
        }

        // Upload the front image URL to the database
        private async Task AddFrontImageUrlsToDatabase(string username,string frontImageUrl)
        {
            string email = GlobalData.email;

            Dictionary<string, string> dicFrontImageUrl = new Dictionary<string, string>();

            dicFrontImageUrl = keyValuePairs("Front Image", frontImageUrl);

            var userCredsRef = firebaseClient
                .Child("Available Properties")
                .Child(username);

            // Get the existing data from Firebase
            var existingData = await userCredsRef.OnceSingleAsync<Dictionary<string, object>>();

            // If existing data is null, initialize it as an empty dictionary
            if (existingData == null)
            {
                existingData = new Dictionary<string, object>();
            }

            // Add/Update the front image URL in the existing data
            existingData["Front Image"] = frontImageUrl;
            existingData["Name"] = username;
            existingData["Email"] = email;

            // Update the database with the modified data (keeping existing values like address)
            await userCredsRef.PutAsync(existingData);

            // Show a success message to the user
            MessageBox.Show("Image uploaded successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // Store key/value pairs
        private Dictionary<string, string> keyValuePairs (string key, string value)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();

            dictionary.Add(key, value);

            return dictionary;
        }

        // Display the front images and addresses of the available properties
        private void DisplayAvailableProperties(List<Property> properties)
        {
            // Clear any existing controls in the FlowLayoutPanel
            flowPanelBook.Controls.Clear();

            foreach (var property in properties)
            {
                // Create a PictureBox to display the front image
                PictureBox pictureBox = new PictureBox();
                pictureBox.Load(property.FrontImageUrl); // Load the front image from the URL
                pictureBox.SizeMode = PictureBoxSizeMode.Zoom; // Adjust size mode as needed
                pictureBox.Size = new Size(200, 200); // Set size for the PictureBox

                // Create a Label to display the address below the image
                Label addressLabel = new Label();
                addressLabel.Text = property.Address;
                addressLabel.AutoSize = true;
                addressLabel.TextAlign = ContentAlignment.MiddleCenter;

                // Create a Panel to hold both the PictureBox and Label
                Panel propertyPanel = new Panel();
                propertyPanel.Size = new Size(200, 250); // Adjust size as needed
                propertyPanel.Controls.Add(pictureBox); // Add the PictureBox to the Panel
                propertyPanel.Controls.Add(addressLabel); // Add the Label to the Panel

                // Position the Label below the PictureBox
                addressLabel.Location = new Point(0, pictureBox.Bottom);

                // Add the Panel to the FlowLayoutPanel
                flowPanelBook.Controls.Add(propertyPanel);
            }
        }

        // Get available properties from Firebase
        private async Task<List<Property>> GetAvailablePropertiesFromFirebase()
        {
            List<Property> properties = new List<Property>();
            var availablePropertiesRef = firebaseClient.Child("Available Properties");
            var availableProperties = await availablePropertiesRef.OnceAsync<Dictionary<string, object>>();

            foreach (var property in availableProperties)
            {
                var data = property.Object;

                if (data.ContainsKey("Front Image") && data.ContainsKey("Address"))
                {
                    string frontImage = data["Front Image"].ToString();
                    string address = "";

                    // Check if Address is a nested object
                    if (data["Address"] is Dictionary<string, object> addressData)
                    {
                        if (addressData.ContainsKey("Address"))
                        {
                            address = addressData["Address"].ToString();
                        }
                    }
                    else
                    {
                        // Fallback if Address is a direct string
                        address = data["Address"].ToString();
                    }

                    Property newProperty = new Property
                    {
                        FrontImageUrl = frontImage,
                        Address = address
                    };

                    properties.Add(newProperty);
                }
            }

            return properties;
        }

        private void searchLabel_Click(object sender, EventArgs e)
        {
            ShowPanel(searchPanel);
        }

        private void searchButton_Click(object sender, EventArgs e)
        {
            
        }

        private async Task<List<Property>> SearchPropertiesByCity(string city)
        {
            List<Property> properties = new List<Property>();

            // Get a reference to the "Available Properties" node in Firebase
            var availablePropertiesRef = firebaseClient.Child("Available Properties");

            // Retrieve all available properties
            var availableProperties = await availablePropertiesRef.OnceAsync<Dictionary<string, object>>();

            // Iterate through each property
            foreach (var property in availableProperties)
            {
                var data = property.Object;

                // Check if the property has an Address field and it's a Dictionary
                if (data.ContainsKey("Address") && data["Address"] is Dictionary<string, object> addressData)
                {
                    // Check if the property's city matches the search query
                    if (addressData.ContainsKey("City") && addressData["City"].ToString().ToLower() == city)
                    {
                        // Extract property details
                        string frontImage = data.ContainsKey("Front Image") ? data["Front Image"].ToString() : "";
                        string address = addressData.ContainsKey("Address") ? addressData["Address"].ToString() : "";
                        string pricePerNight = data.ContainsKey("PricePerNight") ? data["PricePerNight"].ToString() : "";

                        // Create a new Property object and add it to the list
                        Property newProperty = new Property
                        {
                            FrontImageUrl = frontImage,
                            Address = address
                        };

                        properties.Add(newProperty);
                    }
                }
            }

            return properties;
        }
    }

    // Class to hold property details
    public class Property
    {
        public string FrontImageUrl { get; set; }
        public string Address { get; set; }
    }
}
