﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using CU30Interface;
using Nikon;

namespace CameraAndStage
{
    public partial class CameraAndStageForm : Form
    {
        private NikonManager manager;
        private NikonDevice device;
        private Timer liveViewTimer;
        CU30 CU30obj = new CU30();

        public CameraAndStageForm()
        {
            InitializeComponent();

            // Disable buttons
            ToggleButtons(false);

            // Initialize live view timer
            liveViewTimer = new Timer();
            liveViewTimer.Tick += new EventHandler(liveViewTimer_Tick);
            liveViewTimer.Interval = 1000 / 30;

            // Initialize Nikon manager
            manager = new NikonManager("Type0009.md3");
            manager.DeviceAdded += new DeviceAddedDelegate(manager_DeviceAdded);
            manager.DeviceRemoved += new DeviceRemovedDelegate(manager_DeviceRemoved);
        }

        // Camera controls

        protected override void OnClosing(CancelEventArgs e)
        {
            // Disable live view (in case it's enabled)
            if (device != null)
            {
                device.LiveViewEnabled = false;
            }

            // Shut down the Nikon manager
            manager.Shutdown();
            base.OnClosing(e);
        }

        void manager_DeviceAdded(NikonManager sender, NikonDevice device)
        {
            this.device = device;

            // Set the device name
            nameLabel.Text = device.Name;

            // Enable buttons
            ToggleButtons(true);

            // Hook up device capture events
            device.ImageReady += new ImageReadyDelegate(device_ImageReady);
            device.CaptureComplete += new CaptureCompleteDelegate(device_CaptureComplete);
        }

        void manager_DeviceRemoved(NikonManager sender, NikonDevice device)
        {
            this.device = null;

            // Stop live view timer
            liveViewTimer.Stop();

            // Clear device name
            nameLabel.Text = "No Camera";

            // Disable buttons
            ToggleButtons(false);

            // Clear live view picture
            pictureBox.Image = null;
        }

        void liveViewTimer_Tick(object sender, EventArgs e)
        {
            // Get live view image
            NikonLiveViewImage image = null;

            try
            {
                image = device.GetLiveViewImage();
            }
            catch (NikonException)
            {
                liveViewTimer.Stop();
            }

            // Set live view image on picture box
            if (image != null)
            {
                MemoryStream stream = new MemoryStream(image.JpegBuffer);
                pictureBox.Image = Image.FromStream(stream);
            }
        }

        void device_ImageReady(NikonDevice sender, NikonImage image)
        {
            SaveFileDialog dialog = new SaveFileDialog();

            dialog.Filter = (image.Type == NikonImageType.Jpeg) ?
                "Jpeg Image (*.jpg)|*.jpg" :
                "Nikon NEF (*.nef)|*.nef";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                using (FileStream stream = new FileStream(dialog.FileName, FileMode.Create, FileAccess.Write))
                {
                    stream.Write(image.Buffer, 0, image.Buffer.Length);
                }
            }
        }

        void device_CaptureComplete(NikonDevice sender, int data)
        {
            // Re-enable buttons when the capture completes
            ToggleButtons(true);
        }

        void ToggleButtons(bool enabled)
        {
            this.captureButton.Enabled = enabled;
            this.liveViewButton.Enabled = enabled;
        }

        private void captureButton_Click(object sender, EventArgs e)
        {
            if (device == null)
            {
                return;
            }

            ToggleButtons(false);

            try
            {
                device.Capture();
            }
            catch (NikonException ex)
            {
                MessageBox.Show(ex.Message);
                ToggleButtons(true);
            }

            pictureBox.Image = null;
        }

        private void liveViewButton_Click(object sender, EventArgs e)
        {
            if (device == null)
            {
                return;
            }
            if (device.LiveViewEnabled)
            {
                device.LiveViewEnabled = false;
                liveViewTimer.Stop();
                pictureBox.Image = null;
            }
            else
            {
                device.LiveViewEnabled = true;
                liveViewTimer.Start();
            }
        }

        // Stage controls
        private void openButton_Click(object sender, EventArgs e)
        {
            string szResult = CU30obj.m_CU30Open(0).ToString();

            if (szResult.Length > 5)
            {
                this.bottomTextBox.Text = szResult;
            }
            else
            {
                this.bottomTextBox.Text = "connected";
            }
        }

        private void XPlusButton_Click(object sender, EventArgs e)
        {
            int vel = (int)this.XVelocityNumericUpDown.Value;
            int steps = (int)this.XStepsNumericUpDown.Value;
            CU30obj.m_CU30Step(1, vel, steps);
        }

        private void XMinusButton_Click(object sender, EventArgs e)
        {
            int vel = (int)this.XVelocityNumericUpDown.Value;
            int steps = (int)this.XStepsNumericUpDown.Value;
            CU30obj.m_CU30Step(1, -vel, steps);
        }

        private void YPlusButton_Click(object sender, EventArgs e)
        {
            int vel = (int)this.YVelocityNumericUpDown.Value;
            int steps = (int)this.YStepsNumericUpDown.Value;
            CU30obj.m_CU30Step(2, vel, steps);
        }

        private void YMinusButton_Click(object sender, EventArgs e)
        {
            int vel = (int)this.YVelocityNumericUpDown.Value;
            int steps = (int)this.YStepsNumericUpDown.Value;
            CU30obj.m_CU30Step(2, -vel, steps);
        }

        private void ZPlusButton_Click(object sender, EventArgs e)
        {
            int vel = (int)this.ZVelocityNumericUpDown.Value;
            int steps = (int)this.ZStepsNumericUpDown.Value;
            CU30obj.m_CU30Step(3, vel, steps);
        }

        private void ZMinusButton_Click(object sender, EventArgs e)
        {
            int vel = (int)this.ZVelocityNumericUpDown.Value;
            int steps = (int)this.ZStepsNumericUpDown.Value;
            CU30obj.m_CU30Step(3, -vel, steps);
        }

        private void CameraAndStageForm_Load(object sender, EventArgs e)
        {

        }

    }
}
