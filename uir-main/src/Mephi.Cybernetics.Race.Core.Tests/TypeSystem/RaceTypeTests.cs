using Mephi.Cybernetics.Race.Core.TypeSystem;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mephi.Cybernetics.Race.Core.Tests.TypeSystem
{
    [TestFixture]
    public class RaceTypeTests
    {
        [Test]
        public void TestCreateRaceType()
        {
            var raceType1 = RaceType.Define()
                .Name("name")
                .Namespace("Some.Namespace")
                .Source("some.assembly.dll")
                ;

            var raceType2 = RaceType.Define()
                .Name("name2")
                .Namespace("Some.Namespace")
                .Source("some.assembly.dll")
                .Subtyping()
                    .Parent(raceType1)
                    .Parent(RaceType.Define()
                        .Name("")
                        .Namespace("")
                        .Source(""))
                    .End()
                ;
            //Не работает?
            var raceType3 = RaceType.Define()
                .Name("name2")
                .Namespace("Some.Namespace")
                .Source("some.assembly.dll")
                    .Function()
                        .Argument("arg1", raceType1)
                        .Argument("arg2", raceType2)
                        .Result(raceType1)
                        .End()
                ;
        }
    }
}
