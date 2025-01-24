using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Management;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace AirBnB
{
    public partial class frmDashboard : Form
    {
        private FirebaseClient firebaseClient;
        private ListPropertyManager listPropertyManager;
        private ListedPropertyViewer listedPropertyViewer;
        private PropertyBookingManager propertyBookingManager;
        private Dictionary<string, object> selectedPropertyData;
        private Dictionary<string, object> selectedReservationData;
        private PropertyReservationManager propertyReservationManager;
        private SelectedProperty selectedPropertyManager;
        private CityCardManager cityCardManager;

        // Constants for image padding and property card dimensions
        const int IMAGE_PADDING = 10;
        private const int PROPERTY_CARD_WIDTH = 350;
        private const int PROPERTY_CARD_HEIGHT = 300;

        public frmDashboard()
        {
            InitializeComponent();
            InitializeFirebase();

            // Initialize various managers and viewers
            listPropertyManager = new ListPropertyManager(firebaseClient);
            listedPropertyViewer = new ListedPropertyViewer(firebaseClient);
            propertyBookingManager = new PropertyBookingManager(firebaseClient);
            propertyReservationManager = new PropertyReservationManager(firebaseClient);
            selectedPropertyManager = new SelectedProperty(firebaseClient); ;
            cityCardManager = new CityCardManager(firebaseClient);

            // Apply rounded corners to various UI elements
            this.ApplyRoundedCornersToAll();
            txtCardNumber?.ApplyRoundedCorners(25);
            txtAddressLine2?.ApplyRoundedCorners(25);
            button_ConfirmBooking?.ApplyRoundedCorners(75);
            txtPrice?.ApplyRoundedCorners(25);
            txtDescription?.ApplyRoundedCorners(25);
            uploadButton?.ApplyRoundedCorners(75);
            searchButton?.ApplyRoundedCorners(25);
            buttonNext?.ApplyRoundedCorners(25);
            buttonPrevious?.ApplyRoundedCorners(25);

            // Disable confirm booking button initially
            button_ConfirmBooking.Enabled = false;

            // Subscribe to events
            propertyBookingManager.PropertySelected += PropertyBookingManager_PropertySelected;
            propertyReservationManager.ReservationSelected += PropertyReservationManager_ReservationSelected;
            propertyBookingManager.CitySelected += PropertyBookingManager_CitySelected;
        }

        // Initialize Firebase connection
        public void InitializeFirebase()
        {
            firebaseClient = new FirebaseClient("https://airbnb-d4964-default-rtdb.europe-west1.firebasedatabase.app/");
        }

        private void frmListed_Load(object sender, EventArgs e)
        {
            // Set username label and show home panel on form load
            usernameLabel.Text = GlobalData.Username;
            ShowPanel(panelHome);
        }

        private void listButton_Click(object sender, EventArgs e)
        {
            // Show list property panel
            ShowPanel(panelList);
        }

        private async void bookButton_Click(object sender, EventArgs e)
        {
            // Show cities panel and display cities
            ShowPanel(panelCities);
            await propertyBookingManager.DisplayCitiesPanel(flowLayoutCities, buttonPrevious, buttonNext);
        }

        private async void listedButton_Click(object sender, EventArgs e)
        {
            // Show listed properties panel and display user's listed properties
            ShowPanel(panelListed);
            string username = GlobalData.Username;
            string imageUrl = await listedPropertyViewer.GetImageUrlFromFirebase(username);
            if (imageUrl != null)
            {
                listedPropertyViewer.DisplayImages(imageUrl, flowPanelImages);
            }
            else
            {
                MessageBox.Show("No listed property images found.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Show login form and hide dashboard
            new frmLogin().Show();
            this.Hide();
        }

        private void ShowPanel(Panel panel)
        {
            // Bring the specified panel to front
            panel.BringToFront();
        }

        private async void uploadButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Select files and upload property details
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
            // Upload front image for the property
            await listPropertyManager.UploadFrontImage(GlobalData.Username, GlobalData.email);
            MessageBox.Show("Front image uploaded successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void searchLabel_Click(object sender, EventArgs e)
        {
            // Show search panel
            ShowPanel(searchPanel);
        }

        private async void searchButton_Click(object sender, EventArgs e)
        {
            // Search properties by city and display results
            string city = txtSearch.Text;
            var properties = await propertyBookingManager.SearchPropertiesByCity(city);
            if (properties != null && properties.Count > 0)
            {
                await propertyBookingManager.DisplayAvailableProperties(properties, flowPanelSearch);
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
                // Handle property selection and display property details
                selectedPropertyData = propertyData;
                var addressData = await selectedPropertyManager.GetPropertyAddress(propertyData["Username"].ToString());
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

                // Reset booking information
                bookingCalendar.SelectionStart = bookingCalendar.SelectionEnd = DateTime.Today;
                labelCheckIn.Text = "Check-in Date: Not selected";
                labelCheckOut.Text = "Check-out Date: Not selected";
                labelTotalNights.Text = "Total Nights: 0";
                labelTotalPrice.Text = "Total Price: £0.00";
                button_ConfirmBooking.Enabled = false;

                ShowPanel(panelPropertyDetails);
                await selectedPropertyManager.DisplayPropertyDetails(propertyData, flowLayoutPanelImages);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading property details: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button_FinalBook_Click(object sender, EventArgs e)
        {
            // Show final booking panel
            ShowPanel(panelFinalBook);
        }

        private void bookingCalendar_DateSelected(object sender, DateRangeEventArgs e)
        {
            // Handle booking date selection and update booking information
            DateTime checkInDate = bookingCalendar.SelectionStart;
            DateTime checkOutDate = bookingCalendar.SelectionEnd;

            // Validate check-in date
            if (checkInDate < DateTime.Today)
            {
                MessageBox.Show("Please select a check-in date from today onwards.",
                    "Invalid Check-in Date",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                bookingCalendar.SetSelectionRange(DateTime.Today, DateTime.Today);
                ClearBookingInformation();
                return;
            }

            // Calculate total nights
            int totalNights = (checkOutDate - checkInDate).Days;

            // Validate stay duration
            if (totalNights < 1)
            {
                ClearBookingInformation();
                return;
            }

            // Calculate total price
            decimal pricePerNight = 0;
            if (labelPricePerNight != null)
            {
                string priceText = labelPricePerNight.Text.Replace("Price per night: £", "");
                decimal.TryParse(priceText, out pricePerNight);
            }
            decimal totalPrice = totalNights * pricePerNight;

            // Update booking information labels
            labelCheckIn.Text = $"Check-in Date: {checkInDate:d}";
            labelCheckOut.Text = $"Check-out Date: {checkOutDate:d}";
            labelTotalNights.Text = $"Total Nights: {totalNights}";
            labelTotalPrice.Text = $"Total Price: £{totalPrice:N2}";

            // Enable booking button
            button_ConfirmBooking.Enabled = true;
        }

        private void ClearBookingInformation()
        {
            // Clear booking information labels and disable booking button
            labelCheckIn.Text = "Check-in Date: Not selected";
            labelCheckOut.Text = "Check-out Date: Not selected";
            labelTotalNights.Text = "Total Nights: 0";
            labelTotalPrice.Text = "Total Price: £0.00";
            button_ConfirmBooking.Enabled = false;
        }

        private void button_ConfirmBooking_Click(object sender, EventArgs e)
        {
            // Show payment panel
            ShowPanel(panelPayment);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            // Show home panel
            ShowPanel(panelHome);
        }

        private void usernameLabel_Click(object sender, EventArgs e)
        {
            // Show home panel
            ShowPanel(panelHome);
        }

        private async void buttonReservations_Click(object sender, EventArgs e)
        {
            // Show reservations panel and display user's reservations
            string username = GlobalData.Username;
            ShowPanel(panelReservations);
            var reservations = await propertyReservationManager.RetrieveUserReservationDetails(username);
            if (reservations.Count != 0)
            {
                await propertyReservationManager.DisplayUserReservations(reservations, flowPanelReservations);
            }
            else
            {
                MessageBox.Show("No reservations found", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void PropertyReservationManager_ReservationSelected(object sender, Dictionary<string, object> reservationData)
        {
            // Handle reservation selection and display reservation details
            selectedReservationData = reservationData;
            if (labelResAddress != null)
            {
                labelResAddress.Text = $"Address: {reservationData["address"]}";
            }
            if (labelResCheckIn != null)
            {
                labelResCheckIn.Text = $"Check-in Date: {reservationData["startDate"]}";
            }
            if (labelResCheckOut != null)
            {
                labelResCheckOut.Text = $"Check-out Date: {reservationData["endDate"]}";
            }
            if (labelResTotalNights != null)
            {
                labelResTotalNights.Text = $"Total Nights: {reservationData["nights"]}";
            }
            if (labelResTotalPrice != null)
            {
                decimal pricePerNight = decimal.Parse(reservationData["pricePerNight"].ToString());
                int nights = int.Parse(reservationData["nights"].ToString());
                decimal totalPrice = pricePerNight * nights;
                labelResTotalPrice.Text = $"Total Price: £{totalPrice}";
            }
            ShowPanel(panelSelectedReservation);
            await selectedPropertyManager.DisplayPropertyDetails(reservationData, flowPanelSelectedResevation);
        }

        private async void buttonCancelReservation_Click(object sender, EventArgs e)
        {
            // Handle reservation cancellation
            DialogResult result = MessageBox.Show(
                "Are you sure you want to cancel this reservation?",
                "Confirm Cancellation",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );
            if (result == DialogResult.Yes)
            {
                await propertyReservationManager.CancelReservation(selectedReservationData);
                buttonReservations_Click(sender, e);
            }
        }

        private async void buttonPay_Click(object sender, EventArgs e)
        {
            try
            {
                // Process payment and create reservation
                var paymentManager = new PaymentManager();
                var paymentDetails = new PaymentDetails
                {
                    FullName = txtFullName.Text,
                    CardNumber = txtCardNumber.Text.Replace(" ", ""),
                    ExpiryDate = txtExpiryDate.Text,
                    CVV = txtCVV.Text,
                    AddressLine1 = txtAddressLine1.Text,
                    AddressLine2 = txtAddressLine2.Text,
                    City = txtCiti.Text,
                    PostCode = txtPostCode.Text
                };
                string priceText = labelTotalPrice.Text.Replace("Total Price: £", "");
                if (!decimal.TryParse(priceText, out decimal totalAmount))
                {
                    MessageBox.Show("Invalid price amount.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                paymentManager.ProcessPayment(paymentDetails, totalAmount);
                MessageBox.Show("Payment processed successfully. Reservation successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                if (selectedPropertyData == null)
                {
                    MessageBox.Show("Property data is missing.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                DateTime checkInDate = bookingCalendar.SelectionStart.Date;
                DateTime checkOutDate = bookingCalendar.SelectionEnd.Date;
                string strCheckInDate = checkInDate.ToString("dd/MM/yyyy");
                string strCheckOutDate = checkOutDate.ToString("dd/MM/yyyy");
                string username = GlobalData.Username;
                int totalNights = (checkOutDate - checkInDate).Days;
                var propertyData = await firebaseClient
                    .Child("Available Properties")
                    .Child(selectedPropertyData["Username"].ToString())
                    .OnceSingleAsync<Dictionary<string, object>>();
                var addressData = await firebaseClient
                    .Child("Available Properties")
                    .Child(selectedPropertyData["Username"].ToString())
                    .Child("Address")
                    .OnceSingleAsync<Dictionary<string, object>>();
                selectedPropertyManager.AddReservationToDatabase(username, strCheckOutDate, totalNights, strCheckInDate, propertyData, addressData);
                ShowPanel(panelHome);
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while processing the payment: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private async void PropertyBookingManager_CitySelected(object sender, string city)
        {
            // Handle city selection and display properties in the selected city
            ShowPanel(panelBook);
            try
            {
                var properties = await propertyBookingManager.SearchPropertiesByCity(city);
                if (properties != null && properties.Count > 0)
                {
                    await propertyBookingManager.DisplayAvailableProperties(
                        properties,
                        flowPanelBook
                    );
                }
                else
                {
                    MessageBox.Show($"No properties found in {city}.", "Search Result",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading properties: {ex.Message}", "Error",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void labelSearchCities_Click(object sender, EventArgs e)
        {
            // Show search panel
            ShowPanel(searchPanel);
        }

        private void buttonPrevious_Click(object sender, EventArgs e)
        {
            // Navigate to previous page of city cards
            propertyBookingManager.CityCardManager.PreviousPage();
        }

        private void buttonNext_Click(object sender, EventArgs e)
        {
            // Navigate to next page of city cards
            propertyBookingManager.CityCardManager.NextPage();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Exit the application when the form is closing
            base.OnFormClosing(e);
            if (e.CloseReason == CloseReason.UserClosing)
            {
                Application.Exit();
            }
        }
    }
}