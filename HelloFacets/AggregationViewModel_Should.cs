using FluentAssertions;
using HelloFacets.MiniMods;
using NUnit.Framework;

namespace HelloFacets
{
    [TestFixture]
    // ReSharper disable once InconsistentNaming
    internal class AggregationViewModel_Should
    {
        [Test]
        public void Be_Creatable()
        {
            var sut = new AggregationViewModel();

            sut.Should().NotBeNull();
            sut.Should().BeAssignableTo<TreeItemViewModel<AggregationViewModel>>();
            sut.Should().BeAssignableTo<ITreeItemViewModel<AggregationViewModel>>();
        }
    }
}