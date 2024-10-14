using Firebase.Database;
using System;
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

            panel.Visible = true;
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
                propertyBookingManager.DisplayAvailableProperties(properties, flowPanelBook);
            }
            else
            {
                MessageBox.Show($"No properties found in {city}.", "Search Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}