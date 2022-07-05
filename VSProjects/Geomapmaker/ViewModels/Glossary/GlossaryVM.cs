using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Controls;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using Geomapmaker.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Geomapmaker.ViewModels.Glossary
{
    public class GlossaryVM : ProWindow, INotifyPropertyChanged
    {
        public event EventHandler WindowCloseEvent;

        public ICommand CommandCancel => new RelayCommand(() => CloseProwindow());

        public UndefinedGlossaryVM Undefined { get; set; }
        public CreateGlossaryVM Create { get; set; }
        public EditGlossaryVM Edit { get; set; }
        public DeleteGlossaryVM Delete { get; set; }

        public GlossaryVM()
        {
            Undefined = new UndefinedGlossaryVM(this);
            Create = new CreateGlossaryVM(this);
            Edit = new EditGlossaryVM(this);
            Delete = new DeleteGlossaryVM(this);
        }

        public void CloseProwindow()
        {
            WindowCloseEvent(this, new EventArgs());
        }

        private List<GlossaryTerm> _terms { get; set; }
        public List<GlossaryTerm> Terms
        {
            get => _terms;
            set
            {
                _terms = value;
                NotifyPropertyChanged();
            }
        }

        public async void GetGlossaryTermsAndUndefined(bool SetUndefined = true)
        {
            Tuple<List<GlossaryTerm>, List<GlossaryTerm>> tuple = await Data.Glossary.GetUndefinedGlossaryTerms();

            Terms = tuple.Item1.OrderBy(a => a.Term).ToList();

            if (SetUndefined)
            {
                Undefined.SetUndefinedTerms(tuple.Item2);
            }
        }

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
                _glossary.glossaryVM.GetGlossaryTermsAndUndefined();
            }, ps.Progressor);
        }
    }
}
