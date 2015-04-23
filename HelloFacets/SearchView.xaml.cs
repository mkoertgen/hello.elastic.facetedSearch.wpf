using HelloFacets.MiniMods;

namespace HelloFacets
{
    public partial class SearchView
    {
        public SearchView()
        {
            ShortcutParser.AttachToCaliburn();

            InitializeComponent();
        }
    }
}
