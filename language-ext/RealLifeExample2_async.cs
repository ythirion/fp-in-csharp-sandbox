using LanguageExt;
using System;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace RealLifeExample2_async
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

        private TryAsync<Context> CreateContext(long personId)
        {
            return TryAsync(() => personRepository.GetById(personId))
                    .Map(person => new Context(person));
        }

        private TryAsync<Context> RegisterTwitter(Context context)
        {
            return TryAsync(() => twitterService.Register(context.Email, context.Name))
                    .Map(account => context.SetAccount(account));
        }

        private TryAsync<Context> Authenticate(Context context)
        {
            return TryAsync(() => twitterService.Authenticate(context.Email, context.Password))
                    .Map(token => context.SetToken(token));
        }

        private TryAsync<Context> Tweet(Context context)
        {
            return TryAsync(() => twitterService.Tweet(context.Token, "Hello les cocos"))
                    .Map(tweet => context.SetTweet(tweet));
        }

        private TryAsync<Context> UpdateParty(Context context)
        {
            return TryAsync(async () =>
            {
                await personRepository.Update(context.Id, context.AccountId);
                return context;
            });
        }

        public async Task<string> Register(long personId)
        {
            string result = string.Empty;
            await CreateContext(personId)
                    .Bind(RegisterTwitter)
                    .Bind(Authenticate)
                    .Bind(Tweet)
                    .Bind(UpdateParty)
                    .Match(
                    context =>
                    {
                        logger.LogSuccess($"User {personId} registered");
                        result = context.Url;
                    },
                    exception =>
                    {
                        logger.LogFailure($"Unable to register user : {personId} {exception.Message}");
                    });

            return result;
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
        public async Task<Person> GetById(long id)
        {
            return await Task.FromResult(new Person { Email = "rick.sanchez@crazy.com", Name = "Rick Sanchez" });
        }

        public TryOption<string> GetPersonEmail(long id)
        {
            return Try(() => GetById(id))
                    .Map(person => person.ToString())
                    .ToTryOption();
        }

        internal async Task Update(long personId, string accountId)
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
        public async Task<Account> Register(string email, string name)
        {
            return await Task.FromResult(new Account { Id = "9" });
        }

        public async Task<string> Authenticate(string email, string password)
        {
            return await Task.FromResult(Guid.NewGuid().ToString());
        }

        public async Task<Tweet> Tweet(string token, string message)
        {
            return await Task.FromResult(new Tweet { Url = "anUrl" });
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