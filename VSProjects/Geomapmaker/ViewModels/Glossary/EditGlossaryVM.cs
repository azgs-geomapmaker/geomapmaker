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
    public class EditGlossaryVM : PropertyChangedBase, INotifyDataErrorInfo
    {
        public ICommand CommandUpdate => new RelayCommand(() => Update(), () => CanUpdate());

        public GlossaryVM ParentVM { get; set; }

        private GlossaryTerm _selected;
        public GlossaryTerm Selected
        {
            get => _selected;
            set
            {
                SetProperty(ref _selected, value, () => Selected);
                Term = Selected?.Term;
                Definition = Selected?.Definition;
                DefinitionSourceID = Selected?.DefinitionSourceID;
                NotifyPropertyChanged("Visibility");
            }
        }

        public string Visibility => Selected == null ? "Hidden" : "Visible";

        private string _term;
        public string Term
        {
            get => _term;
            set
            {
                SetProperty(ref _term, value, () => Term);
                ValidateRequiredString(Term, "Term");
                ValidateChangeWasMade();
            }
        }

        private string _definition;
        public string Definition
        {
            get => _definition;
            set
            {
                SetProperty(ref _definition, value, () => Definition);
                ValidateRequiredString(Definition, "Definition");
                ValidateChangeWasMade();
            }
        }

        private string _definitionSourceID;
        public string DefinitionSourceID
        {
            get => _definitionSourceID;
            set
            {
                SetProperty(ref _definitionSourceID, value, () => DefinitionSourceID);
                ValidateRequiredString(DefinitionSourceID, "DefinitionSourceID");
                ValidateChangeWasMade();
            }
        }

        public EditGlossaryVM(GlossaryVM parentVM)
        {
            ParentVM = parentVM;
        }

        private bool CanUpdate()
        {
            return Selected != null && !HasErrors;
        }

        public async void Update()
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
                                            // In order to update the Map and/or the attribute table.
                                            // Has to be called before any changes are made to the row.
                                            context.Invalidate(row);

                                            row["Term"] = Term;
                                            row["Definition"] = Definition;
                                            row["DefinitionSourceID"] = DefinitionSourceID;

                                            // After all the changes are done, persist it.
                                            row.Store();

                                            // Has to be called after the store too.
                                            context.Invalidate(row);
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

        private void ValidateRequiredString(string text, string propertyKey)
        {
            // Required field
            if (string.IsNullOrEmpty(text))
            {
                _validationErrors[propertyKey] = new List<string>() { "" };
            }
            else
            {
                _validationErrors.Remove(propertyKey);
            }

            RaiseErrorsChanged(propertyKey);
        }

        private void ValidateChangeWasMade()
        {
            const string propertyKey = "SilentError";

            if (Selected != null &&
                Term == Selected.Term &&
                Definition == Selected.Definition &&
                DefinitionSourceID == Selected.DefinitionSourceID
                )
            {
                _validationErrors[propertyKey] = new List<string>() { "No changes have been made." };
            }
            else
            {
                _validationErrors.Remove(propertyKey);
            }

            RaiseErrorsChanged(propertyKey);
        }

        #endregion
    }
}
