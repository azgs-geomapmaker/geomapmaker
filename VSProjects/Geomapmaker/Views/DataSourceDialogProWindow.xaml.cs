using ArcGIS.Core.Data;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework;
using Geomapmaker.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Geomapmaker
{
    /// <summary>
    /// Interaction logic for DataSourceDialogProWindow.xaml
    /// </summary>
    public partial class DataSourceDialogProWindow : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public DataSourceDialogProWindow()
        {
            InitializeComponent();

            SourceCombo.ItemsSource = DataHelper.DataSources;
            SourceCombo.DisplayMemberPath = "Source";  //TODO: figure out why this is require here but not in LoginDialogWPF?
            SourceCombo.SelectedIndex = -1;
            SourceCombo.IsTextSearchEnabled = true;
            SourceCombo.IsEditable = true;
            SourceCombo.SelectionChanged += sourceSelected;
        }

        //This is to capture the user entering a new source (not in the list)
        private string userEnteredSource;
        public string UserEnteredSource
        {
            get
            {
                return userEnteredSource;
            }
            set
            {
                //if (userEnteredMapUnit != value) {
                userEnteredSource = value;
                if (value == null)
                {
                    DataHelper.DataSource = null;
                }
                else if (!DataHelper.DataSources.Any(source => source.Source == value))
                {
                    //userEnteredMapUnit = value;
                    var dataSource = new DataSource();
                    dataSource.Source = userEnteredSource;
                    DataHelper.DataSource = dataSource;
                }
            }
        }


        private async void selectButton_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("selected index = " + SourceCombo.SelectedIndex);

            DataSource source = (DataSource)SourceCombo.SelectedItem;
            if (source == null)
            {
                source = new DataSource()
                {
                    Source = SourceCombo.Text,
                    Url = URLTextBox.Text,
                    Notes = NotesTextBox.Text
                };
            }

            bool newSource = SourceCombo.SelectedIndex == -1;

            await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
            {
                string message = String.Empty;
                bool result = false;
                EditOperation editOperation = new EditOperation();

                using (Geodatabase geodatabase = new Geodatabase(DataHelper.connectionProperties))
                {
                    using (Table enterpriseTable = geodatabase.OpenDataset<Table>("DataSources"))
                    {

                        if (newSource/*SourceCombo.SelectedIndex == -1*/)
                        {
                            //new source
                            editOperation.Callback(context =>
                            {
                                TableDefinition tableDefinition = enterpriseTable.GetDefinition();
                                using (RowBuffer rowBuffer = enterpriseTable.CreateRowBuffer())
                                {
                                    // Either the field index or the field name can be used in the indexer.
                                    rowBuffer["Source"] = source.Source;
                                    rowBuffer["Url"] = source.Url;
                                    rowBuffer["Notes"] = source.Notes;

                                    using (Row row = enterpriseTable.CreateRow(rowBuffer))
                                    {
                                        // To Indicate that the attribute table has to be updated.
                                        context.Invalidate(row);
                                    }
                                }
                            }, enterpriseTable);
                        }
                        else
                        {
                            //existing source

                            editOperation.Callback(context =>
                            {
                                QueryFilter filter = new QueryFilter { WhereClause = "objectid = " + source.ID };

                                using (RowCursor rowCursor = enterpriseTable.Search(filter, false))
                                {
                                    TableDefinition tableDefinition = enterpriseTable.GetDefinition();
                                    int subtypeFieldIndex = tableDefinition.FindField(tableDefinition.GetSubtypeField());

                                    while (rowCursor.MoveNext())
                                    { //TODO: Anything? Should be only one
                                        using (Row row = rowCursor.Current)
                                        {
                                            // In order to update the Map and/or the attribute table.
                                            // Has to be called before any changes are made to the row.
                                            context.Invalidate(row);

                                            row["Source"] = source.Source;
                                            row["Url"] = source.Url;
                                            row["Notes"] = source.Notes;

                                            //After all the changes are done, persist it.
                                            row.Store();

                                            // Has to be called after the store too.
                                            context.Invalidate(row);
                                        }
                                    }
                                }
                            }, enterpriseTable);
                        }

                        try
                        {
                            result = editOperation.Execute();
                            if (!result) message = editOperation.ErrorMessage;
                        }
                        catch (GeodatabaseException exObj)
                        {
                            message = exObj.Message;
                        }

                    }
                }

                if (!string.IsNullOrEmpty(message))
                    MessageBox.Show(message);
                else
                {
                    //UserEnteredSource = null; 
                }

            });

            await DataHelper.PopulateDataSources();
            DataHelper.DataSource = DataHelper.DataSources.First(dataSource => dataSource.Source == source.Source);
            FrameworkApplication.State.Activate("datasource_selected");

            this.DialogResult = true;  //DialogResult.OK;

        }

        private void sourceSelected(object sender, System.EventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;

            DataSource selectedDS = (DataSource)comboBox.SelectedItem;
            Debug.WriteLine("selected DS = " + selectedDS);
            if (selectedDS != null)
            {
                NotesTextBox.Text = selectedDS.Notes;
                URLTextBox.Text = selectedDS.Url;
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = false;  //DialogResult.Cancel;
        }

    }
}
