using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GestionDroits
{
    public partial class Form3 : Form
    {
        public string user;

        public Form3(AutoCompleteStringCollection autocomplete)
        {
            InitializeComponent();
            this.autocomplete.AutoCompleteCustomSource = autocomplete;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.user = this.autocomplete.Text;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
