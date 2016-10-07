using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Net.Mail;
using System.Linq;
using System.Windows.Forms;

namespace GestionDroits
{
    public partial class Form3 : Form
    {
        public string user;
        PrincipalContext context;
        Dictionary<string, string> groups = new Dictionary<string, string>();

        public Form3(AutoCompleteStringCollection autocomplete)
        {
            InitializeComponent();
            this.autocomplete.AutoCompleteCustomSource = autocomplete;
            this.context = new PrincipalContext(ContextType.Domain);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listView1.CheckedItems.Count == 0)
            {
                MessageBox.Show("Cochez au moins une case dans la liste",
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                return;
            }
            foreach (ListViewItem item in listView1.CheckedItems)
            {
                UserPrincipal user = UserPrincipal.FindByIdentity(context, groups[item.Text]);
                MailMessage mail = new MailMessage(ConfigurationManager.AppSettings.Get("mail"), user.EmailAddress);
                SmtpClient client = new SmtpClient();
                client.Port = 25;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Host = ConfigurationManager.AppSettings.Get("mailserver");
                mail.Subject = "[Partage] Demande d'accès : " + item.Text;
                mail.Body = "L'utilisateur " + this.autocomplete.Text + " demande l'accès au partage " + item.Text +
                    ". Lancer l'application GestionDroits afin de l'ajouter si vous validez la demande.";
                // Envoie du mail aux destinataires
                client.Send(mail);
            }
            MessageBox.Show("L'ensembles des mails ont été envoyés avec succès !",
                "Envoyé", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string val = this.autocomplete.Text;

            // Définition du contexte de recherche
            PrincipalContext yourOU = new PrincipalContext(ContextType.Domain, ConfigurationManager.AppSettings.Get("domain"),
                ConfigurationManager.AppSettings.Get("Partages"));
            // On demande l'ensemble des groupes
            GroupPrincipal findAllGroups = new GroupPrincipal(yourOU, "*");
            PrincipalSearcher ps = new PrincipalSearcher(findAllGroups);
            foreach (var tmp in ps.FindAll())
            {
                GroupPrincipal group = GroupPrincipal.FindByIdentity(yourOU, tmp.Name);

                Console.WriteLine(group.Name);

                if (UserPrincipal.FindByIdentity(context, IdentityType.Name, val).IsMemberOf(group))
                {
                    var tmp2 = (DirectoryEntry)group.GetUnderlyingObject();
                    var tmp3 = tmp2.Properties["managedBy"].Value;

                    this.listView1.Items.Add(group.Name);
                    groups.Add(group.Name, tmp3.ToString());
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
