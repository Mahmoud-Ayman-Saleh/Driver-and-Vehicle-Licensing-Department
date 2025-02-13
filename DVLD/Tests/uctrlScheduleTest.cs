﻿using BusinessLayer;
using DVLD.Properties;
using DVLD.Tests.Vison_Test;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace DVLD.Tests
{
    public partial class uctrlScheduleTest : UserControl
    {
        clsLocalDLA _DLApp;
        clsApplication _Application;
        clsTestAppointments testAppointment = new clsTestAppointments();

        private int _ID = -1;
        clsTestAppointments.TestType enTestType;
        bool _IsRetakeTest;
        enum ctrlMode { Add = 1, Update = 2 };
        ctrlMode _ctrlMode = ctrlMode.Add;

        public int DLAppID
        {
            get
            {
                return _ID;
            }
            set
            {
                _ID = value;
            }
        }

        public uctrlScheduleTest()
        {
            InitializeComponent();

            
        }

        public void SetControl(int DLAppID,clsTestAppointments.TestType enTestType,bool IsRetakeTest,clsTestAppointments testAppointment = null)
        {
            
            //if Edit Mode
            if (testAppointment != null)
            {
                this.testAppointment = testAppointment;
                _ctrlMode = ctrlMode.Update;

                //if Test is Locked 
                if (testAppointment.IsLocked == true)
                {
                    btnSave.Enabled = false;
                    dtpDate.Enabled = false;
                    lbl_AppointmentLocked.Visible = true;
                }

            }
            _IsRetakeTest = IsRetakeTest;
            this.enTestType = enTestType;
            this.DLAppID = DLAppID;
            _LoadApplicationData();


            //picturebox
            switch (enTestType)
            {
                case clsTestAppointments.TestType.VisionTest:
                    {
                        pictureBox1.Image = Resources.eye_scan;
                        break;
                    }
                case clsTestAppointments.TestType.WrittenTest:
                    {
                        pictureBox1.Image = Resources.test;
                        break;
                    }
                default:
                    {
                        pictureBox1.Image = Resources.steering_wheel;
                        break;
                    }
            }

            //Label
            if (IsRetakeTest)
            {
                lblFormLabel.Text = "Schedule Retake Test";
                gbRetakeTestInfo.Enabled = true;
            }
            else
            {
                switch (enTestType)
                {
                    case clsTestAppointments.TestType.VisionTest:
                        {
                            lblFormLabel.Text = "Vision Test Appointments";
                            break;
                        }
                    case clsTestAppointments.TestType.WrittenTest:
                        {
                            lblFormLabel.Text = "Written Test Appointments";
                            break;
                        }
                    default:
                        {
                            lblFormLabel.Text = "Street Test Appointments";
                            break;
                        }
                }
            }

            //Modify DateTimePicker
            if (IsRetakeTest)
            {
                //so Retake Date Not Set Before Last Appointment
                dtpDate.MinDate = clsTestAppointments.GetLastTestAppointment(DLAppID)?.AppointmentDate ?? DateTime.Today;
            }
            else
            {
            dtpDate.MinDate = DateTime.Today;
            }
            
            
        }

        private void _LoadApplicationData()
        {
            if (DLAppID != -1)
            {
                _DLApp = clsLocalDLA.GetLocalDLAppByLocalDLAppID(DLAppID);

                if (_DLApp == null)
                {
                    if (MessageBox.Show("Can't Find this Application with ID of " + DLAppID + ".", "", MessageBoxButtons.OK, MessageBoxIcon.Warning) == DialogResult.OK)
                    {
                        return;
                    }

                }

                _Application = clsApplication.GetApplicationByApplicationID(_DLApp.ApplicationID);

            }
            fillLabels();

        }
        private void fillLabels()
        {
            if (DLAppID != -1)
            {
                    //Schedule Info
                    lbl_DLA_ID2.Text = DLAppID.ToString();
                    lbl_DClass2.Text = _DLApp.ClassName();
                    lblName2.Text = clsPerson.Find(_Application.ApplicantPersonID).FullName;
                    lbl_Trial2.Text = clsTestAppointments.GetTrails(DLAppID, enTestType).ToString();    
                    decimal fees = (_ctrlMode == ctrlMode.Add) ? clsTestType.GetTestType((int)enTestType).Fees : testAppointment.PaidFees;
                    lbl_Fees2.Text = fees.ToString("0.00") ;

                    //checking if the Olddate is Outdated
                    if( _ctrlMode == ctrlMode.Add) { dtpDate.Value = DateTime.Today; }
                    else
                    {
                        if (testAppointment.AppointmentDate > DateTime.Today)
                        {
                            dtpDate.Value = testAppointment.AppointmentDate;
                        }
                        else
                        {
                        dtpDate.Text = testAppointment.AppointmentDate.ToString();
                        }
                    }
#warning this fill label work too soon
                //retake test info
                if (_IsRetakeTest)
                    {
                        lbl_RTestAppID2.Text = (_ctrlMode == ctrlMode.Add) ? "N/A":testAppointment.RetakeTestApplicationID.ToString();
                        decimal RetakeFees = clsApplicationType.GetApplicationType(7).Fees;
                        lblTotalFees2.Text = (fees + RetakeFees).ToString("0.00");
                        lblRAppFees2.Text = RetakeFees.ToString("0.00");
                    }
                    else
                    {
                        lblTotalFees2.Text = fees.ToString();
                        lblRAppFees2.Text = "0";
                    }
                
                



            }
         
        }

        private void btnSave_Click(object sender, EventArgs e)
        {

                testAppointment.enTestType = enTestType;
                testAppointment.PaidFees = clsTestType.GetTestType((int)enTestType).Fees;
                testAppointment.AppointmentDate = Convert.ToDateTime(dtpDate.Text);
                testAppointment.CreatedByUserID = clsLog.User.UserID;
                testAppointment.LocalDrivingLicenseApplicationID = _DLApp.LocalDrivingLicenseApplicationID;

            if (_IsRetakeTest)
            {

                if (_ctrlMode == ctrlMode.Add)
                {
                clsApplication newRetakeApplication =  new clsApplication();
                    newRetakeApplication.ApplicationStatus = (byte)clsApplication.Status.New;
                    newRetakeApplication.ApplicantPersonID = _Application.ApplicantPersonID;
                    newRetakeApplication.ApplicationDate = dtpDate.Value;
                    newRetakeApplication.LastStatusDate = dtpDate.Value;
                    newRetakeApplication.ApplicationTypeID = (int)clsApplicationType.ApplicationType.Retake_Test;//RetakeType ID
                    newRetakeApplication.CreatedByUserID = clsLog.User.UserID;
                    newRetakeApplication.PaidFees = clsApplicationType.GetApplicationType((int)clsApplicationType.ApplicationType.Retake_Test).Fees;

                    if (newRetakeApplication.Save())
                    {
                        testAppointment.RetakeTestApplicationID = newRetakeApplication.ApplicationID;
                        lbl_RTestAppID2.Text = testAppointment.RetakeTestApplicationID.ToString();
                    }
                }


                
            }

            testAppointment.Save();
            btnSave.Enabled = false;
        }

        private void dtpDate_DropDown(object sender, EventArgs e)
        {

        }
    }
}
