using ArcGIS.Core.Data;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Geomapmaker.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace Geomapmaker.ViewModels.Glossary
{
    public class DeleteGlossaryVM : PropertyChangedBase, INotifyDataErrorInfo
    {
        public ICommand CommandDelete => new RelayCommand(() => Delete(), () => CanDelete());

        public GlossaryVM ParentVM { get; set; }

        private GlossaryTerm _selected;
        public GlossaryTerm Selected
        {
            get => _selected;
            set
            {
                SetProperty(ref _selected, value, () => Selected);

                Term = Selected?.Term;
                NotifyPropertyChanged("Term");

                Definition = Selected?.Definition;
                NotifyPropertyChanged("Definition");

                DefinitionSourceID = Selected?.DefinitionSourceID;
                NotifyPropertyChanged("DefinitionSourceID");

                NotifyPropertyChanged("Visibility");
            }
        }

        public string Visibility => Selected == null ? "Hidden" : "Visible";

        public string Term { get; set; }
        public string Definition { get; set; }
        public string DefinitionSourceID { get; set; }

        public DeleteGlossaryVM(GlossaryVM parentVM)
        {
            ParentVM = parentVM;
        }

        private bool CanDelete()
        {
            return Selected != null && !HasErrors;
        }

        public async void Delete()
        {
            string errorMessage = null;

            StandaloneTable table = MapView.Active?.Map.StandaloneTables.FirstOrDefault(a => a.Name == "Glossary");

            if (table == null)
            {
                MessageBox.Show("Glossary table not found in active map.", "One or more errors occured.");
                return;
            }

            await QueuedTask.Run(() =>
            {
                try
                {
                    using (Table enterpriseTable = table.GetTable())
                    {
                        if (enterpriseTable != null)
                        {
                            EditOperation editOperation = new EditOperation();

                            editOperation.Callback(context =>
                            {
                                QueryFilter filter = new QueryFilter { ObjectIDs = new List<long> { Selected.ObjectId } };

                                using (RowCursor rowCursor = enterpriseTable.Search(filter, false))
                                {
                                    while (rowCursor.MoveNext())
                                    {
                                        using (Row row = rowCursor.Current)
                                        {
                                            context.Invalidate(row);

                                            row.Delete();
                                        }
                                    }
                                }
                            }, enterpriseTable);

                            bool result = editOperation.Execute();
                        }
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = ex.InnerException == null ? ex.Message : ex.InnerException.ToString();

                    // Trim the stack-trace from the error msg
                    if (errorMessage.Contains("--->"))
                    {
                        errorMessage = errorMessage.Substring(0, errorMessage.IndexOf("--->"));
                    }
                }
            });

            if (!string.IsNullOrEmpty(errorMessage))
            {
                MessageBox.Show(errorMessage, "One or more errors occured.");
            }
            else
            {
                ParentVM.CloseProwindow();
            }
        }

        #region Validation

        private readonly Dictionary<string, ICollection<string>> _validationErrors = new Dictionary<string, ICollection<string>>();

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        private void RaiseErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        public System.Collections.IEnumerable GetErrors(string propertyName)
        {
            // Return null if parameters is null/empty OR there are no errors for that parameter
            // Otherwise, return the errors for that parameter.
            return string.IsNullOrEmpty(propertyName) || !_validationErrors.ContainsKey(propertyName) ?
                null : (System.Collections.IEnumerable)_validationErrors[propertyName];
        }

        public bool HasErrors => _validationErrors.Count > 0;

        #endregion
    }
}
