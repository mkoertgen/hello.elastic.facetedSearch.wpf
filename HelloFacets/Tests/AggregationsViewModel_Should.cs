using NUnit.Framework;

namespace HelloFacets.Tests
{
    // ReSharper disable InconsistentNaming
    [TestFixture]
    class AggregationsViewModel_Should
    {
        [Test]
        public void Raise_CheckedChanged_after_checking_r_unchecking_an_item()
        {
            var item = new AggregationViewModel {Name = "2"};
            var sut = new AggregationsViewModel
            {
                Items = new[]
                {
                    new AggregationViewModel
                    {
                        Name = "1",
                        Items = new[] { item }
                    }
                }
            };

            var checkedChanged = false;
            sut.CheckedChanged += (o, e) => checkedChanged = true;

            item.IsChecked = !item.IsChecked;

            Assert.IsTrue(checkedChanged);
        }
    }
}