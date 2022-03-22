namespace USkummelB
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.ListViewGroup listViewGroup3 = new System.Windows.Forms.ListViewGroup("Funnet", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup4 = new System.Windows.Forms.ListViewGroup("Aktivert", System.Windows.Forms.HorizontalAlignment.Left);
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.ntfsSelect = new System.Windows.Forms.RadioButton();
            this.fat32Select = new System.Windows.Forms.RadioButton();
            this.merkelappCheckBox = new System.Windows.Forms.CheckBox();
            this.formatChecked = new System.Windows.Forms.CheckBox();
            this.checkBox3 = new System.Windows.Forms.CheckBox();
            this.cleanChecked = new System.Windows.Forms.CheckBox();
            this.usbListView = new System.Windows.Forms.ListView();
            this.Status = new System.Windows.Forms.ColumnHeader();
            this.Disk = new System.Windows.Forms.ColumnHeader();
            this.Navn = new System.Windows.Forms.ColumnHeader();
            this.DeviceNavn = new System.Windows.Forms.ColumnHeader();
            this.DiskSize = new System.Windows.Forms.ColumnHeader();
            this.Lokasjon = new System.Windows.Forms.ColumnHeader();
            this.Hub = new System.Windows.Forms.ColumnHeader();
            this.button1 = new System.Windows.Forms.Button();
            this.testButton = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.ntfsSelect);
            this.groupBox1.Controls.Add(this.fat32Select);
            this.groupBox1.Controls.Add(this.merkelappCheckBox);
            this.groupBox1.Controls.Add(this.formatChecked);
            this.groupBox1.Controls.Add(this.checkBox3);
            this.groupBox1.Controls.Add(this.cleanChecked);
            this.groupBox1.Location = new System.Drawing.Point(1181, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(232, 367);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Standard handling";
            // 
            // ntfsSelect
            // 
            this.ntfsSelect.AutoSize = true;
            this.ntfsSelect.Location = new System.Drawing.Point(45, 135);
            this.ntfsSelect.Name = "ntfsSelect";
            this.ntfsSelect.Size = new System.Drawing.Size(52, 19);
            this.ntfsSelect.TabIndex = 2;
            this.ntfsSelect.Text = "NTFS";
            this.ntfsSelect.UseVisualStyleBackColor = true;
            // 
            // fat32Select
            // 
            this.fat32Select.AutoSize = true;
            this.fat32Select.Checked = true;
            this.fat32Select.Location = new System.Drawing.Point(45, 110);
            this.fat32Select.Name = "fat32Select";
            this.fat32Select.Size = new System.Drawing.Size(55, 19);
            this.fat32Select.TabIndex = 2;
            this.fat32Select.TabStop = true;
            this.fat32Select.Text = "FAT32";
            this.fat32Select.UseVisualStyleBackColor = true;
            // 
            // merkelappCheckBox
            // 
            this.merkelappCheckBox.AutoSize = true;
            this.merkelappCheckBox.Checked = true;
            this.merkelappCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.merkelappCheckBox.Location = new System.Drawing.Point(23, 172);
            this.merkelappCheckBox.Name = "merkelappCheckBox";
            this.merkelappCheckBox.Size = new System.Drawing.Size(202, 19);
            this.merkelappCheckBox.TabIndex = 1;
            this.merkelappCheckBox.Text = "Lag merkelapp basert på størrelse";
            this.merkelappCheckBox.UseVisualStyleBackColor = true;
            // 
            // formatChecked
            // 
            this.formatChecked.AutoSize = true;
            this.formatChecked.Location = new System.Drawing.Point(23, 81);
            this.formatChecked.Name = "formatChecked";
            this.formatChecked.Size = new System.Drawing.Size(74, 19);
            this.formatChecked.TabIndex = 1;
            this.formatChecked.Text = "Formater";
            this.formatChecked.UseVisualStyleBackColor = true;
            // 
            // checkBox3
            // 
            this.checkBox3.Appearance = System.Windows.Forms.Appearance.Button;
            this.checkBox3.AutoSize = true;
            this.checkBox3.Location = new System.Drawing.Point(23, 25);
            this.checkBox3.Name = "checkBox3";
            this.checkBox3.Size = new System.Drawing.Size(54, 25);
            this.checkBox3.TabIndex = 1;
            this.checkBox3.Text = "Aktiver";
            this.checkBox3.UseVisualStyleBackColor = true;
            this.checkBox3.CheckedChanged += new System.EventHandler(this.AktivertCheckBox_CheckedChanged);
            // 
            // cleanChecked
            // 
            this.cleanChecked.AutoSize = true;
            this.cleanChecked.Location = new System.Drawing.Point(23, 56);
            this.cleanChecked.Name = "cleanChecked";
            this.cleanChecked.Size = new System.Drawing.Size(119, 19);
            this.cleanChecked.TabIndex = 1;
            this.cleanChecked.Text = "Fjern partisjon(er)";
            this.cleanChecked.UseVisualStyleBackColor = true;
            // 
            // usbListView
            // 
            this.usbListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.Status,
            this.Disk,
            this.Navn,
            this.DeviceNavn,
            this.DiskSize,
            this.Lokasjon,
            this.Hub});
            this.usbListView.FullRowSelect = true;
            listViewGroup3.CollapsedState = System.Windows.Forms.ListViewGroupCollapsedState.Expanded;
            listViewGroup3.Header = "Funnet";
            listViewGroup3.Name = "listViewGroupFunnet";
            listViewGroup4.CollapsedState = System.Windows.Forms.ListViewGroupCollapsedState.Expanded;
            listViewGroup4.Header = "Aktivert";
            listViewGroup4.Name = "listViewGroupAktivert";
            this.usbListView.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup3,
            listViewGroup4});
            this.usbListView.Location = new System.Drawing.Point(12, 12);
            this.usbListView.Name = "usbListView";
            this.usbListView.Size = new System.Drawing.Size(916, 602);
            this.usbListView.TabIndex = 1;
            this.usbListView.UseCompatibleStateImageBehavior = false;
            this.usbListView.View = System.Windows.Forms.View.Details;
            // 
            // Status
            // 
            this.Status.Text = "Status";
            this.Status.Width = 70;
            // 
            // Disk
            // 
            this.Disk.Text = "Disk";
            this.Disk.Width = 40;
            // 
            // Navn
            // 
            this.Navn.Text = "Navn";
            this.Navn.Width = 100;
            // 
            // DeviceNavn
            // 
            this.DeviceNavn.Text = "DeviceNavn";
            this.DeviceNavn.Width = 100;
            // 
            // DiskSize
            // 
            this.DiskSize.Text = "Størrelse";
            this.DiskSize.Width = 100;
            // 
            // Lokasjon
            // 
            this.Lokasjon.Text = "Lokasjon";
            this.Lokasjon.Width = 150;
            // 
            // Hub
            // 
            this.Hub.Text = "Hub";
            this.Hub.Width = 550;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(963, 61);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(120, 35);
            this.button1.TabIndex = 2;
            this.button1.Text = "Aktiver hub";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // testButton
            // 
            this.testButton.Location = new System.Drawing.Point(976, 129);
            this.testButton.Name = "testButton";
            this.testButton.Size = new System.Drawing.Size(95, 45);
            this.testButton.TabIndex = 3;
            this.testButton.Text = "Test";
            this.testButton.UseVisualStyleBackColor = true;
            this.testButton.Click += new System.EventHandler(this.testButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1425, 641);
            this.Controls.Add(this.testButton);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.usbListView);
            this.Controls.Add(this.groupBox1);
            this.Name = "Form1";
            this.Text = "USkummelB";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private GroupBox groupBox1;
        private RadioButton ntfsSelect;
        private RadioButton fat32Select;
        private CheckBox formatChecked;
        private CheckBox cleanChecked;
        private CheckBox checkBox3;
        private ListView usbListView;
        private ColumnHeader Disk;
        private ColumnHeader Navn;
        private ColumnHeader DiskSize;
        private ColumnHeader Lokasjon;
        private ColumnHeader Hub;
        private CheckBox merkelappCheckBox;
        private Button button1;
        private ColumnHeader DeviceNavn;
        private Button testButton;
        private ColumnHeader Status;
    }
}