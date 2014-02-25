using System.Collections.Generic;
using System.Linq;
using Should;
using Xunit;

namespace AutoMapper.UnitTests.Bug
{
    public class MappingToAProtectedCollection : AutoMapperSpecBase
    {
        private Destination _destination;

        public class Source
        {
            public int[] Things { get; set; }
            public int[] Stuff { get; set; }
        }

        public class Destination
        {
            private readonly List<int> _things = new List<int>();
            private readonly List<int> _stuff = new List<int>();

            public IEnumerable<int> Things
            {
                get { return _things.Select(x => x); }
                set{}
            }

            public IEnumerable<int> Stuff
            {
                get { return _stuff.Select(x => x); }
                set { }
            } 

            public void AddThing(int thing)
            {
                _things.Add(thing);
            }

            public void AddStuff(int stuff)
            {
                _stuff.Add(stuff);
            }
        }

        protected override void Establish_context()
        {
            Mapper.CreateMap<Source, Destination>();
        }

        protected override void Because_of()
        {
            var source = new Source
            {
                Things = new[] { 1, 2, 3, 4 },
                Stuff = new[] { 5, 6 },
            };
            _destination = Mapper.Map<Source, Destination>(source);
        }

        [Fact]
        public void Should_map_the_list_of_source_items()
        {
            _destination.Things.ShouldNotBeNull();
            _destination.Things.ShouldBeOfLength(4);
            _destination.Things.ShouldContain(1);
            _destination.Things.ShouldContain(2);
            _destination.Things.ShouldContain(3);
            _destination.Things.ShouldContain(4);

            _destination.Stuff.ShouldNotBeNull();
            _destination.Stuff.ShouldBeOfLength(2);
            _destination.Stuff.ShouldContain(5);
            _destination.Stuff.ShouldContain(6);
        }
    }
}