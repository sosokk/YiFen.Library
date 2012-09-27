using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace YiFen.TemplateGenerator
{
    public partial class Generator : Form
    {
        public Generator()
        {
            InitializeComponent();
            PathText.Text = AppDomain.CurrentDomain.BaseDirectory;
            DataTypeComboBox.SelectedItem = "SQLServer";
        }

        private void EntityButton_Click(object sender, EventArgs e)
        {
            List<TableInfo> tables = Utility.GetInformation(DataTypeComboBox.SelectedItem.ToString())
                .Tables(ConnectionText.Text);
            Utility.GeneratorEntity(tables, EntityNameSpaceText.Text, PathText.Text);
            MessageBox.Show(tables.Count.ToString());
        }

        private void ServiceButton_Click(object sender, EventArgs e)
        {
            List<TableInfo> tables = Utility.GetInformation(DataTypeComboBox.SelectedItem.ToString())
                .Tables(ConnectionText.Text);
            Utility.GeneratorService(tables, ServiceNameSpaceText.Text, EntityNameSpaceText.Text, PathText.Text);
            MessageBox.Show(tables.Count.ToString());
        }


    }
}
