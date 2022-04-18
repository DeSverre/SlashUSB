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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.emergencyLight = new System.Windows.Forms.PictureBox();
            this.versionLabel = new System.Windows.Forms.Label();
            this.testButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.activateHUBbt = new System.Windows.Forms.Button();
            this.ExFATselect = new System.Windows.Forms.RadioButton();
            this.ntfsSelect = new System.Windows.Forms.RadioButton();
            this.fat32Select = new System.Windows.Forms.RadioButton();
            this.roundUpSizeCB = new System.Windows.Forms.CheckBox();
            this.merkelappCheckBox = new System.Windows.Forms.CheckBox();
            this.formatChecked = new System.Windows.Forms.CheckBox();
            this.activatedCB = new System.Windows.Forms.CheckBox();
            this.cleanChecked = new System.Windows.Forms.CheckBox();
            this.usbListView = new System.Windows.Forms.ListView();
            this._Status = new System.Windows.Forms.ColumnHeader();
            this.Disk = new System.Windows.Forms.ColumnHeader();
            this.Navn = new System.Windows.Forms.ColumnHeader();
            this.DeviceNavn = new System.Windows.Forms.ColumnHeader();
            this.DiskSize = new System.Windows.Forms.ColumnHeader();
            this.Lokasjon = new System.Windows.Forms.ColumnHeader();
            this.Hub = new System.Windows.Forms.ColumnHeader();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.emergencyLight)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Controls.Add(this.emergencyLight);
            this.groupBox1.Controls.Add(this.versionLabel);
            this.groupBox1.Controls.Add(this.testButton);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.activateHUBbt);
            this.groupBox1.Controls.Add(this.ExFATselect);
            this.groupBox1.Controls.Add(this.ntfsSelect);
            this.groupBox1.Controls.Add(this.fat32Select);
            this.groupBox1.Controls.Add(this.roundUpSizeCB);
            this.groupBox1.Controls.Add(this.merkelappCheckBox);
            this.groupBox1.Controls.Add(this.formatChecked);
            this.groupBox1.Controls.Add(this.activatedCB);
            this.groupBox1.Controls.Add(this.cleanChecked);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // emergencyLight
            // 
            resources.ApplyResources(this.emergencyLight, "emergencyLight");
            this.emergencyLight.Name = "emergencyLight";
            this.emergencyLight.TabStop = false;
            // 
            // versionLabel
            // 
            resources.ApplyResources(this.versionLabel, "versionLabel");
            this.versionLabel.Name = "versionLabel";
            // 
            // testButton
            // 
            resources.ApplyResources(this.testButton, "testButton");
            this.testButton.Name = "testButton";
            this.testButton.UseVisualStyleBackColor = true;
            this.testButton.Click += new System.EventHandler(this.testButton_Click);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // activateHUBbt
            // 
            resources.ApplyResources(this.activateHUBbt, "activateHUBbt");
            this.activateHUBbt.Name = "activateHUBbt";
            this.activateHUBbt.UseVisualStyleBackColor = true;
            this.activateHUBbt.Click += new System.EventHandler(this.ActHubButClick);
            // 
            // ExFATselect
            // 
            resources.ApplyResources(this.ExFATselect, "ExFATselect");
            this.ExFATselect.Checked = true;
            this.ExFATselect.Name = "ExFATselect";
            this.ExFATselect.TabStop = true;
            this.ExFATselect.UseVisualStyleBackColor = true;
            // 
            // ntfsSelect
            // 
            resources.ApplyResources(this.ntfsSelect, "ntfsSelect");
            this.ntfsSelect.Name = "ntfsSelect";
            this.ntfsSelect.UseVisualStyleBackColor = true;
            // 
            // fat32Select
            // 
            resources.ApplyResources(this.fat32Select, "fat32Select");
            this.fat32Select.Name = "fat32Select";
            this.fat32Select.UseVisualStyleBackColor = true;
            // 
            // roundUpSizeCB
            // 
            resources.ApplyResources(this.roundUpSizeCB, "roundUpSizeCB");
            this.roundUpSizeCB.Checked = true;
            this.roundUpSizeCB.CheckState = System.Windows.Forms.CheckState.Checked;
            this.roundUpSizeCB.Name = "roundUpSizeCB";
            this.roundUpSizeCB.UseVisualStyleBackColor = true;
            // 
            // merkelappCheckBox
            // 
            resources.ApplyResources(this.merkelappCheckBox, "merkelappCheckBox");
            this.merkelappCheckBox.Checked = true;
            this.merkelappCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.merkelappCheckBox.Name = "merkelappCheckBox";
            this.merkelappCheckBox.UseVisualStyleBackColor = true;
            // 
            // formatChecked
            // 
            resources.ApplyResources(this.formatChecked, "formatChecked");
            this.formatChecked.Name = "formatChecked";
            this.formatChecked.UseVisualStyleBackColor = true;
            // 
            // activatedCB
            // 
            resources.ApplyResources(this.activatedCB, "activatedCB");
            this.activatedCB.Name = "activatedCB";
            this.activatedCB.UseVisualStyleBackColor = true;
            this.activatedCB.CheckedChanged += new System.EventHandler(this.AktivertCheckBox_CheckedChanged);
            // 
            // cleanChecked
            // 
            resources.ApplyResources(this.cleanChecked, "cleanChecked");
            this.cleanChecked.Name = "cleanChecked";
            this.cleanChecked.UseVisualStyleBackColor = true;
            // 
            // usbListView
            // 
            resources.ApplyResources(this.usbListView, "usbListView");
            this.usbListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this._Status,
            this.Disk,
            this.Navn,
            this.DeviceNavn,
            this.DiskSize,
            this.Lokasjon,
            this.Hub});
            this.usbListView.FullRowSelect = true;
            this.usbListView.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            ((System.Windows.Forms.ListViewGroup)(resources.GetObject("usbListView.Groups"))),
            ((System.Windows.Forms.ListViewGroup)(resources.GetObject("usbListView.Groups1")))});
            this.usbListView.Name = "usbListView";
            this.usbListView.UseCompatibleStateImageBehavior = false;
            this.usbListView.View = System.Windows.Forms.View.Details;
            this.usbListView.SelectedIndexChanged += new System.EventHandler(this.ListViewSelect_Changed);
            // 
            // _Status
            // 
            resources.ApplyResources(this._Status, "_Status");
            // 
            // Disk
            // 
            resources.ApplyResources(this.Disk, "Disk");
            // 
            // Navn
            // 
            resources.ApplyResources(this.Navn, "Navn");
            // 
            // DeviceNavn
            // 
            resources.ApplyResources(this.DeviceNavn, "DeviceNavn");
            // 
            // DiskSize
            // 
            resources.ApplyResources(this.DiskSize, "DiskSize");
            // 
            // Lokasjon
            // 
            resources.ApplyResources(this.Lokasjon, "Lokasjon");
            // 
            // Hub
            // 
            resources.ApplyResources(this.Hub, "Hub");
            // 
            // MainForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.usbListView);
            this.Controls.Add(this.groupBox1);
            this.Name = "MainForm";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.emergencyLight)).EndInit();
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
        private ColumnHeader _Status;
        private RadioButton ExFATselect;
        private Label label1;
        private Label versionLabel;
        private PictureBox emergencyLight;
        private CheckBox roundUpSizeCB;
    }
}