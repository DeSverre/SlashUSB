namespace USkummelB
{
    partial class MainForm
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
            System.Windows.Forms.ListViewGroup listViewGroup1 = new System.Windows.Forms.ListViewGroup("Funnet", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup2 = new System.Windows.Forms.ListViewGroup("Aktivert", System.Windows.Forms.HorizontalAlignment.Left);
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.testButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.activateHUBbt = new System.Windows.Forms.Button();
            this.ExFATselect = new System.Windows.Forms.RadioButton();
            this.ntfsSelect = new System.Windows.Forms.RadioButton();
            this.fat32Select = new System.Windows.Forms.RadioButton();
            this.merkelappCheckBox = new System.Windows.Forms.CheckBox();
            this.formatChecked = new System.Windows.Forms.CheckBox();
            this.activatedCB = new System.Windows.Forms.CheckBox();
            this.cleanChecked = new System.Windows.Forms.CheckBox();
            this.usbListView = new System.Windows.Forms.ListView();
            this.Status = new System.Windows.Forms.ColumnHeader();
            this.Disk = new System.Windows.Forms.ColumnHeader();
            this.Navn = new System.Windows.Forms.ColumnHeader();
            this.DeviceNavn = new System.Windows.Forms.ColumnHeader();
            this.DiskSize = new System.Windows.Forms.ColumnHeader();
            this.Lokasjon = new System.Windows.Forms.ColumnHeader();
            this.Hub = new System.Windows.Forms.ColumnHeader();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.testButton);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.activateHUBbt);
            this.groupBox1.Controls.Add(this.ExFATselect);
            this.groupBox1.Controls.Add(this.ntfsSelect);
            this.groupBox1.Controls.Add(this.fat32Select);
            this.groupBox1.Controls.Add(this.merkelappCheckBox);
            this.groupBox1.Controls.Add(this.formatChecked);
            this.groupBox1.Controls.Add(this.activatedCB);
            this.groupBox1.Controls.Add(this.cleanChecked);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.groupBox1.Location = new System.Drawing.Point(0, 435);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(736, 130);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Standard handling";
            // 
            // testButton
            // 
            this.testButton.Enabled = false;
            this.testButton.Location = new System.Drawing.Point(554, 67);
            this.testButton.Name = "testButton";
            this.testButton.Size = new System.Drawing.Size(120, 35);
            this.testButton.TabIndex = 3;
            this.testButton.Text = "Utfør handling";
            this.testButton.UseVisualStyleBackColor = true;
            this.testButton.Click += new System.EventHandler(this.testButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(309, 77);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(176, 15);
            this.label1.TabIndex = 3;
            this.label1.Text = "Kun hurtigformatering er støttet";
            // 
            // activateHUBbt
            // 
            this.activateHUBbt.Enabled = false;
            this.activateHUBbt.Location = new System.Drawing.Point(554, 20);
            this.activateHUBbt.Name = "activateHUBbt";
            this.activateHUBbt.Size = new System.Drawing.Size(120, 35);
            this.activateHUBbt.TabIndex = 2;
            this.activateHUBbt.Text = "Aktiver hub";
            this.activateHUBbt.UseVisualStyleBackColor = true;
            this.activateHUBbt.Click += new System.EventHandler(this.actHubButClick);
            // 
            // ExFATselect
            // 
            this.ExFATselect.AutoSize = true;
            this.ExFATselect.Checked = true;
            this.ExFATselect.Location = new System.Drawing.Point(237, 50);
            this.ExFATselect.Name = "ExFATselect";
            this.ExFATselect.Size = new System.Drawing.Size(55, 19);
            this.ExFATselect.TabIndex = 2;
            this.ExFATselect.TabStop = true;
            this.ExFATselect.Text = "ExFAT";
            this.ExFATselect.UseVisualStyleBackColor = true;
            // 
            // ntfsSelect
            // 
            this.ntfsSelect.AutoSize = true;
            this.ntfsSelect.Location = new System.Drawing.Point(237, 75);
            this.ntfsSelect.Name = "ntfsSelect";
            this.ntfsSelect.Size = new System.Drawing.Size(52, 19);
            this.ntfsSelect.TabIndex = 2;
            this.ntfsSelect.Text = "NTFS";
            this.ntfsSelect.UseVisualStyleBackColor = true;
            // 
            // fat32Select
            // 
            this.fat32Select.AutoSize = true;
            this.fat32Select.Location = new System.Drawing.Point(237, 25);
            this.fat32Select.Name = "fat32Select";
            this.fat32Select.Size = new System.Drawing.Size(55, 19);
            this.fat32Select.TabIndex = 2;
            this.fat32Select.Text = "FAT32";
            this.fat32Select.UseVisualStyleBackColor = true;
            // 
            // merkelappCheckBox
            // 
            this.merkelappCheckBox.AutoSize = true;
            this.merkelappCheckBox.Checked = true;
            this.merkelappCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.merkelappCheckBox.Location = new System.Drawing.Point(309, 25);
            this.merkelappCheckBox.Name = "merkelappCheckBox";
            this.merkelappCheckBox.Size = new System.Drawing.Size(202, 19);
            this.merkelappCheckBox.TabIndex = 1;
            this.merkelappCheckBox.Text = "Lag merkelapp basert på størrelse";
            this.merkelappCheckBox.UseVisualStyleBackColor = true;
            // 
            // formatChecked
            // 
            this.formatChecked.AutoSize = true;
            this.formatChecked.Location = new System.Drawing.Point(99, 50);
            this.formatChecked.Name = "formatChecked";
            this.formatChecked.Size = new System.Drawing.Size(74, 19);
            this.formatChecked.TabIndex = 1;
            this.formatChecked.Text = "Formater";
            this.formatChecked.UseVisualStyleBackColor = true;
            // 
            // activatedCB
            // 
            this.activatedCB.Appearance = System.Windows.Forms.Appearance.Button;
            this.activatedCB.AutoSize = true;
            this.activatedCB.Location = new System.Drawing.Point(12, 25);
            this.activatedCB.Name = "activatedCB";
            this.activatedCB.Size = new System.Drawing.Size(54, 25);
            this.activatedCB.TabIndex = 1;
            this.activatedCB.Text = "Aktiver";
            this.activatedCB.UseVisualStyleBackColor = true;
            this.activatedCB.CheckedChanged += new System.EventHandler(this.AktivertCheckBox_CheckedChanged);
            // 
            // cleanChecked
            // 
            this.cleanChecked.AutoSize = true;
            this.cleanChecked.Location = new System.Drawing.Point(99, 25);
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
            this.usbListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.usbListView.FullRowSelect = true;
            listViewGroup1.CollapsedState = System.Windows.Forms.ListViewGroupCollapsedState.Expanded;
            listViewGroup1.Header = "Funnet";
            listViewGroup1.Name = "listViewGroupFunnet";
            listViewGroup2.CollapsedState = System.Windows.Forms.ListViewGroupCollapsedState.Expanded;
            listViewGroup2.Header = "Aktivert";
            listViewGroup2.Name = "listViewGroupAktivert";
            this.usbListView.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup1,
            listViewGroup2});
            this.usbListView.Location = new System.Drawing.Point(0, 0);
            this.usbListView.Name = "usbListView";
            this.usbListView.Size = new System.Drawing.Size(736, 435);
            this.usbListView.TabIndex = 1;
            this.usbListView.UseCompatibleStateImageBehavior = false;
            this.usbListView.View = System.Windows.Forms.View.Details;
            this.usbListView.SelectedIndexChanged += new System.EventHandler(this.listViewSelect_Changed);
            // 
            // Status
            // 
            this.Status.Text = "Status";
            this.Status.Width = 150;
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
            this.DeviceNavn.Width = 150;
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
            this.Hub.Width = 50;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(736, 565);
            this.Controls.Add(this.usbListView);
            this.Controls.Add(this.groupBox1);
            this.Name = "MainForm";
            this.Text = "USkummelB";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
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
        private CheckBox activatedCB;
        private ListView usbListView;
        private ColumnHeader Disk;
        private ColumnHeader Navn;
        private ColumnHeader DiskSize;
        private ColumnHeader Lokasjon;
        private ColumnHeader Hub;
        private CheckBox merkelappCheckBox;
        private Button activateHUBbt;
        private ColumnHeader DeviceNavn;
        private Button testButton;
        private ColumnHeader Status;
        private RadioButton ExFATselect;
        private Label label1;
    }
}