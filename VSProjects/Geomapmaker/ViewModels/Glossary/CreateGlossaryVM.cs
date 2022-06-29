using ArcGIS.Core.Data;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Geomapmaker.ViewModels.Glossary
{
    public class CreateGlossaryVM : PropertyChangedBase, INotifyDataErrorInfo
    {
        public ICommand CommandSave => new RelayCommand(() => Save(), () => CanSave());

        public GlossaryVM ParentVM { get; set; }

        public CreateGlossaryVM(GlossaryVM parentVM)
        {
            ParentVM = parentVM;

            // Trigger validation
            Term = "";
            Definition = "";
        }

        private bool CanSave()
        {
            return !HasErrors;
        }

        public async void Save()
        {
            string errorMessage = null;

            StandaloneTable standalone = MapView.Active?.Map.StandaloneTables.FirstOrDefault(a => a.Name == "Glossary");

            if (standalone == null)
            {
                MessageBox.Show("Glossary table not found in active map.", "One or more errors occured.");
                return;
            }

            await QueuedTask.Run(() =>
            {
                try
                {
                    using (Table enterpriseTable = standalone.GetTable())
                    {
                        if (enterpriseTable != null)
                        {
                            EditOperation editOperation = new EditOperation();

                            editOperation.Callback(context =>
                            {
                                TableDefinition tableDefinition = enterpriseTable.GetDefinition();

                                using (RowBuffer rowBuffer = enterpriseTable.CreateRowBuffer())
                                {
                                    rowBuffer["term"] = Term;
                                    rowBuffer["definition"] = Definition;
                                    rowBuffer["definitionsourceid"] = DefinitionSourceID;
                                    rowBuffer["glossary_id"] = Guid.NewGuid().ToString();

                                    using (Row row = enterpriseTable.CreateRow(rowBuffer))
                                    {
                                        // To Indicate that the attribute table has to be updated.
                                        context.Invalidate(row);
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

        private string _term;
        public string Term
        {
            get => _term;
            set
            {
                SetProperty(ref _term, value, () => Term);
                ValidateUniqueTerm(Term, "Term");
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
            }
        }

        private string _definitionSourceID = GeomapmakerModule.DataSourceId;
        public string DefinitionSourceID
        {
            get => _definitionSourceID;
            set
            {
                SetProperty(ref _definitionSourceID, value, () => DefinitionSourceID);
                ValidateRequiredString(DefinitionSourceID, "DefinitionSourceID");
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

        private void ValidateUniqueTerm(string text, string propertyKey)
        {
            // Required field
            if (string.IsNullOrWhiteSpace(text))
            {
                _validationErrors[propertyKey] = new List<string>() { "" };
            }
            else if (ParentVM.Terms.Any(a => a.Term.ToLower() == text?.ToLower()))
            {
                _validationErrors[propertyKey] = new List<string>() { "Term is taken." };
            }
            else
            {
                _validationErrors.Remove(propertyKey);
            }

            RaiseErrorsChanged(propertyKey);
        }

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

        #endregion
    }
}
