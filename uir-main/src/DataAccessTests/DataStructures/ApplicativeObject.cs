using Mephi.Cybernetics.Race.DataAccess;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects.Attributes;

namespace Mephi.Cybernetics.Race.DataAccessTests.DataStructures
{
    [AgregatedClass(typeof(ObjectAbstraction), typeof(ObjectApplication))]
    public abstract class ApplicativeObject : EntityBase
    {
        public abstract string Name { get; set; }
    }

    public class ObjectApplication : ApplicativeObject
    {
        public override string Name
        {
            get { return GetValue<string>("Name"); }
            set { SetValue("Name", value); }
        }
    }

    public class ObjectAbstraction : ApplicativeObject
    {
        public override string Name
        {
            get { return GetValue<string>("Name"); }
            set { SetValue("Name", value); }
        }
    }
}
