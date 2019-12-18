using System;
using Xunit;
using LanguageExt;
using static LanguageExt.Prelude;
using FluentAssertions;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using FakeItEasy;
using System.Threading.Tasks;

namespace FPInCsharp
{
    public class Examples
    {
        #region intro
        //Pure function
        static int Double(int i) => i * 2;

        [Fact]
        public void immutability()
        {
            var spongebob = new Person(name: "spongebob", age: 22);
            var spongebob2 = spongebob.With(age: 43);

            spongebob2.GetHashCode()
                .Should()
                .NotBe(spongebob.GetHashCode());
        }

        class PersonOO
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }

        internal class Person
        {
            public string Name { get; }
            public int Age { get; }

            // Smart constructor enforcing a name and age are given
            public Person(string name, int age)
            {
                Name = name;
                Age = age;
            }

            // Updates here, enforcing a new object is returned every time
            // strings are nullable because they are reference types
            public Person With(string name = null, int? age = null)
            {
                // if null, return the current value
                // else set the newly passed in value
                // ?? null coalescing operator
                return new Person(name: name ?? Name, age: age ?? Age);
            }
        }

        #endregion

        #region RecordTypes

        public class User
        {
            public readonly Guid Id;
            public readonly string Name;
            public readonly int Age;

            public User(
                Guid id,
                string name,
                int age)
            {
                Id = id;
                Name = name;
                Age = age;
            }
        }

        public class UserRecord : Record<UserRecord>
        {
            public readonly Guid Id;
            public readonly string Name;
            public readonly int Age;

            public UserRecord(
                Guid id,
                string name,
                int age)
            {
                Id = id;
                Name = name;
                Age = age;
            }
        }

        [Fact]
        public void record_types()
        {
            var spongeGuid = Guid.NewGuid();

            var spongeBob = new User(spongeGuid, "Spongebob", 40);
            var spongeBob2 = new User(spongeGuid, "Spongebob", 40);

            Assert.False(spongeBob.Equals(spongeBob2));

            var spongeBobRecord = new UserRecord(spongeGuid, "Spongebob", 40);
            var spongeBobRecord2 = new UserRecord(spongeGuid, "Spongebob", 40);

            Assert.True(spongeBobRecord.Equals(spongeBobRecord2));
        }

        #endregion

        #region Functors

        [Fact]
        public void option_functor()
        {
            Option<int> aValue = 2;
            aValue.Map(x => x + 3); // Some(5)

            Option<int> none = None;
            none.Map(x => x + 3); // None

            //Left -> Some, Right -> None
            aValue.Match(x => x + 3, () => 0); // 5
            none.Match(x => x + 3, () => 0); // 0

            // Returns the Some case 'as is' -> 2 and 1 in the None case
            int value = aValue.IfNone(1);
            int noneValue = none.IfNone(42); // 42
        }

        [Fact]
        public void lists_functor()
        {
            new int[] { 2, 4, 6 }.Map(x => x + 3); // 5,7,9
            new List<int> { 2, 4, 6 }.Map(x => x + 3); // 5,7,9
            //Prefer use List (Immutable list)
            List(2, 4, 6).Map(x => x + 3); // 5,7,9
        }

        static Func<int, int> Add2 = x => x + 2;
        static Func<int, int> Add3 = x => x + 3;
        static int Add5(int x) => Add2.Compose(Add3)(x);

        [Fact]
        public void functions_functor()
        {
            List(2, 4, 6).Map(Add5);// 7,9,11

            List(2, 4, 6)
                .Map(x => Add5(x)) // 7,9,11
                .Map(x => Add3(x)) // 10,12,14
                .Map(x => Add2(x)) // 12,14,16
                .Map(x => Add5(x)); // 17,19,21
        }

        #endregion

        #region Monads

        static Option<double> Half(double x)
            => x % 2 == 0 ? x / 2 : Option<double>.None;

        [Fact]
        public void bind_monad()
        {
            Option<double>.Some(3).Bind(x => Half(x));// None
            Option<double>.Some(4).Bind(x => Half(x));// Some(2)
        }

        [Fact]
        public void chain_bind_monad()
        {
            Option<double>.Some(20)
                .Bind(x => Half(x))// Some(10)
                .Bind(x => Half(x))// Some(5)
                .Bind(x => Half(x));// None
        }

        [Fact(Skip = "Require user input")]
        public void file_monad_example()
        {
            GetLine()
                .Bind(ReadFile)
                .Bind(PrintStrln)
                .Match(success => Console.WriteLine("SUCCESS"),
                        failure => Console.WriteLine("FAILURE"));
        }

        static Try<string> GetLine()
        {
            Console.Write("File:");
            return Try(() => Console.ReadLine());
        }

        static Try<string> ReadFile(string filePath) =>
            Try(() => File.ReadAllText(filePath));

        static Try<bool> PrintStrln(string line)
        {
            Console.WriteLine(line);
            return Try(true);
        }

        #endregion

        #region Memoization

        static Func<string, string> GenerateGuidForUser = user => user + ":" + Guid.NewGuid();
        static Func<string, string> GenerateGuidForUserMemoized = memo(GenerateGuidForUser);

        [Fact]
        public void memoization_example()
        {
            GenerateGuidForUserMemoized("spongebob");// spongebob:e431b439-3397-4016-8d2e-e4629e51bf62
            GenerateGuidForUserMemoized("buzz");// buzz:50c4ee49-7d74-472c-acc8-fd0f593fccfe
            GenerateGuidForUserMemoized("spongebob");// spongebob:e431b439-3397-4016-8d2e-e4629e51bf62
        }

        #endregion

        #region Partial application

        static Func<int, int, int> Multiply = (a, b) => a * b;
        static Func<int, int> TwoTimes = par(Multiply, 2);

        [Fact]
        public void partial_app_example()
        {
            Multiply(3, 4); // 12
            TwoTimes(9); // 18
        }

        #endregion

        #region Either

        public static Either<Exception, string> GetHtml(string url)
        {
            var httpClient = new HttpClient(new HttpClientHandler());
            try
            {
                var httpResponseMessage = httpClient.GetAsync(url).Result;
                return httpResponseMessage.Content.ReadAsStringAsync().Result;
            }
            catch (Exception ex) { return ex; }
        }

        [Fact]
        public void either_example()
        {

            GetHtml("unknown url"); // Left InvalidOperationException
            GetHtml("https://www.google.com"); // Right <!doctype html...

            var result = GetHtml("https://www.google.com");

            result.Match(
                    Left: ex => Console.WriteLine("an exception occured" + ex),
                    Right: r => Console.WriteLine(r)
                );
        }

        #endregion

        #region Fold vs Reduce

        [Fact]
        public void fold_vs_reduce()
        {
            //fold takes an explicit initial value for the accumulator
            //Can choose the result type
            var foldResult = List(1, 2, 3, 4, 5)
                .Map(x => x * 10)
                .Fold(0m, (x, s) => s + x); // 150m

            //reduce uses the first element of the input list as the initial accumulator value
            //Result type will be the one of the list
            var reduceResult = List(1, 2, 3, 4, 5)
                .Map(x => x * 10)
                .Reduce((x, s) => s + x); // 150
        }

        #endregion

        #region Real life example 1

        [Fact]
        public void personService_example()
        {
            var logger = A.Fake<RealLifeExample1.ILogger>();
            var service = new RealLifeExample1.PersonService(logger);
            var personId = 10;
            
            service.SendEmail(personId);
            A.CallTo(() => logger.LogSuccess(A<string>.Ignored)).MustHaveHappenedOnceExactly();
        }
        #endregion

        #region Real life example 2

        [Fact]
        public void register_user_example()
        {
            var logger = A.Fake<RealLifeExample2.ILogger>();
            var service = new RealLifeExample2.PersonService(logger);
            var personId = 10;

            var url = service.Register(personId);
            url.Should().Be("anUrl");
            A.CallTo(() => logger.LogSuccess(A<string>.Ignored)).MustHaveHappenedOnceExactly();
        }

        #endregion

        #region Real life example 2 - Async version

        [Fact]
        public async Task register_user_async_example()
        {
            var logger = A.Fake<RealLifeExample2_async.ILogger>();
            var service = new RealLifeExample2_async.PersonService(logger);
            var personId = 10;

            var url = await service.Register(personId);
            url.Should().Be("anUrl");
            A.CallTo(() => logger.LogSuccess(A<string>.Ignored)).MustHaveHappenedOnceExactly();
        }

        #endregion
    }
}
