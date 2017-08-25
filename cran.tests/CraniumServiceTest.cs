using cran.Data;
using cran.Model.Dto;
using cran.Model.ViewModel;
using cran.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.UserSecrets;
using Moq;
using System;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Xunit;

[assembly: UserSecretsId("CRANSECRETS201707021036")]

namespace cran.tests
{    
    

    public class CraniumServiceTest
    {

        protected IConfiguration GetConfiguration()
        {
            // Adding JSON file into IConfiguration.
            IConfiguration config = new ConfigurationBuilder()
                .AddUserSecrets<CraniumServiceTest>()
                .Build();            

            return config;
        }

        private ApplicationDbContext CreateDbContext(IConfiguration config)
        {            
            string connString = config["ConnectionString"];
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connString)
            .Options;
            ApplicationDbContext context = new ApplicationDbContext(options, GetPrincipalMock());
            return context;
        }

        private TestingObject<CraniumService> GetTestingObject()
        {
            IConfiguration config = GetConfiguration();
            ApplicationDbContext context = CreateDbContext(config);        

           

            var testingObject = new TestingObject<CraniumService>();
            testingObject.AddDependency(context);
            testingObject.AddDependency(new Mock<IDbLogService>(MockBehavior.Loose));
            testingObject.AddDependency(GetPrincipalMock());
            return testingObject;
        }

        private IPrincipal GetPrincipalMock()
        {
            var identityMock = new Mock<IIdentity>(MockBehavior.Loose);
            identityMock.Setup(x => x.Name).Returns("testuser");
            IIdentity identity = identityMock.Object;

            var pricipalMock = new Mock<IPrincipal>(MockBehavior.Loose);
            pricipalMock.Setup(x => x.Identity).Returns(identity);
            IPrincipal principal = pricipalMock.Object;
            return principal;
        }

        [Fact]
        public async Task TestStartTest()
        {
            var testignObject = GetTestingObject();
            ICraniumService service = testignObject.GetResolvedTestingObject();
            var courses = await service.CoursesAsync();
            int courseId = courses.Courses.Where(x => x.Title == "JS").Select(x => x.Id).First();
            var result = await service.StartCourseAsync(courseId);
            Assert.True(result.IdCourseInstance > 0);
            Assert.True(result.IdCourse == courseId);
            Assert.True(result.IdCourseInstanceQuestion > 0);
            Assert.True(result.NumQuestionsAlreadyAsked == 0);
            Assert.True(result.NumQuestionsTotal > 0);

            var result2 = await service.NextQuestion(result.IdCourseInstance);
            Assert.True(result2.IdCourse == courseId);
            Assert.True(result2.IdCourseInstanceQuestion > 0);
            Assert.True(result2.NumQuestionsAlreadyAsked == 1);
            Assert.True(result2.NumQuestionsTotal > 0);

            var result3 = await service.NextQuestion(result.IdCourseInstance);
            Assert.True(result3.IdCourse == courseId);
            Assert.True(result3.IdCourseInstanceQuestion > 0);
            Assert.True(result3.NumQuestionsAlreadyAsked == 2);
            Assert.True(result3.NumQuestionsTotal > 0);

            var result4 = await service.GetQuestionToAskAsync(result.IdCourseInstanceQuestion);

            QuestionAnswerDto answer = new QuestionAnswerDto();
            answer.IdCourseInstanceQuestion = result.IdCourseInstanceQuestion;
            answer.Answers.Add(true);
            answer.Answers.Add(false);

            var result5 = await service.AnswerQuestionAndGetSolutionAsync(answer);

            
            var result6 = await service.AnswerQuestionAndGetNextQuestionAsync(answer);
        }   
    }
}