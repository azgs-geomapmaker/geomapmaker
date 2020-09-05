using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Mapping;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Geomapmaker
{
    /// <summary>
    /// Interaction logic for LoginDialogProWindow.xaml
    /// </summary>
    public partial class LoginDialogProWindow : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        Npgsql.NpgsqlConnection conn = new NpgsqlConnection("Server=127.0.0.1;User Id=geomapmaker; " +
           "Password=password;Database=geomapmaker;");

        public LoginDialogProWindow()
        {
            InitializeComponent();

            //NpgsqlConnection conn = new NpgsqlConnection("Server=127.0.0.1;User Id=douglas; " +
            //   "Password=password;Database=geomapmaker;");
            conn.Open();
            NpgsqlCommand command = new NpgsqlCommand("SELECT id, name, notes FROM geomapmaker.users order by name asc", conn);
            NpgsqlDataReader dr = command.ExecuteReader();

            DataTable dT = new DataTable();
            dT.Load(dr);

            foreach (DataRow row in dT.Rows)
            {
                Debug.Write("Hi there \n");
                //Debug.Write("{0} \n", row["name"].ToString());
                Debug.WriteLine(row["name"].ToString());
            }


            UserCombo.ItemsSource = dT.DefaultView;
            UserCombo.DisplayMemberPath = "name";  //TODO: figure out why this is require here but not in LoginDialogWPF?
            UserCombo.SelectedIndex = -1;
            UserCombo.IsTextSearchEnabled = true;
            UserCombo.IsEditable = true;
            //UserCombo.Style = Com ComboBoxStyle.DropDown;
            //UserCombo. AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            //UserCombo.AutoCompleteSource = AutoCompleteSource.ListItems;
            UserCombo.SelectionChanged += nameSelected;

            // Output rows
            //while (dr.Read())
            //    Console.Write("{0} \n", dr[0]);

            //NpgsqlCommand command = new NpgsqlCommand("SELECT name FROM public.users", conn);

            // Execute the query and obtain a result set
            //DbDataReader dr = command.ExecuteDbDataReader(SingleResult);


            conn.Close();
        }

        private void nameSelected(object sender, System.EventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;

            System.Data.DataRowView selectedUser = (System.Data.DataRowView)comboBox.SelectedItem;
            Debug.WriteLine("selected user = " + selectedUser);
            if (selectedUser != null)
            {
                NotesTextBox.Text = selectedUser.Row.Field<String>("notes");
            }
        }

        private async void loginButton_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("selected index = " + UserCombo.SelectedIndex);

            //Clean up map and other stuff from old user
            await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
            {
                var map = MapView.Active.Map;
                map.RemoveLayers(DataHelper.currentLayers);
                map.RemoveStandaloneTables(DataHelper.currentTables);
                DataHelper.currentLayers.Clear();
                DataHelper.currentTables.Clear();
            });
            FrameworkApplication.State.Deactivate("project_selected");
            ArcGIS.Desktop.Framework.Contracts.DockPane dockPane = FrameworkApplication.DockPaneManager.Find("Geomapmaker_AddEditMapUnitsDockPane");
            if (dockPane != null) {
                dockPane.IsVisible = false;
            }

            conn.Open();
            NpgsqlCommand command;
            if (UserCombo.SelectedIndex == -1)
            {
                Debug.WriteLine("new name");
                command = new NpgsqlCommand("insert into geomapmaker.users (name, notes) values ($c$" + UserCombo.Text + "$c$, $n$" + NotesTextBox.Text + "$n$) returning id", conn);
            }
            else
            {
                Debug.WriteLine("old name");
                command = new NpgsqlCommand("update geomapmaker.users set notes = $n$" + NotesTextBox.Text + "$n$ where name = $c$" + UserCombo.Text + "$c$ returning id", conn);
            }
            int userID = (int)command.ExecuteScalar();
            conn.Close();
            DataHelper.UserLogin(userID, UserCombo.Text);
            Debug.WriteLine("selected text = " + UserCombo.Text);
            Debug.WriteLine("LoginDialog, userID = " + DataHelper.userID);
            // UserSelectionFinishedEventArgs userSelectionFinishedEventArgs = new UserSelectionFinishedEventArgs(this.parentModule.userID); //TODO: using module context for now

            //FrameworkApplication.EventAggregator.GetEvent<UserSelectionFinishedEvent>().Publish(
            //UserSelectionFinishedEvent.Publish(userSelectionFinishedEventArgs);
            FrameworkApplication.State.Activate("user_logged_in");
            //addFeatureLayer();
            this.DialogResult = true;// DialogResult.OK;
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = false;  //DialogResult.Cancel;
        }

    }


}
