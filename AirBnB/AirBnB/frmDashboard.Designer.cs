﻿namespace AirBnB
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
            this.flowPanelBook = new System.Windows.Forms.FlowLayoutPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.panelList = new System.Windows.Forms.Panel();
            this.frontImageButton = new System.Windows.Forms.Button();
            this.txtAddress = new System.Windows.Forms.TextBox();
            this.uploadButton = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panelListed.SuspendLayout();
            this.panelBook.SuspendLayout();
            this.panelList.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(26)))), ((int)(((byte)(30)))));
            this.panel1.Controls.Add(this.panel3);
            this.panel1.Controls.Add(this.logoutButton);
            this.panel1.Controls.Add(this.listedButton);
            this.panel1.Controls.Add(this.listButton);
            this.panel1.Controls.Add(this.bookButton);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(400, 1052);
            this.panel1.TabIndex = 0;
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.SeaShell;
            this.panel3.Location = new System.Drawing.Point(0, 239);
            this.panel3.Margin = new System.Windows.Forms.Padding(4);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(7, 295);
            this.panel3.TabIndex = 2;
            // 
            // logoutButton
            // 
            this.logoutButton.BackColor = System.Drawing.Color.DimGray;
            this.logoutButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.logoutButton.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.logoutButton.FlatAppearance.BorderSize = 0;
            this.logoutButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.logoutButton.Font = new System.Drawing.Font("Nirmala UI", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.logoutButton.ForeColor = System.Drawing.Color.SeaShell;
            this.logoutButton.Location = new System.Drawing.Point(0, 954);
            this.logoutButton.Margin = new System.Windows.Forms.Padding(4);
            this.logoutButton.Name = "logoutButton";
            this.logoutButton.Size = new System.Drawing.Size(400, 98);
            this.logoutButton.TabIndex = 1;
            this.logoutButton.Text = "Logout";
            this.logoutButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.logoutButton.UseVisualStyleBackColor = false;
            this.logoutButton.Click += new System.EventHandler(this.button3_Click);
            // 
            // listedButton
            // 
            this.listedButton.BackColor = System.Drawing.Color.DimGray;
            this.listedButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.listedButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.listedButton.FlatAppearance.BorderSize = 0;
            this.listedButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.listedButton.Font = new System.Drawing.Font("Nirmala UI", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listedButton.ForeColor = System.Drawing.Color.SeaShell;
            this.listedButton.Image = global::AirBnB.Properties.Resources.icons8_house_64;
            this.listedButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.listedButton.Location = new System.Drawing.Point(0, 435);
            this.listedButton.Margin = new System.Windows.Forms.Padding(4);
            this.listedButton.Name = "listedButton";
            this.listedButton.Size = new System.Drawing.Size(400, 98);
            this.listedButton.TabIndex = 1;
            this.listedButton.Text = "Listed Property";
            this.listedButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.listedButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.listedButton.UseVisualStyleBackColor = false;
            this.listedButton.Click += new System.EventHandler(this.listedButton_Click);
            // 
            // listButton
            // 
            this.listButton.BackColor = System.Drawing.Color.DimGray;
            this.listButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.listButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.listButton.FlatAppearance.BorderSize = 0;
            this.listButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.listButton.Font = new System.Drawing.Font("Nirmala UI", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listButton.ForeColor = System.Drawing.Color.SeaShell;
            this.listButton.Image = global::AirBnB.Properties.Resources.icons8_key_64;
            this.listButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.listButton.Location = new System.Drawing.Point(0, 337);
            this.listButton.Margin = new System.Windows.Forms.Padding(4);
            this.listButton.Name = "listButton";
            this.listButton.Size = new System.Drawing.Size(400, 98);
            this.listButton.TabIndex = 1;
            this.listButton.Text = "List Property";
            this.listButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.listButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.listButton.UseVisualStyleBackColor = false;
            this.listButton.Click += new System.EventHandler(this.listButton_Click);
            // 
            // bookButton
            // 
            this.bookButton.BackColor = System.Drawing.Color.DimGray;
            this.bookButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.bookButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.bookButton.FlatAppearance.BorderSize = 0;
            this.bookButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bookButton.Font = new System.Drawing.Font("Nirmala UI", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bookButton.ForeColor = System.Drawing.Color.SeaShell;
            this.bookButton.Image = global::AirBnB.Properties.Resources.icons8_leasing_60;
            this.bookButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.bookButton.Location = new System.Drawing.Point(0, 239);
            this.bookButton.Margin = new System.Windows.Forms.Padding(4);
            this.bookButton.Name = "bookButton";
            this.bookButton.Size = new System.Drawing.Size(400, 98);
            this.bookButton.TabIndex = 1;
            this.bookButton.Text = "Book Property";
            this.bookButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.bookButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.bookButton.UseVisualStyleBackColor = false;
            this.bookButton.Click += new System.EventHandler(this.bookButton_Click);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.usernameLabel);
            this.panel2.Controls.Add(this.pictureBox1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Margin = new System.Windows.Forms.Padding(4);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(400, 239);
            this.panel2.TabIndex = 0;
            // 
            // usernameLabel
            // 
            this.usernameLabel.AutoSize = true;
            this.usernameLabel.BackColor = System.Drawing.Color.Transparent;
            this.usernameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.usernameLabel.ForeColor = System.Drawing.Color.SeaShell;
            this.usernameLabel.Location = new System.Drawing.Point(107, 15);
            this.usernameLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.usernameLabel.Name = "usernameLabel";
            this.usernameLabel.Size = new System.Drawing.Size(124, 29);
            this.usernameLabel.TabIndex = 1;
            this.usernameLabel.Text = "Username";
            this.usernameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::AirBnB.Properties.Resources._CITYPNG_COM_White_User_Member_Guest_Icon_PNG_Image___4000x4000;
            this.pictureBox1.Location = new System.Drawing.Point(16, 15);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(4);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(83, 80);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // panelListed
            // 
            this.panelListed.BackColor = System.Drawing.Color.White;
            this.panelListed.BackgroundImage = global::AirBnB.Properties.Resources.AdobeStock_288768256_2mb;
            this.panelListed.Controls.Add(this.flowPanelImages);
            this.panelListed.Controls.Add(this.label1);
            this.panelListed.Location = new System.Drawing.Point(400, 0);
            this.panelListed.Margin = new System.Windows.Forms.Padding(4);
            this.panelListed.Name = "panelListed";
            this.panelListed.Size = new System.Drawing.Size(1121, 1052);
            this.panelListed.TabIndex = 1;
            // 
            // flowPanelImages
            // 
            this.flowPanelImages.BackColor = System.Drawing.Color.Transparent;
            this.flowPanelImages.Location = new System.Drawing.Point(8, 113);
            this.flowPanelImages.Margin = new System.Windows.Forms.Padding(4);
            this.flowPanelImages.Name = "flowPanelImages";
            this.flowPanelImages.Size = new System.Drawing.Size(1113, 939);
            this.flowPanelImages.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Font = new System.Drawing.Font("Nirmala UI", 26.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(32, 32);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(503, 60);
            this.label1.TabIndex = 0;
            this.label1.Text = "Your Property Pictures:";
            // 
            // panelHome
            // 
            this.panelHome.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panelHome.BackgroundImage = global::AirBnB.Properties.Resources._4e8ff027_0c5e_4603_a5a9_541e77c28b5b;
            this.panelHome.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.panelHome.Location = new System.Drawing.Point(400, 0);
            this.panelHome.Margin = new System.Windows.Forms.Padding(4);
            this.panelHome.Name = "panelHome";
            this.panelHome.Size = new System.Drawing.Size(1121, 1052);
            this.panelHome.TabIndex = 2;
            // 
            // panelBook
            // 
            this.panelBook.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panelBook.BackColor = System.Drawing.Color.Transparent;
            this.panelBook.Controls.Add(this.flowPanelBook);
            this.panelBook.Controls.Add(this.label2);
            this.panelBook.Location = new System.Drawing.Point(400, 0);
            this.panelBook.Margin = new System.Windows.Forms.Padding(4);
            this.panelBook.Name = "panelBook";
            this.panelBook.Size = new System.Drawing.Size(1121, 1052);
            this.panelBook.TabIndex = 3;
            // 
            // flowPanelBook
            // 
            this.flowPanelBook.AutoSize = true;
            this.flowPanelBook.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowPanelBook.Location = new System.Drawing.Point(0, 110);
            this.flowPanelBook.Margin = new System.Windows.Forms.Padding(4);
            this.flowPanelBook.Name = "flowPanelBook";
            this.flowPanelBook.Size = new System.Drawing.Size(0, 0);
            this.flowPanelBook.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Nirmala UI", 26.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(39, 48);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(442, 60);
            this.label2.TabIndex = 0;
            this.label2.Text = "Available Properties";
            // 
            // panelList
            // 
            this.panelList.BackColor = System.Drawing.Color.Transparent;
            this.panelList.BackgroundImage = global::AirBnB.Properties.Resources.image;
            this.panelList.Controls.Add(this.frontImageButton);
            this.panelList.Controls.Add(this.txtAddress);
            this.panelList.Controls.Add(this.uploadButton);
            this.panelList.Controls.Add(this.label7);
            this.panelList.Controls.Add(this.label6);
            this.panelList.Controls.Add(this.label5);
            this.panelList.Controls.Add(this.label4);
            this.panelList.Controls.Add(this.label3);
            this.panelList.Location = new System.Drawing.Point(400, 0);
            this.panelList.Margin = new System.Windows.Forms.Padding(4);
            this.panelList.Name = "panelList";
            this.panelList.Size = new System.Drawing.Size(1121, 1052);
            this.panelList.TabIndex = 4;
            // 
            // frontImageButton
            // 
            this.frontImageButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(26)))), ((int)(((byte)(30)))));
            this.frontImageButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.frontImageButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.frontImageButton.FlatAppearance.BorderSize = 0;
            this.frontImageButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.frontImageButton.Font = new System.Drawing.Font("Nirmala UI", 26.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.frontImageButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(223)))), ((int)(((byte)(204)))));
            this.frontImageButton.Location = new System.Drawing.Point(620, 902);
            this.frontImageButton.Margin = new System.Windows.Forms.Padding(4);
            this.frontImageButton.Name = "frontImageButton";
            this.frontImageButton.Size = new System.Drawing.Size(427, 92);
            this.frontImageButton.TabIndex = 4;
            this.frontImageButton.Text = "UPLOAD";
            this.frontImageButton.UseVisualStyleBackColor = false;
            this.frontImageButton.Click += new System.EventHandler(this.frontImageButton_Click);
            // 
            // txtAddress
            // 
            this.txtAddress.BackColor = System.Drawing.Color.White;
            this.txtAddress.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtAddress.Font = new System.Drawing.Font("MS UI Gothic", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtAddress.Location = new System.Drawing.Point(43, 812);
            this.txtAddress.Margin = new System.Windows.Forms.Padding(4);
            this.txtAddress.Multiline = true;
            this.txtAddress.Name = "txtAddress";
            this.txtAddress.Size = new System.Drawing.Size(705, 39);
            this.txtAddress.TabIndex = 3;
            // 
            // uploadButton
            // 
            this.uploadButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(26)))), ((int)(((byte)(30)))));
            this.uploadButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.uploadButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.uploadButton.FlatAppearance.BorderColor = System.Drawing.Color.Sienna;
            this.uploadButton.FlatAppearance.BorderSize = 0;
            this.uploadButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.uploadButton.Font = new System.Drawing.Font("Nirmala UI", 26.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.uploadButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(223)))), ((int)(((byte)(204)))));
            this.uploadButton.Location = new System.Drawing.Point(43, 902);
            this.uploadButton.Margin = new System.Windows.Forms.Padding(4);
            this.uploadButton.Name = "uploadButton";
            this.uploadButton.Size = new System.Drawing.Size(427, 92);
            this.uploadButton.TabIndex = 1;
            this.uploadButton.Text = "UPLOAD";
            this.uploadButton.UseVisualStyleBackColor = false;
            this.uploadButton.Click += new System.EventHandler(this.uploadButton_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Nirmala UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.ForeColor = System.Drawing.SystemColors.GrayText;
            this.label7.Location = new System.Drawing.Point(613, 864);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(174, 37);
            this.label7.TabIndex = 0;
            this.label7.Text = "Front Image";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Nirmala UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.SystemColors.GrayText;
            this.label6.Location = new System.Drawing.Point(43, 864);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(152, 37);
            this.label6.TabIndex = 0;
            this.label6.Text = "All Images";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Nirmala UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.SystemColors.GrayText;
            this.label5.Location = new System.Drawing.Point(36, 772);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(120, 37);
            this.label5.TabIndex = 0;
            this.label5.Text = "Address";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Nirmala UI", 26.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(26)))), ((int)(((byte)(30)))));
            this.label4.Location = new System.Drawing.Point(39, 132);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(269, 60);
            this.label4.TabIndex = 0;
            this.label4.Text = "images of it";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Nirmala UI", 26.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(26)))), ((int)(((byte)(30)))));
            this.label3.Location = new System.Drawing.Point(39, 74);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(863, 60);
            this.label3.TabIndex = 0;
            this.label3.Text = "Enter your property address and upload ";
            // 
            // frmDashboard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::AirBnB.Properties.Resources.AdobeStock_288768256_2mb;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(1520, 1052);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panelHome);
            this.Controls.Add(this.panelListed);
            this.Controls.Add(this.panelList);
            this.Controls.Add(this.panelBook);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
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
    }
}