namespace Geomapmaker.ViewModels.ContactsFaults
{
    public class DeleteContactFault
    {
        public ContactsFaultsViewModel ParentVM { get; set; }

        public DeleteContactFault(ContactsFaultsViewModel parentVM)
        {
            ParentVM = parentVM;
        }
    }
}
