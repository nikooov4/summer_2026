using Mephi.Cybernetics.Race.DataAccess;
using System;
using Mephi.Cybernetics.Race.Core.Attributes;

namespace TemplateGeneratorAPI.SubjectDomain
{
    public class Person : EntityBase
    {

        public Person()
        {
            ID = Guid.NewGuid();
        }

        public string Name
        {
            get { return GetValue<string>(nameof(Name)); }
            set { SetValue(nameof(Name), value); }
        }

        public string Surname
        {
            get { return GetValue<string>(nameof(Surname)); }
            set { SetValue(nameof(Surname), value); }
        }

        public string Patronymic
        {
            get { return GetValue<string>(nameof(Patronymic)); }
            set { SetValue(nameof(Patronymic), value); }
        }

        [Computable]
        public string FullName
        {
            get
            {
                return String.Join(
                    " ",
                    GetValue<string>(nameof(Surname)),
                    GetValue<string>(nameof(Name)),
                    GetValue<string>(nameof(Patronymic))
                );
            }
        }

        [Computable]
        public string ShortName
        {
            get
            {
                return String.Format(
                    "{0} {1}. {2}.",
                    GetValue<string>(nameof(Surname)),
                    GetValue<string>(nameof(Name))[0],
                    GetValue<string>(nameof(Patronymic))[0]
                );
            }
        }

        // TODO: as relation or as subclass?
        // [Relation(typeof(Student), "ID", "")]
        // public List<Student> Students
        // {
        //     get { return GetValue<List<Student>>(nameof(Students)); }
        //     set { SetValue(nameof(Students), value); }
        // }

        // [Relation(typeof(Academic), "ID", "")]
        // public List<Academic> Academics
        // {
        //     get { return GetValue<List<Academic>>(nameof(Academics)); }
        //     set { SetValue(nameof(Academics), value); }
        // }
    }
}