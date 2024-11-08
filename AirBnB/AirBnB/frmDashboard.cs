using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace AirBnB
{
    public partial class frmDashboard : Form
    {
        private FirebaseClient firebaseClient;
        private ListPropertyManager listPropertyManager;
        private ListedPropertyViewer listedPropertyViewer;
        private PropertyBookingManager propertyBookingManager;

        public frmDashboard()
        {
            InitializeComponent();
            InitializeFirebase();

            listPropertyManager = new ListPropertyManager(firebaseClient);
            listedPropertyViewer = new ListedPropertyViewer(firebaseClient);
            propertyBookingManager = new PropertyBookingManager(firebaseClient);

            // Subscribe to the PropertySelected event
            propertyBookingManager.PropertySelected += PropertyBookingManager_PropertySelected;
        }

        public void InitializeFirebase()
        {
            firebaseClient = new FirebaseClient("https://airbnb-d4964-default-rtdb.europe-west1.firebasedatabase.app/");
        }

        private void frmListed_Load(object sender, EventArgs e)
        {
            usernameLabel.Text = GlobalData.Username;
            ShowPanel(panelHome);
        }

        private void listButton_Click(object sender, EventArgs e)
        {
            ShowPanel(panelList);
        }

        private async void bookButton_Click(object sender, EventArgs e)
        {
            ShowPanel(panelBook);

            var properties = await propertyBookingManager.GetAvailablePropertiesFromFirebase();

            if (properties != null && properties.Count > 0)
            {
                propertyBookingManager.DisplayAvailableProperties(properties, flowPanelBook);
            }
            else
            {
                MessageBox.Show("No available properties found.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private async void listedButton_Click(object sender, EventArgs e)
        {
            ShowPanel(panelListed);

            string username = GlobalData.Username;

            var imageUrls = await listedPropertyViewer.GetImageUrlsFromFirebase(username);

            if (imageUrls != null && imageUrls.Count > 0)
            {
                listedPropertyViewer.DisplayImages(imageUrls, flowPanelImages);
            }
            else
            {
                MessageBox.Show("No listed property images found.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            new frmLogin().Show();
            this.Close();
        }

        private void ShowPanel(Panel panel)
        {
            panelList.Visible = false;
            panelListed.Visible = false;
            panelBook.Visible = false;
            panelHome.Visible = false;
            searchPanel.Visible = false;
            panelPropertyDetails.Visible = false;
            panelFinalBook.Visible = false;

            panel.Visible = true;
            panel.BringToFront();
        }

        private async void uploadButton_Click(object sender, EventArgs e)
        {
            try
            {
                listPropertyManager.SelectFiles();
                await listPropertyManager.UploadProperty(
                    GlobalData.Username,
                    txtAddress.Text,
                    txtCity.Text,
                    txtTitle.Text,
                    txtPrice.Text,
                    txtDescription.Text
                );
                MessageBox.Show("Property details and images uploaded successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Action Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void frontImageButton_Click(object sender, EventArgs e)
        {
            await listPropertyManager.UploadFrontImage(GlobalData.Username, GlobalData.email);
            MessageBox.Show("Front image uploaded successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void searchLabel_Click(object sender, EventArgs e)
        {
            ShowPanel(searchPanel);
        }

        private async void searchButton_Click(object sender, EventArgs e)
        {
            string city = txtSearch.Text;
            var properties = await propertyBookingManager.SearchPropertiesByCity(city);

            if (properties != null && properties.Count > 0)
            {
                propertyBookingManager.DisplayAvailableProperties(properties, flowPanelSearch);
            }
            else
            {
                MessageBox.Show($"No properties found in {city}.", "Search Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private async void PropertyBookingManager_PropertySelected(object sender, Dictionary<string, object> propertyData)
        {
            try
            {
                // Get the complete address data
                var addressData = await firebaseClient
                    .Child("Available Properties")
                    .Child(propertyData["Username"].ToString())
                    .Child("Address")
                    .OnceSingleAsync<Dictionary<string, object>>();

                if (labelAddress != null)
                {
                    labelAddress.Text = $"Address: {addressData["Address"]}";
                }

                if (labelPricePerNight != null)
                {
                    labelPricePerNight.Text = $"Price per night: £{propertyData["PricePerNight"]}";
                }

                if (labelContact != null)
                {
                    labelContact.Text = $"Contact: {propertyData["Email"]}";
                }

                // Show details panel
                ShowPanel(panelPropertyDetails);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading property details: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            DisplaySelectedPropertyImages(sender, propertyData);
        }

        private async void DisplaySelectedPropertyImages(object sender, Dictionary<string, object> propertyData)
        {
            flowLayoutPanelImages.Controls.Clear();
            flowLayoutPanelImages.AutoScroll = true;
            flowLayoutPanelImages.WrapContents = true;
            flowLayoutPanelImages.FlowDirection = FlowDirection.LeftToRight;
            flowLayoutPanelImages.Padding = new Padding(10);

            // Get the property images
            var images = await firebaseClient
                .Child("Available Properties")
                .Child(propertyData["Username"].ToString())
                .Child("ImageUrls")
                .OnceSingleAsync<List<string>>();

            foreach (var image in images)
            {
                PictureBox propertyImage = new PictureBox
                {
                    Width = 350,
                    Height = 300,
                    Location = new Point(10, 10),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    BorderStyle = BorderStyle.FixedSingle
                };

                propertyImage.Load(image.ToString());

                flowLayoutPanelImages.Controls.Add(propertyImage);
            }
        }

        private void button_FinalBook_Click(object sender, EventArgs e)
        {
            ShowPanel(panelFinalBook);
        }

        private void bookingCalendar_DateSelected(object sender, DateRangeEventArgs e)
        {
            DateTime checkInDate = bookingCalendar.SelectionStart;
            DateTime checkOutDate = bookingCalendar.SelectionEnd;

            // Update labels
            labelCheckIn.Text = $"Check-in Date: {checkInDate:d}";
            labelCheckOut.Text = $"Check-out Date: {checkOutDate:d}";

            // Calculate total nights and price
            int totalNights = (checkOutDate - checkInDate).Days;

            // Get price from the previous screen
            decimal pricePerNight = 0;
            if (labelPricePerNight != null)
            {
                string priceText = labelPricePerNight.Text.Replace("Price per night: £", "");
                decimal.TryParse(priceText, out pricePerNight);
            }

            decimal totalPrice = totalNights * pricePerNight;

            labelTotalNights.Text = $"Total Nights: {totalNights}";
            labelTotalPrice.Text = $"Total Price: £{totalPrice:N2}";

            // Enable confirm button if dates are valid
            button_ConfirmBooking.Enabled = totalNights > 0;
        }
    }
}