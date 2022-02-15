﻿using ArcGIS.Core.Data;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Geomapmaker._helpers;
using Geomapmaker.Models;
using Geomapmaker.RibbonElements;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Geomapmaker.ViewModels.MapUnits
{
    public class DeleteMapUnitVM : PropertyChangedBase, INotifyDataErrorInfo
    {
        // Deletes's save button
        public ICommand CommandDelete { get; }

        public MapUnitsViewModel ParentVM { get; set; }

        public DeleteMapUnitVM(MapUnitsViewModel parentVM)
        {
            // Init commands
            CommandDelete = new RelayCommand(() => DeleteAsync(), () => CanDelete());

            ParentVM = parentVM;
        }

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

                HexColor = ColorConverter.RGBtoHex(Selected?.AreaFillRGB);
                NotifyPropertyChanged("HexColor");

                GeoMaterial = Selected?.GeoMaterial;
                NotifyPropertyChanged("GeoMaterial");

                GeoMaterialConfidence = Selected?.GeoMaterialConfidence;
                NotifyPropertyChanged("GeoMaterialConfidence");

                NotifyPropertyChanged("Visibility");

                ValidateCanDelete();
            }
        }

        public string Visibility => Selected == null ? "Hidden" : "Visible";

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
            return Selected != null && !HasErrors;
        }

        private async Task DeleteAsync()
        {
            MessageBoxResult messageBoxResult = MessageBox.Show($"Are you sure you want to delete {Name}?", $"Delete {Name}?", MessageBoxButton.YesNo);

            if (messageBoxResult == MessageBoxResult.No)
            {
                return;
            }

            string errorMessage = null;

            StandaloneTable dmu = MapView.Active?.Map.StandaloneTables.FirstOrDefault(a => a.Name == "DescriptionOfMapUnits");

            if (dmu == null)
            {
                MessageBox.Show("DescriptionOfMapUnits table not found in active map.");
                return;
            }

            await QueuedTask.Run(() =>
            {
                try
                {
                    Table enterpriseTable = dmu.GetTable();

                    EditOperation editOperation = new EditOperation();

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
                }
                catch (Exception ex)
                {
                    errorMessage = ex.InnerException == null ? ex.Message : ex.InnerException.ToString();

                    // Trim the stack-trace from the error msg
                    if (errorMessage.Contains("--->"))
                    {
                        errorMessage = errorMessage.Substring(0, errorMessage.IndexOf("--->"));
                    }

                    errorMessage = errorMessage + Environment.NewLine + Environment.NewLine + "Check attribute rules.";
                }
            });

            if (!string.IsNullOrEmpty(errorMessage))
            {
                MessageBox.Show(errorMessage, "One or more errors occured.");
            }
            else
            {
                // Add new symbology if needed. Remove old symbology if needed.
                Data.MapUnitPolys.RebuildMapUnitPolygonsSymbology();

                ParentVM.RefreshMapUnitsAsync();

                // Reset values
                Selected = null;
            }
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