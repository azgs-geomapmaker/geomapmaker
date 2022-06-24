using ArcGIS.Core.Data;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Controls;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Geomapmaker.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Geomapmaker.ViewModels.Glossary
{
    public class GlossaryVM : ProWindow, INotifyPropertyChanged, INotifyDataErrorInfo
    {
        public event EventHandler WindowCloseEvent;

        public ICommand CommandCancel => new RelayCommand(() => CloseProwindow());

        public ICommand CommandSkip => new RelayCommand(() => Skip(), () => CanSkip());

        public ICommand CommandSave => new RelayCommand(() => Save(), () => CanSave());

        public GlossaryVM()
        {
            // Trigger validation
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
                Skip();
            }
        }

        private bool CanSkip()
        {
            return currentIndex < UndefinedTerms?.Count - 1;
        }

        public void Skip()
        {
            if (currentIndex < UndefinedTerms?.Count - 1)
            {
                currentIndex++;
                SetValues();
            }
            else
            {
                CloseProwindow();
            }
        }

        public void CloseProwindow()
        {
            WindowCloseEvent(this, new EventArgs());
        }

        private int currentIndex = 0;

        public async void GetUndefinedTerms()
        {
            UndefinedTerms = await Data.Glossary.GetUndefinedGlossaryTerms();

            if (UndefinedTerms.Count == 0)
            {
                Heading = "No Undefined Glossary Terms";
            }
            else
            {
                SetValues();
            }
        }

        private void SetValues()
        {
            Heading = $"{currentIndex + 1} of {UndefinedTerms.Count} Undefined Glossary Terms";
            Dataset = UndefinedTerms[currentIndex].DatasetName;
            Field = UndefinedTerms[currentIndex].FieldName;
            Term = UndefinedTerms[currentIndex].Term;
            Definition = UndefinedTerms[currentIndex].Definition;
        }

        private List<GlossaryTerm> _undefinedTerms { get; set; } = new List<GlossaryTerm>();
        public List<GlossaryTerm> UndefinedTerms
        {
            get => _undefinedTerms;
            set
            {
                _undefinedTerms = value;
                NotifyPropertyChanged();
            }
        }

        private string _heading { get; set; }
        public string Heading
        {
            get => _heading;
            set
            {
                _heading = value;
                NotifyPropertyChanged();
            }
        }

        private string _dataset { get; set; }
        public string Dataset
        {
            get => _dataset;
            set
            {
                _dataset = value;
                NotifyPropertyChanged();
            }
        }

        private string _field { get; set; }
        public string Field
        {
            get => _field;
            set
            {
                _field = value;
                NotifyPropertyChanged();
            }
        }

        private string _term { get; set; }
        public string Term
        {
            get => _term;
            set
            {
                _term = value;
                NotifyPropertyChanged();
            }
        }

        private string _definition { get; set; }
        public string Definition
        {
            get => _definition;
            set
            {
                _definition = value;
                ValidateRequiredString(Definition, "Definition");
                NotifyPropertyChanged();
            }
        }

        private string _definitionSourceID = GeomapmakerModule.DataSourceId;
        public string DefinitionSourceID
        {
            get => _definitionSourceID;
            set
            {
                _definitionSourceID = value;
                ValidateRequiredString(DefinitionSourceID, "DefinitionSourceID");
                NotifyPropertyChanged();
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

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    internal class ShowGlossary : Button
    {
        private Views.Glossary.Glossary _glossary = null;

        protected override async void OnClick()
        {
            //already open?
            if (_glossary != null)
            {
                _glossary.Close();
                return;
            }

            _glossary = new Views.Glossary.Glossary
            {
                Owner = System.Windows.Application.Current.MainWindow
            };

            _glossary.Closed += (o, e) =>
            {
                _glossary = null;
            };

            _glossary.glossaryVM.WindowCloseEvent += (s, e) => _glossary.Close();

            _glossary.Show();

            // Display progress dialog
            ProgressorSource ps = new ProgressorSource("Finding Undefined Glossary Terms");

            await QueuedTask.Run(() =>
            {
                _glossary.glossaryVM.GetUndefinedTerms();
            }, ps.Progressor);
        }
    }
}
