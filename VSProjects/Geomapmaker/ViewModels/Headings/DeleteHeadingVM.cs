namespace Geomapmaker.ViewModels.Headings
{
    public class DeleteHeadingVM
    {
        public HeadingsViewModel Parent { get; set; }

        public DeleteHeadingVM(HeadingsViewModel parent)
        {
            Parent = parent;
        }
    }
}
