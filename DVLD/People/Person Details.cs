﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DVLD.PeopleForm
{
    public partial class Person_Details : Form
    {
        public Person_Details(int PersonID = -1)
        {
            InitializeComponent();

            uctrPersonDetails1.PersonID = PersonID;
        }

        private void Person_Details_Activated(object sender, EventArgs e)
        {
            uctrPersonDetails1.UpdateControl();
        }
    }
}
