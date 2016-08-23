using System;
using System.ComponentModel;
using System.Configuration;
using System.DirectoryServices.AccountManagement;
using System.Globalization;
using System.Windows.Forms;

namespace GestionDroits
{
    public partial class Form2 : Form
    {
        PrincipalContext context;

        public Form2(PrincipalContext context, AutoCompleteStringCollection autocomplete)
        {
            this.InitializeComponent();
            this.context = context;
            this.autocomplete.AutoCompleteCustomSource = autocomplete;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Indication de fin de traitement
            Cursor.Current = Cursors.WaitCursor;

            // Suppression des anciennes valeurs
            this.dataGridView1.Rows.Clear();
            this.dataGridView1.Columns.Clear();

            UserPrincipal user = UserPrincipal.FindByIdentity(this.context, this.autocomplete.Text);

            // Mise en forme du nom
            TextInfo textInfo = new CultureInfo("fr-FR", false).TextInfo;
            this.textBox1.Text = textInfo.ToTitleCase(user.Surname.ToLower());

            this.textBox2.Text = user.GivenName;
            this.textBox3.Text = user.VoiceTelephoneNumber;
            this.textBox4.Text = user.EmailAddress;

            // Récupération de la liste
            string[] list = ConfigurationManager.AppSettings.Get("list").Split(';');

            for (int i = 0; i < list.Length; i++)
            {
                DataGridView data = new DataGridView();
                // Ajotu du nom de la colonne
                this.dataGridView1.Columns.Add(list[i], list[i]);
                // Affiche l'ensemble des valeurs automatiquement
                this.dataGridView1.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                // Bloque le tri
                this.dataGridView1.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                // Récupération de la liste des groupes pour l'OU
                data = this.getListGroup(user.SamAccountName, ConfigurationManager.AppSettings.Get(list[i]));
                for (int j = 0; j < data.Rows.Count; j++)
                {
                    try
                    {
                        // Ajout de la valeur
                        this.dataGridView1.Rows[j].Cells[i].Value = data.Rows[j].Cells[0].Value;
                    }
                    // Si la ligne existe pas
                    catch (ArgumentOutOfRangeException)
                    {
                        // Ajout d'une nouvelle ligne
                        this.dataGridView1.Rows.Add();
                        // Ajout de la valeur
                        this.dataGridView1.Rows[j].Cells[i].Value = data.Rows[j].Cells[0].Value;
                    }
                }
            }

            // Indication de fin de traitement
            Cursor.Current = Cursors.Default;
        }

        /// <summary>
        /// Permet de mettre l'ensemble des groupes trouvé avec le filtre passé en argument
        /// dans un DataGridView retourné à la fin de la fonction
        /// </summary>
        /// <param name="userName">Nom de l'utilisateur</param>
        /// <param name="filtre">Chemin de l'OU contenant les groupes cherchés</param>
        /// <returns></returns>
        public DataGridView getListGroup(string userName, string filtre)
        {
            DataGridView data = new DataGridView();
            data.AllowUserToAddRows = false;
            data.Columns.Add("Nom", "Nom");
            // Définition du contexte de recherche
            PrincipalContext yourOU = new PrincipalContext(ContextType.Domain, 
                ConfigurationManager.AppSettings.Get("domain"), filtre);
            // On demande l'ensemble des groupes
            GroupPrincipal findAllGroups = new GroupPrincipal(yourOU, "*");
            PrincipalSearcher ps = new PrincipalSearcher(findAllGroups);
            foreach (var tmp in ps.FindAll())
            {
                GroupPrincipal group = GroupPrincipal.FindByIdentity(yourOU, tmp.Name);

                if (UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, userName).IsMemberOf(group))
                {
                    data.Rows.Add(group.Name);
                }
            }
            // Tri les valeur par ordre alphabétique
            data.Sort(data.Columns[0], ListSortDirection.Ascending);
            return data;
        }
    }
}
