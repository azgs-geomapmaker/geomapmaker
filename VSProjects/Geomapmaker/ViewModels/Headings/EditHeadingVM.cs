namespace Geomapmaker.ViewModels.Headings
{
    public class EditHeadingVM
    {
        public HeadingsViewModel Parent { get; set; }

        public EditHeadingVM(HeadingsViewModel parent)
        {
            Parent = parent;
        }
    }
}
