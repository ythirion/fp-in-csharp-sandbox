using LanguageExt;
using static LanguageExt.Prelude;

namespace FPInCsharp.RealLifeExample1
{
    public class Person
    {
        public string Email { get; set; }
    }

    class EmailService
    {
        public void SendWelcome(string email)
        {

        }
    }

    public interface ILogger
    {
        void LogSuccess(string text);
        void LogFailure(string text);
    }

    class PersonRepository
    {
        public Person GetById(long id)
        {
            return new Person { Email = "rick.sanchez@crazy.com" };
        }

        public TryOption<string> GetPersonEmail(long id)
        {
            return Try(() => GetById(id))
                    .Map(person => person.Email)
                    .ToTryOption();
        }
    }

    class PersonService
    {
        private readonly PersonRepository personRepository;
        private readonly EmailService emailService;
        private readonly ILogger logger;

        public PersonService(ILogger logger)
        {
            personRepository = new PersonRepository();
            emailService = new EmailService();
            this.logger = logger;
        }

        public void SendEmail(long personId)
        {
            personRepository.GetPersonEmail(personId)
                .Match(email =>
                {
                    emailService.SendWelcome(email);
                    logger.LogSuccess($"Email sent for {personId}");
                },
                () => logger.LogFailure($"Email not sent for {personId}"),
                exception => logger.LogFailure($"Error for {personId} {exception}")); ;
        }
    }
}
