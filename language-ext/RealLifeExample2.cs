using LanguageExt;
using System;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace RealLifeExample2
{
    class PersonService
    {
        private readonly PersonRepository personRepository;
        private readonly EmailService emailService;
        private readonly ILogger logger;
        private readonly TwitterService twitterService;

        public PersonService(ILogger logger)
        {
            this.logger = logger;
            personRepository = new PersonRepository();
            emailService = new EmailService();
            twitterService = new TwitterService();
        }

        private Try<Context> CreateContext(long personId)
        {
            return Try(() => personRepository.GetById(personId))
                    .Map(person => new Context(person));
        }

        private Try<Context> RegisterTwitter(Context context)
        {
            return Try(() => twitterService.Register(context.Email, context.Name))
                    .Map(account => context.SetAccount(account));
        }

        private Try<Context> Authenticate(Context context)
        {
            return Try(() => twitterService.Authenticate(context.Email, context.Password))
                    .Map(token => context.SetToken(token));
        }

        private Try<Context> Tweet(Context context)
        {
            return Try(() => twitterService.Tweet(context.Token, "Hello les cocos"))
                    .Map(tweet => context.SetTweet(tweet));
        }

        private Try<Context> UpdateParty(Context context)
        {
            return Try(() =>
            {
                personRepository.Update(context.Id, context.AccountId);
                return context;
            });
        }

        public string Register(long personId)
        {
            return CreateContext(personId)
                    .Bind(RegisterTwitter)
                    .Bind(Authenticate)
                    .Bind(Tweet)
                    .Bind(UpdateParty)
                    .Match(
                    context =>
                    {
                        logger.LogSuccess($"User {personId} registered");
                        return context.Url;
                    },
                    exception =>
                    {
                        logger.LogFailure($"Unable to register user : {personId} {exception.Message}");
                        return string.Empty;
                    });
        }
    }
    public interface ILogger
    {
        void LogSuccess(string text);
        void LogFailure(string text);
    }

    class Person
    {
        public long Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
    }

    class EmailService
    {
        public void SendWelcome(string email)
        {

        }
    }

    class PersonRepository
    {
        public Person GetById(long id)
        {
            return new Person { Email = "rick.sanchez@crazy.com", Name = "Rick Sanchez" };
        }

        public TryOption<string> GetPersonEmail(long id)
        {
            return Try(() => GetById(id))
                    .Map(person => person.ToString())
                    .ToTryOption();
        }

        internal void Update(long personId, string accountId)
        {

        }
    }

    class Account
    {
        public string Id { get; set; }
    }

    class Tweet
    {
        public string Url { get; set; }
    }

    class TwitterService
    {
        public Account Register(string email, string name)
        {
            return new Account { Id = "9" };
        }

        public string Authenticate(string email, string password)
        {
            return Guid.NewGuid().ToString();
        }

        public Tweet Tweet(string token, string message)
        {
            return new Tweet { Url = "anUrl" };
        }
    }

    class Context
    {
        public long Id { get; }
        public string Email { get; }
        public string Name { get; }
        public string Password { get; }
        public string AccountId { get; set; }
        public string Token { get; set; }
        public string Url { get; set; }

        public Context(Person person)
        {
            Id = person.Id;
            Email = person.Email;
            Name = person.Name;
            Password = person.Password;
        }

        internal Context SetAccount(Account account)
        {
            AccountId = account.Id;
            return this;
        }

        internal Context SetToken(string token)
        {
            Token = token;
            return this;
        }

        internal Context SetTweet(Tweet tweet)
        {
            Url = tweet.Url;
            return this;
        }
    }
}