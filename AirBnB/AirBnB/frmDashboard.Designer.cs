namespace AirBnB
{
    partial class frmDashboard
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmDashboard));
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.buttonReservations = new System.Windows.Forms.Button();
            this.logoutButton = new System.Windows.Forms.Button();
            this.listedButton = new System.Windows.Forms.Button();
            this.listButton = new System.Windows.Forms.Button();
            this.bookButton = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.usernameLabel = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.panelListed = new System.Windows.Forms.Panel();
            this.flowPanelImages = new System.Windows.Forms.FlowLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.panelHome = new System.Windows.Forms.Panel();
            this.panelBook = new System.Windows.Forms.Panel();
            this.searchLabel = new System.Windows.Forms.Label();
            this.flowPanelBook = new System.Windows.Forms.FlowLayoutPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.panelList = new System.Windows.Forms.Panel();
            this.frontImageButton = new System.Windows.Forms.Button();
            this.txtTitle = new System.Windows.Forms.TextBox();
            this.txtPrice = new System.Windows.Forms.TextBox();
            this.txtCity = new System.Windows.Forms.TextBox();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.txtAddress = new System.Windows.Forms.TextBox();
            this.uploadButton = new System.Windows.Forms.Button();
            this.labelTitle = new System.Windows.Forms.Label();
            this.labelPrice = new System.Windows.Forms.Label();
            this.labelCity = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.labelDescription = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.searchPanel = new System.Windows.Forms.Panel();
            this.flowPanelSearch = new System.Windows.Forms.FlowLayoutPanel();
            this.searchButton = new System.Windows.Forms.Button();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.panelPropertyDetails = new System.Windows.Forms.Panel();
            this.flowLayoutPanelImages = new System.Windows.Forms.FlowLayoutPanel();
            this.labelContact = new System.Windows.Forms.Label();
            this.labelPricePerNight = new System.Windows.Forms.Label();
            this.button_FinalBook = new System.Windows.Forms.Button();
            this.labelAddress = new System.Windows.Forms.Label();
            this.panelFinalBook = new System.Windows.Forms.Panel();
            this.button_ConfirmBooking = new System.Windows.Forms.Button();
            this.labelTotalPrice = new System.Windows.Forms.Label();
            this.labelTotalNights = new System.Windows.Forms.Label();
            this.labelCheckOut = new System.Windows.Forms.Label();
            this.labelCheckIn = new System.Windows.Forms.Label();
            this.bookingCalendar = new System.Windows.Forms.MonthCalendar();
            this.panelReservations = new System.Windows.Forms.Panel();
            this.flowPanelReservations = new System.Windows.Forms.FlowLayoutPanel();
            this.label9 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panelListed.SuspendLayout();
            this.panelBook.SuspendLayout();
            this.panelList.SuspendLayout();
            this.searchPanel.SuspendLayout();
            this.panelPropertyDetails.SuspendLayout();
            this.panelFinalBook.SuspendLayout();
            this.panelReservations.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(90)))), ((int)(((byte)(95)))));
            this.panel1.Controls.Add(this.panel3);
            this.panel1.Controls.Add(this.buttonReservations);
            this.panel1.Controls.Add(this.logoutButton);
            this.panel1.Controls.Add(this.listedButton);
            this.panel1.Controls.Add(this.listButton);
            this.panel1.Controls.Add(this.bookButton);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(300, 855);
            this.panel1.TabIndex = 0;
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.SeaShell;
            this.panel3.Location = new System.Drawing.Point(0, 194);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(5, 320);
            this.panel3.TabIndex = 2;
            // 
            // buttonReservations
            // 
            this.buttonReservations.BackColor = System.Drawing.Color.White;
            this.buttonReservations.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonReservations.Dock = System.Windows.Forms.DockStyle.Top;
            this.buttonReservations.FlatAppearance.BorderSize = 0;
            this.buttonReservations.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonReservations.Font = new System.Drawing.Font("Nirmala UI", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonReservations.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(56)))), ((int)(((byte)(92)))));
            this.buttonReservations.Image = global::AirBnB.Properties.Resources.reservation_completed_icon3;
            this.buttonReservations.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonReservations.Location = new System.Drawing.Point(0, 434);
            this.buttonReservations.Name = "buttonReservations";
            this.buttonReservations.Size = new System.Drawing.Size(300, 80);
            this.buttonReservations.TabIndex = 3;
            this.buttonReservations.Text = "Reservations";
            this.buttonReservations.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonReservations.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.buttonReservations.UseVisualStyleBackColor = false;
            this.buttonReservations.Click += new System.EventHandler(this.buttonReservations_Click);
            // 
            // logoutButton
            // 
            this.logoutButton.BackColor = System.Drawing.Color.White;
            this.logoutButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.logoutButton.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.logoutButton.FlatAppearance.BorderSize = 0;
            this.logoutButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.logoutButton.Font = new System.Drawing.Font("Nirmala UI", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.logoutButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(56)))), ((int)(((byte)(92)))));
            this.logoutButton.Location = new System.Drawing.Point(0, 775);
            this.logoutButton.Name = "logoutButton";
            this.logoutButton.Size = new System.Drawing.Size(300, 80);
            this.logoutButton.TabIndex = 1;
            this.logoutButton.Text = "Logout";
            this.logoutButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.logoutButton.UseVisualStyleBackColor = false;
            this.logoutButton.Click += new System.EventHandler(this.button3_Click);
            // 
            // listedButton
            // 
            this.listedButton.BackColor = System.Drawing.Color.White;
            this.listedButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.listedButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.listedButton.FlatAppearance.BorderSize = 0;
            this.listedButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.listedButton.Font = new System.Drawing.Font("Nirmala UI", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listedButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(56)))), ((int)(((byte)(92)))));
            this.listedButton.Image = global::AirBnB.Properties.Resources.icons8_house_64;
            this.listedButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.listedButton.Location = new System.Drawing.Point(0, 354);
            this.listedButton.Name = "listedButton";
            this.listedButton.Size = new System.Drawing.Size(300, 80);
            this.listedButton.TabIndex = 1;
            this.listedButton.Text = "Listed Property";
            this.listedButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.listedButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.listedButton.UseVisualStyleBackColor = false;
            this.listedButton.Click += new System.EventHandler(this.listedButton_Click);
            // 
            // listButton
            // 
            this.listButton.BackColor = System.Drawing.Color.White;
            this.listButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.listButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.listButton.FlatAppearance.BorderSize = 0;
            this.listButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.listButton.Font = new System.Drawing.Font("Nirmala UI", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(56)))), ((int)(((byte)(92)))));
            this.listButton.Image = global::AirBnB.Properties.Resources.icons8_key_64;
            this.listButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.listButton.Location = new System.Drawing.Point(0, 274);
            this.listButton.Name = "listButton";
            this.listButton.Size = new System.Drawing.Size(300, 80);
            this.listButton.TabIndex = 1;
            this.listButton.Text = "List Property";
            this.listButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.listButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.listButton.UseVisualStyleBackColor = false;
            this.listButton.Click += new System.EventHandler(this.listButton_Click);
            // 
            // bookButton
            // 
            this.bookButton.BackColor = System.Drawing.Color.White;
            this.bookButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.bookButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.bookButton.FlatAppearance.BorderSize = 0;
            this.bookButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bookButton.Font = new System.Drawing.Font("Nirmala UI", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bookButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(56)))), ((int)(((byte)(92)))));
            this.bookButton.Image = global::AirBnB.Properties.Resources.icons8_leasing_60;
            this.bookButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.bookButton.Location = new System.Drawing.Point(0, 194);
            this.bookButton.Name = "bookButton";
            this.bookButton.Size = new System.Drawing.Size(300, 80);
            this.bookButton.TabIndex = 1;
            this.bookButton.Text = "Book Property";
            this.bookButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.bookButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.bookButton.UseVisualStyleBackColor = false;
            this.bookButton.Click += new System.EventHandler(this.bookButton_Click);
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(90)))), ((int)(((byte)(95)))));
            this.panel2.Controls.Add(this.usernameLabel);
            this.panel2.Controls.Add(this.pictureBox1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(300, 194);
            this.panel2.TabIndex = 0;
            // 
            // usernameLabel
            // 
            this.usernameLabel.AutoSize = true;
            this.usernameLabel.BackColor = System.Drawing.Color.Transparent;
            this.usernameLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.usernameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.usernameLabel.ForeColor = System.Drawing.Color.SeaShell;
            this.usernameLabel.Location = new System.Drawing.Point(80, 12);
            this.usernameLabel.Name = "usernameLabel";
            this.usernameLabel.Size = new System.Drawing.Size(97, 24);
            this.usernameLabel.TabIndex = 1;
            this.usernameLabel.Text = "Username";
            this.usernameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.usernameLabel.Click += new System.EventHandler(this.usernameLabel_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pictureBox1.Image = global::AirBnB.Properties.Resources._CITYPNG_COM_White_User_Member_Guest_Icon_PNG_Image___4000x4000;
            this.pictureBox1.Location = new System.Drawing.Point(12, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(62, 65);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // panelListed
            // 
            this.panelListed.BackColor = System.Drawing.Color.White;
            this.panelListed.BackgroundImage = global::AirBnB.Properties.Resources.AdobeStock_288768256_2mb;
            this.panelListed.Controls.Add(this.flowPanelImages);
            this.panelListed.Controls.Add(this.label1);
            this.panelListed.Location = new System.Drawing.Point(300, 0);
            this.panelListed.Name = "panelListed";
            this.panelListed.Size = new System.Drawing.Size(841, 855);
            this.panelListed.TabIndex = 1;
            // 
            // flowPanelImages
            // 
            this.flowPanelImages.BackColor = System.Drawing.Color.Transparent;
            this.flowPanelImages.Location = new System.Drawing.Point(6, 92);
            this.flowPanelImages.Name = "flowPanelImages";
            this.flowPanelImages.Size = new System.Drawing.Size(835, 763);
            this.flowPanelImages.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Font = new System.Drawing.Font("Nirmala UI", 26.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(56)))), ((int)(((byte)(92)))));
            this.label1.Location = new System.Drawing.Point(24, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(401, 47);
            this.label1.TabIndex = 0;
            this.label1.Text = "Your Property Pictures:";
            // 
            // panelHome
            // 
            this.panelHome.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panelHome.BackColor = System.Drawing.Color.SeaShell;
            this.panelHome.BackgroundImage = global::AirBnB.Properties.Resources.airbnb_logo;
            this.panelHome.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.panelHome.Location = new System.Drawing.Point(300, 0);
            this.panelHome.Name = "panelHome";
            this.panelHome.Size = new System.Drawing.Size(841, 855);
            this.panelHome.TabIndex = 2;
            // 
            // panelBook
            // 
            this.panelBook.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panelBook.BackColor = System.Drawing.Color.Transparent;
            this.panelBook.Controls.Add(this.searchLabel);
            this.panelBook.Controls.Add(this.flowPanelBook);
            this.panelBook.Controls.Add(this.label2);
            this.panelBook.Location = new System.Drawing.Point(300, 0);
            this.panelBook.Name = "panelBook";
            this.panelBook.Size = new System.Drawing.Size(841, 855);
            this.panelBook.TabIndex = 3;
            // 
            // searchLabel
            // 
            this.searchLabel.AutoSize = true;
            this.searchLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.searchLabel.Font = new System.Drawing.Font("Nirmala UI", 26.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.searchLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(56)))), ((int)(((byte)(92)))));
            this.searchLabel.Location = new System.Drawing.Point(542, 43);
            this.searchLabel.Name = "searchLabel";
            this.searchLabel.Size = new System.Drawing.Size(156, 47);
            this.searchLabel.TabIndex = 2;
            this.searchLabel.Text = "SEARCH";
            this.searchLabel.Click += new System.EventHandler(this.searchLabel_Click);
            // 
            // flowPanelBook
            // 
            this.flowPanelBook.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowPanelBook.Location = new System.Drawing.Point(0, 89);
            this.flowPanelBook.Name = "flowPanelBook";
            this.flowPanelBook.Size = new System.Drawing.Size(835, 763);
            this.flowPanelBook.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Nirmala UI", 26.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Black;
            this.label2.Location = new System.Drawing.Point(29, 39);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(352, 47);
            this.label2.TabIndex = 0;
            this.label2.Text = "Available Properties";
            // 
            // panelList
            // 
            this.panelList.BackColor = System.Drawing.Color.Transparent;
            this.panelList.BackgroundImage = global::AirBnB.Properties.Resources.image;
            this.panelList.Controls.Add(this.frontImageButton);
            this.panelList.Controls.Add(this.txtTitle);
            this.panelList.Controls.Add(this.txtPrice);
            this.panelList.Controls.Add(this.txtCity);
            this.panelList.Controls.Add(this.txtDescription);
            this.panelList.Controls.Add(this.txtAddress);
            this.panelList.Controls.Add(this.uploadButton);
            this.panelList.Controls.Add(this.labelTitle);
            this.panelList.Controls.Add(this.labelPrice);
            this.panelList.Controls.Add(this.labelCity);
            this.panelList.Controls.Add(this.label7);
            this.panelList.Controls.Add(this.labelDescription);
            this.panelList.Controls.Add(this.label6);
            this.panelList.Controls.Add(this.label5);
            this.panelList.Controls.Add(this.label4);
            this.panelList.Controls.Add(this.label3);
            this.panelList.Location = new System.Drawing.Point(300, 0);
            this.panelList.Name = "panelList";
            this.panelList.Size = new System.Drawing.Size(841, 855);
            this.panelList.TabIndex = 4;
            // 
            // frontImageButton
            // 
            this.frontImageButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(56)))), ((int)(((byte)(92)))));
            this.frontImageButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.frontImageButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.frontImageButton.FlatAppearance.BorderSize = 0;
            this.frontImageButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.frontImageButton.Font = new System.Drawing.Font("Nirmala UI", 26.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.frontImageButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(223)))), ((int)(((byte)(204)))));
            this.frontImageButton.Location = new System.Drawing.Point(465, 733);
            this.frontImageButton.Name = "frontImageButton";
            this.frontImageButton.Size = new System.Drawing.Size(320, 75);
            this.frontImageButton.TabIndex = 4;
            this.frontImageButton.Text = "UPLOAD";
            this.frontImageButton.UseVisualStyleBackColor = false;
            this.frontImageButton.Click += new System.EventHandler(this.frontImageButton_Click);
            // 
            // txtTitle
            // 
            this.txtTitle.BackColor = System.Drawing.Color.White;
            this.txtTitle.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtTitle.Font = new System.Drawing.Font("MS UI Gothic", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTitle.Location = new System.Drawing.Point(32, 387);
            this.txtTitle.Multiline = true;
            this.txtTitle.Name = "txtTitle";
            this.txtTitle.Size = new System.Drawing.Size(220, 32);
            this.txtTitle.TabIndex = 3;
            // 
            // txtPrice
            // 
            this.txtPrice.BackColor = System.Drawing.Color.White;
            this.txtPrice.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtPrice.Font = new System.Drawing.Font("MS UI Gothic", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPrice.Location = new System.Drawing.Point(32, 455);
            this.txtPrice.Multiline = true;
            this.txtPrice.Name = "txtPrice";
            this.txtPrice.Size = new System.Drawing.Size(220, 32);
            this.txtPrice.TabIndex = 3;
            // 
            // txtCity
            // 
            this.txtCity.BackColor = System.Drawing.Color.White;
            this.txtCity.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtCity.Font = new System.Drawing.Font("MS UI Gothic", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCity.Location = new System.Drawing.Point(32, 524);
            this.txtCity.Multiline = true;
            this.txtCity.Name = "txtCity";
            this.txtCity.Size = new System.Drawing.Size(220, 32);
            this.txtCity.TabIndex = 3;
            // 
            // txtDescription
            // 
            this.txtDescription.BackColor = System.Drawing.Color.White;
            this.txtDescription.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtDescription.Font = new System.Drawing.Font("MS UI Gothic", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtDescription.Location = new System.Drawing.Point(32, 592);
            this.txtDescription.Multiline = true;
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(529, 32);
            this.txtDescription.TabIndex = 3;
            // 
            // txtAddress
            // 
            this.txtAddress.BackColor = System.Drawing.Color.White;
            this.txtAddress.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtAddress.Font = new System.Drawing.Font("MS UI Gothic", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtAddress.Location = new System.Drawing.Point(32, 660);
            this.txtAddress.Multiline = true;
            this.txtAddress.Name = "txtAddress";
            this.txtAddress.Size = new System.Drawing.Size(529, 32);
            this.txtAddress.TabIndex = 3;
            // 
            // uploadButton
            // 
            this.uploadButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(56)))), ((int)(((byte)(92)))));
            this.uploadButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.uploadButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.uploadButton.FlatAppearance.BorderColor = System.Drawing.Color.Sienna;
            this.uploadButton.FlatAppearance.BorderSize = 0;
            this.uploadButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.uploadButton.Font = new System.Drawing.Font("Nirmala UI", 26.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.uploadButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(223)))), ((int)(((byte)(204)))));
            this.uploadButton.Location = new System.Drawing.Point(32, 733);
            this.uploadButton.Name = "uploadButton";
            this.uploadButton.Size = new System.Drawing.Size(320, 75);
            this.uploadButton.TabIndex = 1;
            this.uploadButton.Text = "UPLOAD";
            this.uploadButton.UseVisualStyleBackColor = false;
            this.uploadButton.Click += new System.EventHandler(this.uploadButton_Click);
            // 
            // labelTitle
            // 
            this.labelTitle.AutoSize = true;
            this.labelTitle.Font = new System.Drawing.Font("Nirmala UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTitle.ForeColor = System.Drawing.SystemColors.GrayText;
            this.labelTitle.Location = new System.Drawing.Point(27, 354);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Size = new System.Drawing.Size(56, 30);
            this.labelTitle.TabIndex = 0;
            this.labelTitle.Text = "Title";
            // 
            // labelPrice
            // 
            this.labelPrice.AutoSize = true;
            this.labelPrice.Font = new System.Drawing.Font("Nirmala UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelPrice.ForeColor = System.Drawing.SystemColors.GrayText;
            this.labelPrice.Location = new System.Drawing.Point(27, 422);
            this.labelPrice.Name = "labelPrice";
            this.labelPrice.Size = new System.Drawing.Size(158, 30);
            this.labelPrice.TabIndex = 0;
            this.labelPrice.Text = "Price per night";
            // 
            // labelCity
            // 
            this.labelCity.AutoSize = true;
            this.labelCity.Font = new System.Drawing.Font("Nirmala UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelCity.ForeColor = System.Drawing.SystemColors.GrayText;
            this.labelCity.Location = new System.Drawing.Point(27, 491);
            this.labelCity.Name = "labelCity";
            this.labelCity.Size = new System.Drawing.Size(51, 30);
            this.labelCity.TabIndex = 0;
            this.labelCity.Text = "City";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Nirmala UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.ForeColor = System.Drawing.SystemColors.GrayText;
            this.label7.Location = new System.Drawing.Point(460, 702);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(133, 30);
            this.label7.TabIndex = 0;
            this.label7.Text = "Front Image";
            // 
            // labelDescription
            // 
            this.labelDescription.AutoSize = true;
            this.labelDescription.Font = new System.Drawing.Font("Nirmala UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDescription.ForeColor = System.Drawing.SystemColors.GrayText;
            this.labelDescription.Location = new System.Drawing.Point(27, 559);
            this.labelDescription.Name = "labelDescription";
            this.labelDescription.Size = new System.Drawing.Size(125, 30);
            this.labelDescription.TabIndex = 0;
            this.labelDescription.Text = "Description";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Nirmala UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.SystemColors.GrayText;
            this.label6.Location = new System.Drawing.Point(32, 702);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(116, 30);
            this.label6.TabIndex = 0;
            this.label6.Text = "All Images";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Nirmala UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.SystemColors.GrayText;
            this.label5.Location = new System.Drawing.Point(27, 627);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(91, 30);
            this.label5.TabIndex = 0;
            this.label5.Text = "Address";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Nirmala UI", 26.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(56)))), ((int)(((byte)(92)))));
            this.label4.Location = new System.Drawing.Point(29, 107);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(214, 47);
            this.label4.TabIndex = 0;
            this.label4.Text = "images of it";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Nirmala UI", 26.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(56)))), ((int)(((byte)(92)))));
            this.label3.Location = new System.Drawing.Point(29, 60);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(691, 47);
            this.label3.TabIndex = 0;
            this.label3.Text = "Enter your property address and upload ";
            // 
            // searchPanel
            // 
            this.searchPanel.BackgroundImage = global::AirBnB.Properties.Resources.AdobeStock_288768256_2mb;
            this.searchPanel.Controls.Add(this.flowPanelSearch);
            this.searchPanel.Controls.Add(this.searchButton);
            this.searchPanel.Controls.Add(this.txtSearch);
            this.searchPanel.Controls.Add(this.label8);
            this.searchPanel.Location = new System.Drawing.Point(300, 0);
            this.searchPanel.Name = "searchPanel";
            this.searchPanel.Size = new System.Drawing.Size(840, 862);
            this.searchPanel.TabIndex = 5;
            // 
            // flowPanelSearch
            // 
            this.flowPanelSearch.BackColor = System.Drawing.Color.Transparent;
            this.flowPanelSearch.Location = new System.Drawing.Point(0, 164);
            this.flowPanelSearch.Name = "flowPanelSearch";
            this.flowPanelSearch.Size = new System.Drawing.Size(838, 690);
            this.flowPanelSearch.TabIndex = 6;
            // 
            // searchButton
            // 
            this.searchButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(56)))), ((int)(((byte)(92)))));
            this.searchButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.searchButton.FlatAppearance.BorderSize = 0;
            this.searchButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.searchButton.Font = new System.Drawing.Font("Nirmala UI", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.searchButton.ForeColor = System.Drawing.Color.SeaShell;
            this.searchButton.Image = global::AirBnB.Properties.Resources.search_13_32;
            this.searchButton.Location = new System.Drawing.Point(606, 93);
            this.searchButton.Name = "searchButton";
            this.searchButton.Size = new System.Drawing.Size(49, 32);
            this.searchButton.TabIndex = 5;
            this.searchButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.searchButton.UseVisualStyleBackColor = false;
            this.searchButton.Click += new System.EventHandler(this.searchButton_Click);
            // 
            // txtSearch
            // 
            this.txtSearch.BackColor = System.Drawing.Color.White;
            this.txtSearch.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtSearch.Font = new System.Drawing.Font("MS UI Gothic", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSearch.Location = new System.Drawing.Point(37, 93);
            this.txtSearch.Multiline = true;
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(574, 32);
            this.txtSearch.TabIndex = 4;
            // 
            // label8
            // 
            this.label8.Font = new System.Drawing.Font("Nirmala UI", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(56)))), ((int)(((byte)(92)))));
            this.label8.Location = new System.Drawing.Point(35, 43);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(150, 55);
            this.label8.TabIndex = 0;
            this.label8.Text = "SEARCH";
            // 
            // panelPropertyDetails
            // 
            this.panelPropertyDetails.BackColor = System.Drawing.Color.SeaShell;
            this.panelPropertyDetails.Controls.Add(this.flowLayoutPanelImages);
            this.panelPropertyDetails.Controls.Add(this.labelContact);
            this.panelPropertyDetails.Controls.Add(this.labelPricePerNight);
            this.panelPropertyDetails.Controls.Add(this.button_FinalBook);
            this.panelPropertyDetails.Controls.Add(this.labelAddress);
            this.panelPropertyDetails.Location = new System.Drawing.Point(300, 0);
            this.panelPropertyDetails.Name = "panelPropertyDetails";
            this.panelPropertyDetails.Size = new System.Drawing.Size(840, 855);
            this.panelPropertyDetails.TabIndex = 6;
            // 
            // flowLayoutPanelImages
            // 
            this.flowLayoutPanelImages.Location = new System.Drawing.Point(0, 107);
            this.flowLayoutPanelImages.Name = "flowLayoutPanelImages";
            this.flowLayoutPanelImages.Size = new System.Drawing.Size(839, 427);
            this.flowLayoutPanelImages.TabIndex = 5;
            // 
            // labelContact
            // 
            this.labelContact.AutoSize = true;
            this.labelContact.BackColor = System.Drawing.Color.Transparent;
            this.labelContact.Font = new System.Drawing.Font("Nirmala UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelContact.ForeColor = System.Drawing.Color.Black;
            this.labelContact.Location = new System.Drawing.Point(38, 589);
            this.labelContact.Name = "labelContact";
            this.labelContact.Size = new System.Drawing.Size(81, 25);
            this.labelContact.TabIndex = 4;
            this.labelContact.Text = "Contact";
            this.labelContact.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelPricePerNight
            // 
            this.labelPricePerNight.AutoSize = true;
            this.labelPricePerNight.BackColor = System.Drawing.Color.Transparent;
            this.labelPricePerNight.Font = new System.Drawing.Font("Nirmala UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelPricePerNight.ForeColor = System.Drawing.Color.Black;
            this.labelPricePerNight.Location = new System.Drawing.Point(38, 627);
            this.labelPricePerNight.Name = "labelPricePerNight";
            this.labelPricePerNight.Size = new System.Drawing.Size(145, 25);
            this.labelPricePerNight.TabIndex = 4;
            this.labelPricePerNight.Text = "Price Per Night";
            this.labelPricePerNight.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // button_FinalBook
            // 
            this.button_FinalBook.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(90)))), ((int)(((byte)(95)))));
            this.button_FinalBook.Cursor = System.Windows.Forms.Cursors.Hand;
            this.button_FinalBook.FlatAppearance.BorderSize = 0;
            this.button_FinalBook.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_FinalBook.Font = new System.Drawing.Font("Nirmala UI", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_FinalBook.ForeColor = System.Drawing.Color.White;
            this.button_FinalBook.Location = new System.Drawing.Point(509, 735);
            this.button_FinalBook.Name = "button_FinalBook";
            this.button_FinalBook.Size = new System.Drawing.Size(300, 80);
            this.button_FinalBook.TabIndex = 2;
            this.button_FinalBook.Text = "Book";
            this.button_FinalBook.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.button_FinalBook.UseVisualStyleBackColor = false;
            this.button_FinalBook.Click += new System.EventHandler(this.button_FinalBook_Click);
            // 
            // labelAddress
            // 
            this.labelAddress.AutoSize = true;
            this.labelAddress.BackColor = System.Drawing.Color.Transparent;
            this.labelAddress.Font = new System.Drawing.Font("Nirmala UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelAddress.ForeColor = System.Drawing.Color.Black;
            this.labelAddress.Location = new System.Drawing.Point(39, 664);
            this.labelAddress.Name = "labelAddress";
            this.labelAddress.Size = new System.Drawing.Size(83, 25);
            this.labelAddress.TabIndex = 3;
            this.labelAddress.Text = "Address";
            this.labelAddress.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panelFinalBook
            // 
            this.panelFinalBook.BackColor = System.Drawing.Color.SeaShell;
            this.panelFinalBook.Controls.Add(this.button_ConfirmBooking);
            this.panelFinalBook.Controls.Add(this.labelTotalPrice);
            this.panelFinalBook.Controls.Add(this.labelTotalNights);
            this.panelFinalBook.Controls.Add(this.labelCheckOut);
            this.panelFinalBook.Controls.Add(this.labelCheckIn);
            this.panelFinalBook.Controls.Add(this.bookingCalendar);
            this.panelFinalBook.Location = new System.Drawing.Point(300, 0);
            this.panelFinalBook.Name = "panelFinalBook";
            this.panelFinalBook.Size = new System.Drawing.Size(841, 852);
            this.panelFinalBook.TabIndex = 7;
            // 
            // button_ConfirmBooking
            // 
            this.button_ConfirmBooking.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(90)))), ((int)(((byte)(95)))));
            this.button_ConfirmBooking.Cursor = System.Windows.Forms.Cursors.Hand;
            this.button_ConfirmBooking.FlatAppearance.BorderSize = 0;
            this.button_ConfirmBooking.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_ConfirmBooking.Font = new System.Drawing.Font("Nirmala UI", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_ConfirmBooking.ForeColor = System.Drawing.Color.White;
            this.button_ConfirmBooking.Location = new System.Drawing.Point(485, 702);
            this.button_ConfirmBooking.Name = "button_ConfirmBooking";
            this.button_ConfirmBooking.Size = new System.Drawing.Size(300, 80);
            this.button_ConfirmBooking.TabIndex = 3;
            this.button_ConfirmBooking.Text = "Confirm Booking";
            this.button_ConfirmBooking.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.button_ConfirmBooking.UseVisualStyleBackColor = false;
            this.button_ConfirmBooking.Click += new System.EventHandler(this.button_ConfirmBooking_Click);
            // 
            // labelTotalPrice
            // 
            this.labelTotalPrice.AutoSize = true;
            this.labelTotalPrice.Font = new System.Drawing.Font("Nirmala UI", 21.75F, System.Drawing.FontStyle.Bold);
            this.labelTotalPrice.Location = new System.Drawing.Point(98, 627);
            this.labelTotalPrice.Name = "labelTotalPrice";
            this.labelTotalPrice.Size = new System.Drawing.Size(210, 40);
            this.labelTotalPrice.TabIndex = 1;
            this.labelTotalPrice.Text = "Total Price: £0";
            // 
            // labelTotalNights
            // 
            this.labelTotalNights.AutoSize = true;
            this.labelTotalNights.Font = new System.Drawing.Font("Nirmala UI", 21.75F, System.Drawing.FontStyle.Bold);
            this.labelTotalNights.Location = new System.Drawing.Point(98, 577);
            this.labelTotalNights.Name = "labelTotalNights";
            this.labelTotalNights.Size = new System.Drawing.Size(215, 40);
            this.labelTotalNights.TabIndex = 1;
            this.labelTotalNights.Text = "Total Nights: 0";
            // 
            // labelCheckOut
            // 
            this.labelCheckOut.AutoSize = true;
            this.labelCheckOut.Font = new System.Drawing.Font("Nirmala UI", 21.75F, System.Drawing.FontStyle.Bold);
            this.labelCheckOut.Location = new System.Drawing.Point(98, 524);
            this.labelCheckOut.Name = "labelCheckOut";
            this.labelCheckOut.Size = new System.Drawing.Size(415, 40);
            this.labelCheckOut.TabIndex = 1;
            this.labelCheckOut.Text = "Check-out Date: Not selected";
            // 
            // labelCheckIn
            // 
            this.labelCheckIn.AutoSize = true;
            this.labelCheckIn.Font = new System.Drawing.Font("Nirmala UI", 21.75F, System.Drawing.FontStyle.Bold);
            this.labelCheckIn.Location = new System.Drawing.Point(98, 467);
            this.labelCheckIn.Name = "labelCheckIn";
            this.labelCheckIn.Size = new System.Drawing.Size(394, 40);
            this.labelCheckIn.TabIndex = 1;
            this.labelCheckIn.Text = "Check-in Date: Not selected";
            // 
            // bookingCalendar
            // 
            this.bookingCalendar.CalendarDimensions = new System.Drawing.Size(3, 2);
            this.bookingCalendar.Location = new System.Drawing.Point(62, 108);
            this.bookingCalendar.MaxDate = new System.DateTime(2025, 12, 31, 0, 0, 0, 0);
            this.bookingCalendar.MaxSelectionCount = 100;
            this.bookingCalendar.MinDate = new System.DateTime(2024, 11, 8, 0, 0, 0, 0);
            this.bookingCalendar.Name = "bookingCalendar";
            this.bookingCalendar.TabIndex = 0;
            this.bookingCalendar.DateSelected += new System.Windows.Forms.DateRangeEventHandler(this.bookingCalendar_DateSelected);
            // 
            // panelReservations
            // 
            this.panelReservations.BackColor = System.Drawing.Color.SeaShell;
            this.panelReservations.Controls.Add(this.label9);
            this.panelReservations.Controls.Add(this.flowPanelReservations);
            this.panelReservations.Location = new System.Drawing.Point(300, 0);
            this.panelReservations.Name = "panelReservations";
            this.panelReservations.Size = new System.Drawing.Size(841, 855);
            this.panelReservations.TabIndex = 8;
            // 
            // flowPanelReservations
            // 
            this.flowPanelReservations.BackColor = System.Drawing.Color.Transparent;
            this.flowPanelReservations.Location = new System.Drawing.Point(0, 113);
            this.flowPanelReservations.Name = "flowPanelReservations";
            this.flowPanelReservations.Size = new System.Drawing.Size(839, 746);
            this.flowPanelReservations.TabIndex = 0;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.BackColor = System.Drawing.Color.Transparent;
            this.label9.Font = new System.Drawing.Font("Nirmala UI", 26.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(56)))), ((int)(((byte)(92)))));
            this.label9.Location = new System.Drawing.Point(29, 39);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(321, 47);
            this.label9.TabIndex = 1;
            this.label9.Text = "Your Reservations:";
            // 
            // frmDashboard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::AirBnB.Properties.Resources.AdobeStock_288768256_2mb;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(1140, 855);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panelReservations);
            this.Controls.Add(this.panelHome);
            this.Controls.Add(this.panelListed);
            this.Controls.Add(this.panelList);
            this.Controls.Add(this.searchPanel);
            this.Controls.Add(this.panelBook);
            this.Controls.Add(this.panelFinalBook);
            this.Controls.Add(this.panelPropertyDetails);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "frmDashboard";
            this.Text = "frmDashboard";
            this.Load += new System.EventHandler(this.frmListed_Load);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.panelListed.ResumeLayout(false);
            this.panelListed.PerformLayout();
            this.panelBook.ResumeLayout(false);
            this.panelBook.PerformLayout();
            this.panelList.ResumeLayout(false);
            this.panelList.PerformLayout();
            this.searchPanel.ResumeLayout(false);
            this.searchPanel.PerformLayout();
            this.panelPropertyDetails.ResumeLayout(false);
            this.panelPropertyDetails.PerformLayout();
            this.panelFinalBook.ResumeLayout(false);
            this.panelFinalBook.PerformLayout();
            this.panelReservations.ResumeLayout(false);
            this.panelReservations.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label usernameLabel;
        private System.Windows.Forms.Button bookButton;
        private System.Windows.Forms.Button logoutButton;
        private System.Windows.Forms.Button listedButton;
        private System.Windows.Forms.Button listButton;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panelListed;
        private System.Windows.Forms.FlowLayoutPanel flowPanelImages;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panelHome;
        private System.Windows.Forms.Panel panelBook;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panelList;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button uploadButton;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtAddress;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button frontImageButton;
        private System.Windows.Forms.FlowLayoutPanel flowPanelBook;
        private System.Windows.Forms.TextBox txtTitle;
        private System.Windows.Forms.TextBox txtPrice;
        private System.Windows.Forms.TextBox txtCity;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.Label labelPrice;
        private System.Windows.Forms.Label labelCity;
        private System.Windows.Forms.Label labelDescription;
        private System.Windows.Forms.Panel searchPanel;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label searchLabel;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Button searchButton;
        private System.Windows.Forms.Panel panelPropertyDetails;
        private System.Windows.Forms.Label labelContact;
        private System.Windows.Forms.Label labelPricePerNight;
        private System.Windows.Forms.Button button_FinalBook;
        private System.Windows.Forms.Label labelAddress;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelImages;
        private System.Windows.Forms.Panel panelFinalBook;
        private System.Windows.Forms.FlowLayoutPanel flowPanelSearch;
        private System.Windows.Forms.MonthCalendar bookingCalendar;
        private System.Windows.Forms.Button button_ConfirmBooking;
        private System.Windows.Forms.Label labelTotalPrice;
        private System.Windows.Forms.Label labelTotalNights;
        private System.Windows.Forms.Label labelCheckOut;
        private System.Windows.Forms.Label labelCheckIn;
        private System.Windows.Forms.Button buttonReservations;
        private System.Windows.Forms.Panel panelReservations;
        private System.Windows.Forms.FlowLayoutPanel flowPanelReservations;
        private System.Windows.Forms.Label label9;
    }
}