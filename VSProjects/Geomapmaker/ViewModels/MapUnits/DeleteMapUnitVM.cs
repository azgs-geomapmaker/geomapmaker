using ArcGIS.Core.Data;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using Geomapmaker.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Geomapmaker.ViewModels.MapUnits
{
    public class DeleteMapUnitVM : DockPane, INotifyDataErrorInfo
    {
        // Deletes's save button
        public ICommand CommandDelete { get; }
        public ICommand CommandReset { get; }

        public DeleteMapUnitVM()
        {
            // Init commands
            CommandDelete = new RelayCommand(() => DeleteMapUnitAsync(), () => CanDelete());
            CommandReset = new RelayCommand(() => ResetAsync());

            // Trigger validation
            ValidateCanDelete();
        }

        public ObservableCollection<MapUnit> AllMapUnits => new ObservableCollection<MapUnit>(Data.DescriptionOfMapUnits.MapUnits);

        private MapUnit _selected;
        public MapUnit Selected
        {
            get => _selected;
            set
            {
                SetProperty(ref _selected, value, () => Selected);

                MapUnit = Selected?.MU;
                NotifyPropertyChanged("MapUnit");

                Name = Selected?.Name;
                NotifyPropertyChanged("Name");

                FullName = Selected?.FullName;
                NotifyPropertyChanged("FullName");

                Age = Selected?.Age;
                NotifyPropertyChanged("Age");

                RelativeAge = Selected?.RelativeAge;
                NotifyPropertyChanged("RelativeAge");

                Description = Selected?.Description;
                NotifyPropertyChanged("Description");

                Label = Selected?.Label;
                NotifyPropertyChanged("Label");

                HexColor = MapUnitsViewModel.RGBtoHex(Selected?.AreaFillRGB);
                NotifyPropertyChanged("HexColor");

                GeoMaterial = Selected?.GeoMaterial;
                NotifyPropertyChanged("GeoMaterial");

                GeoMaterialConfidence = Selected?.GeoMaterialConfidence;
                NotifyPropertyChanged("GeoMaterialConfidence");

                ValidateCanDelete();
            }
        }

        public string MapUnit { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public string Age { get; set; }
        public string RelativeAge { get; set; }
        public string Description { get; set; }
        public string Label { get; set; }
        public string HexColor { get; set; }
        public string GeoMaterial { get; set; }
        public string GeoMaterialConfidence { get; set; }

        private bool CanDelete()
        {
            return !HasErrors;
        }

        private async Task ResetAsync()
        {
            // Refresh map unit data
            await Data.DescriptionOfMapUnits.RefreshMapUnitsAsync();

            NotifyPropertyChanged("AllMapUnits");

            Selected = null;
        }

        private async Task DeleteMapUnitAsync()
        {
            MessageBoxResult messageBoxResult = MessageBox.Show($"Are you want to delete {Name}?", $"Delete {Name}?", System.Windows.MessageBoxButton.YesNo);

            if (messageBoxResult == MessageBoxResult.No)
            {
                return;
            }

            if (Data.DbConnectionProperties.GetProperties() == null)
            {
                return;
            }

            await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
            {

                EditOperation editOperation = new EditOperation();

                using (Geodatabase geodatabase = new Geodatabase(Data.DbConnectionProperties.GetProperties()))
                {
                    using (Table enterpriseTable = geodatabase.OpenDataset<Table>("DescriptionOfMapUnits"))
                    {

                        editOperation.Callback(context =>
                        {
                            QueryFilter filter = new QueryFilter { WhereClause = "objectid = " + Selected.ID };

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

                        if (!result)
                        {
                            MessageBox.Show(editOperation.ErrorMessage);
                        }
                    }
                }
            });

            await Data.DescriptionOfMapUnits.RefreshMapUnitsAsync();

            NotifyPropertyChanged("AllMapUnits");

            // Reset values
            Selected = null;
        }

        #region ### Validation ####

        // Error collection
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

        private void ValidateCanDelete()
        {

            // TODO: Prevent user from deleting any mapunits with mapunitpolys

            const string propertyKey = "SilentError";

            if (Selected == null)
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