﻿using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.ComponentModel;
using System.Configuration;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Net.Mail;
using System.Windows.Forms;

namespace GestionDroits
{
    /// <summary>
    /// Ce programme permet la gestion des membres d'un liste des groupes. Le login de l'utilisateur
    /// doit être mis dans la description du groupe ainsi que marqué en tant que gestionnaire de
    /// celui-ci.
    /// Sinon, seul le responsable du partage sélectionné sera visible.
    /// </summary>
    public partial class Form1 : Form
    {
        private PrincipalContext context;
        //Récupération de la liste des noms
        private readonly string[] data = (string[])ConfigurationManager.AppSettings.Get("list").Split(';');
        private string groupName;
        // Nom de l'utilisateur actuellement connecté sans le domaine
        private readonly string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split('\\')[1];
        private string mail = "";
        private string userPrincipal = "";

        private Form2 form2;

        int row_count = 0;
        IWorkbook workbook;
        ISheet sheet1;
        XSSFCellStyle red;
        XSSFCellStyle green;

        public Form1()
        {
            this.InitializeComponent();
            try
            {
                this.context = new PrincipalContext(ContextType.Domain);
                this.userPrincipal = UserPrincipal.FindByIdentity(this.context, IdentityType.SamAccountName, userName).DistinguishedName;
            }
            catch (Exception)
            {
                MessageBox.Show("L'ordinateur n'est pas dans le domaine", "Erreur du lancement",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                Environment.Exit(-1);
            }
            this.backgroundWorker1.RunWorkerCompleted += backgroundWorker1_RunWorkerCompleted;
            this.backgroundWorker1.RunWorkerAsync();
            foreach (var item in this.data)
            {
                this.listBox1.Items.Add(item);
            }

            // Excel
            if (File.Exists(ConfigurationManager.AppSettings.Get("pathLog")))
            {
                workbook = new XSSFWorkbook(new FileStream(ConfigurationManager.AppSettings.Get("pathLog"), FileMode.Open, FileAccess.ReadWrite));
                sheet1 = workbook.GetSheetAt(workbook.ActiveSheetIndex);
                if (!(sheet1.SheetName.Equals(DateTime.Now.Year.ToString())))
                {
                    sheet1 = workbook.CreateSheet(DateTime.Now.Year.ToString());
                }
                row_count = sheet1.LastRowNum + 1;
            }
            else
            {
                workbook = new XSSFWorkbook();
                sheet1 = workbook.CreateSheet(DateTime.Now.Year.ToString());
            }

            red = (XSSFCellStyle)workbook.CreateCellStyle();
            red.FillForegroundColor = IndexedColors.Red.Index;
            red.FillPattern = FillPattern.SolidForeground;

            green = (XSSFCellStyle)workbook.CreateCellStyle();
            green.FillForegroundColor = IndexedColors.Green.Index;
            green.FillPattern = FillPattern.SolidForeground;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Indication d'un traitement à l'utilisateur
            Cursor.Current = Cursors.WaitCursor;

            // Purge de la liste des groupes
            this.groupList.Rows.Clear();

            this.getListGroup(this.groupList.Rows, ConfigurationManager.AppSettings.Get(listBox1.Text));

            // Indication de fin de traitement
            Cursor.Current = Cursors.Default;
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            this.getMembers();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.AddUserToGroup(this.autocomplete.Text, groupName);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.RemoveUserFromGroup(groupName);
        }

        // ====================================================================

        /// <summary>
        /// Ajouter l'utilisateur choisi dans la liste au groupe séléctionné
        /// </summary>
        /// <param name="userId">Nom de l'utilisateur</param>
        /// <param name="groupName">Nom du groupe</param>
        private void AddUserToGroup(string userName, string groupName)
        {
            try
            {
                GroupPrincipal group = GroupPrincipal.FindByIdentity(context, groupName);
                UserPrincipal oUserPrincipal = UserPrincipal.FindByIdentity(context, userName);
                group.Members.Add(oUserPrincipal);
                group.Save();
                IRow row = sheet1.CreateRow(row_count);
                row.CreateCell(0).SetCellValue(userPrincipal.Split('=')[1].Split(',')[0]);
                ICell cell = row.CreateCell(1);
                cell.SetCellValue("Ajout");
                cell.CellStyle = green;
                row.CreateCell(2).SetCellValue(groupName);
                row.CreateCell(3).SetCellValue(userName);
                row_count++;
                MessageBox.Show("L'utilisateur " + userName + " ajouté avec succès !",
                    "Ajout", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                this.getMembers();
            }
            catch (System.DirectoryServices.DirectoryServicesCOMException) { }
            catch (System.ArgumentNullException)
            {
                MessageBox.Show("Impossible de trouver l'utilisateur. Veuillez réessayer.",
                    "Erreur lors de l'ajout", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
            }
            catch (System.UnauthorizedAccessException)
            {
                MessageBox.Show("Vous n'avez pas les droits pour modificher le groupe.", "Erreur lors de la modification",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
            }
        }

        /// <summary>
        /// Supprime l'utilisateur sélectionné du groupe choisi
        /// </summary>
        /// <param name="groupName">Nom du groupe</param>
        private void RemoveUserFromGroup(string groupName)
        {
            try
            {
                GroupPrincipal group = GroupPrincipal.FindByIdentity(context, groupName);
                UserPrincipal oUserPrincipal = UserPrincipal.FindByIdentity(context, dataMembers.SelectedCells[0].Value.ToString());
                group.Members.Remove(oUserPrincipal);
                group.Save();
                IRow row = sheet1.CreateRow(row_count);
                row.CreateCell(0).SetCellValue(userPrincipal.Split('=')[1].Split(',')[0]);
                ICell cell = row.CreateCell(1);
                cell.SetCellValue("Suppression");
                cell.CellStyle = red;
                row.CreateCell(2).SetCellValue(groupName);
                row.CreateCell(3).SetCellValue(dataMembers.SelectedCells[0].Value.ToString());
                row_count++;
                MessageBox.Show("L'utilisateur " + dataMembers.SelectedCells[0].Value.ToString() + " supprimé avec succès !",
                    "Suppression", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                this.getMembers();
            }
            catch (System.DirectoryServices.DirectoryServicesCOMException) { }
            catch (System.UnauthorizedAccessException)
            {
                MessageBox.Show("Vous n'avez pas les droits pour modificher le groupe.", "Erreur lors de la modification",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
            }
        }
        
        /// <summary>
        /// Permet l'envoie d'un mail au responsable afin de demander l'ajout
        /// d'un utilisateur au partage sélectionné
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click_1(object sender, EventArgs e)
        {
            // Récupération des informations de l'utilisateur courant
            if (autocomplete.Text.Equals(""))
            {
                MessageBox.Show("Merci de choisir un utilisateur dans la liste.",
                    "Erreur lors de l'envoie du mail", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                return;
            }
            UserPrincipal user = UserPrincipal.FindByIdentity(context, autocomplete.Text);

            if (user == null)
            {
                MessageBox.Show("L'utilisateur choisi n'existe pas ou n'a pas de mail renseigné. Envoie du mail impossible.",
                    "Erreur lors de l'envoie du mail", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                return;
            }
            if ((this.mail == null) || (this.mail.Equals("")))
            {
                MessageBox.Show("Le responsable n'a pas de mail renseigné. Envoie du mail impossible.",
                    "Erreur lors de l'envoie du mail", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                return;
            }

            // Création du mail
            MailMessage mail = new MailMessage(ConfigurationManager.AppSettings.Get("mail"), this.mail);
            mail.CC.Add(user.EmailAddress);
            SmtpClient client = new SmtpClient();
            client.Port = 25;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Host = ConfigurationManager.AppSettings.Get("mailserver");
            mail.Subject = "[Partage] Demande d'accès : " + groupList.SelectedCells[0].Value.ToString();
            mail.Body = "L'utilisateur " + this.autocomplete.Text + " demande l'accès au partage " + groupList.SelectedCells[0].Value.ToString() +
                ". Lancer l'application GestionDroits afin de l'ajouter si vous validez la demande.";
            // Envoie du mail aux destinataires
            client.Send(mail);
            MessageBox.Show("Mail envoyé avec succès !",
                "Envoyé", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        // ====================================================================
        //          MENU
        private void aideToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("La documentation sur l'utilisation du logiciel est disponible ici : " + ConfigurationManager.AppSettings.Get("pathDoc"),
                "Aide", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        private void aProposToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Développement fait par Nicolas Le Gall\nDate : 04/07/2016",
                "A propos", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }
        // ====================================================================

        // ====================================================================
        //          THREAD AUTO-COMPLETION
        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            // ----------------------------------------------------------------
            // Mise en place de l'auto-completion
            UserPrincipal u = new UserPrincipal(context);
            PrincipalSearcher search = new PrincipalSearcher(u);
            AutoCompleteStringCollection dataCompletion = new AutoCompleteStringCollection();
            foreach (UserPrincipal result in search.FindAll())
            {
                if (result != null && result.DisplayName != null && !result.SamAccountName.ToUpper().Contains("ADM"))
                {
                    dataCompletion.Add(result.Name);
                }
            }
            e.Result = dataCompletion;
            // ----------------------------------------------------------------
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((e.Cancelled) || (e.Error != null))
                MessageBox.Show("Operation was canceled");

            this.autocomplete.AutoCompleteCustomSource = (AutoCompleteStringCollection)e.Result;

            PrincipalContext yourOU = new PrincipalContext(ContextType.Domain, ConfigurationManager.AppSettings.Get("domain"),
                ConfigurationManager.AppSettings.Get(listBox1.Text));
            GroupPrincipal group = GroupPrincipal.FindByIdentity(context, "utilisateurs gestionDroits VIP");
            if (UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, userName).IsMemberOf(group))
            {
                this.détailsUtilisateurToolStripMenuItem.Visible = true;
                this.détailsUtilisateurToolStripMenuItem.Enabled = true;
            }
        }
        // ====================================================================

        private void getMembers()
        {

            try
            {
                // Indication d'un traitement à l'utilisateur
                Cursor.Current = Cursors.WaitCursor;

                // Désactivation des contrôles de droites
                this.dataMembers.Enabled = false;
                this.deleteButton.Enabled = false;
                this.addButton.Enabled = false;
                this.sendMail.Enabled = false;
                this.autocomplete.Enabled = false;

                // Suppresion des anciennes valeurs
                this.dataMembers.Rows.Clear();
                this.mail = "";

                this.groupName = groupList.SelectedCells[0].Value.ToString();
                GroupPrincipal group = GroupPrincipal.FindByIdentity(this.context, this.groupName);
                DirectoryEntry groupProperties = (DirectoryEntry)group.GetUnderlyingObject();
                PropertyValueCollection managedBy = groupProperties.Properties["managedBy"];
                UserPrincipal gestionnaire = null;
                // Récupération du nom complet du gestionnaire du groupe
                if (managedBy.Value != null)
                {
                    gestionnaire = UserPrincipal.FindByIdentity(this.context, managedBy.Value.ToString());
                    this.responsable.Text = gestionnaire.Name;
                    this.mail = gestionnaire.EmailAddress;

                    this.sendMail.Enabled = true;
                    this.autocomplete.Enabled = true;
                }
                else
                {
                    this.responsable.Text = "Aucun responsable défini";
                }

                if (gestionnaire != null && this.userName.Equals(gestionnaire.SamAccountName))
                {
                    // Ajout des membres dans la liste
                    foreach (Principal p in group.GetMembers())
                    {
                        this.dataMembers.Rows.Add(p.Name);
                    }

                    // Activation des crontrôles pour le gestionnaires
                    this.dataMembers.Enabled = true;
                    this.deleteButton.Enabled = true;
                    this.addButton.Enabled = true;
                    this.sendMail.Enabled = false;
                }

                // Indication de fin de traitement
                Cursor.Current = Cursors.Default;

            }
            catch (System.Exception) { }
        }

        public void getListGroup(DataGridViewRowCollection data, string filtre)
        {
            // Définition du contexte de recherche
            PrincipalContext yourOU = new PrincipalContext(ContextType.Domain, ConfigurationManager.AppSettings.Get("domain"),
                filtre);
            // On demande l'ensemble des groupes
            GroupPrincipal findAllGroups = new GroupPrincipal(yourOU, "*");
            PrincipalSearcher ps = new PrincipalSearcher(findAllGroups);
            foreach (var tmp in ps.FindAll())
            {
                GroupPrincipal group = GroupPrincipal.FindByIdentity(yourOU, tmp.Name);
                var tmp2 = (DirectoryEntry)group.GetUnderlyingObject();
                var tmp3 = tmp2.Properties["managedBy"].Value;

                if (((tmp3 != null) && (tmp3.Equals(userPrincipal))) || (UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, userName).IsMemberOf(group)))
                {
                    data.Add(group.Name);
                }
            }
        }

        private void détailsUtilisateurToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.form2 = new Form2(this.context, this.autocomplete.AutoCompleteCustomSource);
            //form2.MdiParent = this;
            form2.Show();
            //form2.WindowState = FormWindowState.Maximized;

        }

        /// <summary>
        /// Sauvegarde le fichier 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            FileStream sw = new FileStream(ConfigurationManager.AppSettings.Get("pathLog"), FileMode.Create, FileAccess.ReadWrite);
            workbook.Write(sw);
            sw.Close();
        }
    }
}