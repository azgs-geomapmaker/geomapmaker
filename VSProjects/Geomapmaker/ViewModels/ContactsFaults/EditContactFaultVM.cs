namespace Geomapmaker.ViewModels.ContactsFaults
{
    public class EditContactFaultVM
    {
        public ContactsFaultsViewModel ParentVM { get; set; }

        public EditContactFaultVM(ContactsFaultsViewModel parentVM)
        {
            ParentVM = parentVM;
        }

    }
}
