using Mephi.Cybernetics.Race.DataAccess.MetaObjects.Attributes;

namespace TemplateGeneratorAPI.SubjectDomain
{
    public class Student : Person
    {
        [Relation(RelationType.OneToOne, typeof(Group), "ID")]
        public Group Group
        {
            get { return GetValue<Group>(nameof(Group)); }
            set { SetValue(nameof(Group), value); }
        }

        [Relation(RelationType.OneToOne, typeof(FinalQualifyingWork), "ID")]
        public FinalQualifyingWork FinalQualifyingWork
        {
            get { return GetValue<FinalQualifyingWork>(nameof(FinalQualifyingWork)); }
            set { SetValue(nameof(FinalQualifyingWork), value); }
        }
    }
}
