using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Mephi.Cybernetics.Race.DataAccessTests
{
    [TestFixture]
    public class ProcessDescriptionTests
    {
        [Test]
        public void TestDelegateType()
        {
            var type = typeof(Func<int, string>);
            var delegateType = typeof(Delegate);
            Assert.IsTrue(type.IsSubclassOf(delegateType));
        }
    }
}
