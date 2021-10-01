﻿using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using Geomapmaker.Models;
using Geomapmaker.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Geomapmaker
{
    internal class DescriptionOfMapUnitsViewModel : DockPane
    {
        private const string _dockPaneID = "Geomapmaker_DescriptionOfMapUnits";

        // View models
        public DMUCreateVM Create { get; set; } = new DMUCreateVM();

        public List<Interval> Intervals => Data.Intervals.IntervalCollection;

        protected DescriptionOfMapUnitsViewModel() { }

        // Tooltips dictionary
        public Dictionary<string, string> Tooltips => new Dictionary<string, string>
        {
            {"NameDescription", "TODO" },
            {"NameNotes", "TODO" },

            {"FullNameDescription", "TODO" },
            {"FullNameNotes", "TODO" },

            {"AgeDescription", "TODO" },
            {"AgeNotes", "TODO" },

            {"OlderIntervalDescription", "TODO" },
            {"OlderIntervalNotes", "TODO" },

            {"YoungerIntervalDescription", "TODO" },
            {"YoungerIntervalNotes", "TODO" },

            {"RelativeAgeDescription", "TODO" },
            {"RelativeAgeNotes", "TODO" },

            {"DescriptionDescription", "TODO" },
            {"DescriptionNotes", "TODO" },

            {"ParentDescription", "TODO" },
            {"ParentNotes", "TODO" },












            {"Description", "TODO" },
            {"Notes", "TODO" },


        };

        /// <summary>
        /// Show the DockPane.
        /// </summary>
        internal static void Show()
        {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            pane?.Activate();
        }
    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class DescriptionOfMapUnits_ShowButton : Button
    {
        protected override void OnClick()
        {
            DescriptionOfMapUnitsViewModel.Show();
        }
    }
}