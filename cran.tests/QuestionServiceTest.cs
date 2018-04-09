﻿using cran.Data;
using cran.Model.Dto;
using cran.Model.Entities;
using cran.Services;
using cran.Services.Exceptions;
using cran.tests.Infra;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace cran.tests
{
    public class QuestionServiceTest 
    {

        private IQuestionService InitQuestionService(TestingContext context)
        {
            InitContext(context);

            IQuestionService questionService = context.GetService<QuestionService>();
            return questionService;
        }

        private void InitContext(TestingContext context)
        {
            context.AddPrinicpalmock();
            context.AddInMemoryDb();
            context.AddMockLogService();
            context.DependencyMap[typeof(IBinaryService)] = context.GetService<BinaryService>();
            context.DependencyMap[typeof(ITextService)] = context.GetService<TextService>();
            context.DependencyMap[typeof(ICommentsService)] = context.GetService<CommentsService>();
        }

        [Fact]
        public async Task UpdateQuestionOk()
        {
            //Prepare
            TestingContext testingContext = new TestingContext();
            InitContext(testingContext);
            ApplicationDbContext dbContext = testingContext.GetSimple<ApplicationDbContext>();
            Question question = dbContext.Questions.First();           

            testingContext.AddPrincipalMock(question.User.UserId, Roles.User);           
            IQuestionService questionService = testingContext.GetService<QuestionService>();

            QuestionDto dto = await questionService.GetQuestionAsync(question.Id);
            dto.Title = "Another Title";

            //Act 
            await questionService.UpdateQuestionAsync(dto);

            //Assert
            QuestionDto dtoResult = await questionService.GetQuestionAsync(question.Id);
            Assert.Equal("Another Title", dtoResult.Title);
            
        }

        [Fact]
        public async Task UpdateQuestionNoAccess()
        {
            //Perpare
            TestingContext context = new TestingContext();
            IQuestionService questionService = InitQuestionService(context);

            ApplicationDbContext dbContext = context.GetSimple<ApplicationDbContext>();
            int firstQuestionId = dbContext.Questions.First().Id;

            QuestionDto dto = await questionService.GetQuestionAsync(firstQuestionId);
            dto.Title = "Another Title";

            //Act and Assert
            Exception ex = await Assert.ThrowsAsync<SecurityException>(async () =>
            {
                await questionService.UpdateQuestionAsync(dto);
            });
        }

        [Fact]
        public async Task GetQuestion()
        {
            //Perpare
            TestingContext context = new TestingContext();
            IQuestionService questionService = InitQuestionService(context);
            ApplicationDbContext dbContext = context.GetSimple<ApplicationDbContext>();
            int firstQuestionId = dbContext.Questions.First().Id;

            //Act
            QuestionDto questionDto = await questionService.GetQuestionAsync(firstQuestionId);

            //Assert
            Assert.Equal(firstQuestionId, questionDto.Id);
            Assert.Equal(4, questionDto.Tags.Count);
            Assert.True(questionDto.Tags.All(x => x.IdTagType == (int)TagType.Standard));
        }

        [Fact]
        public async Task GetQuestionThatDoesNotExist()
        {
            //Perpare
            TestingContext context = new TestingContext();
            IQuestionService questionService = InitQuestionService(context);            

            //Act
            Exception ex = await Assert.ThrowsAsync<EntityNotFoundException>(async () =>
            {
                QuestionDto dto = await questionService.GetQuestionAsync(9877445);
            });
        }

        [Fact]
        public async Task TestVersionQuestion()
        {
            //Prepare
            TestingContext context = new TestingContext();
            InitContext(context);
            ApplicationDbContext dbContext = context.GetSimple<ApplicationDbContext>();
            Question question = dbContext.Questions.First();
            context.AddPrincipalMock(question.User.UserId, Roles.User);

            IQuestionService questionService = context.GetService<QuestionService>();

            //Act
            int newId = await questionService.VersionQuestionAsync(question.Id);

            //Assert
            Assert.True(question.Id != newId);
            Assert.True(newId > 0);

        }

        [Fact]
        public async Task TestImage()
        {
            //Perpare
            TestingContext context = new TestingContext();
            IQuestionService questionService = InitQuestionService(context);


            //Add Q
            QuestionDto qdto = new QuestionDto()
            {
                Title = "Bla",
                Explanation = "bla",
                Language = "De",
            };
            int newId = await questionService.InsertQuestionAsync(qdto);

            //Add Binary
            IBinaryService binaryService = context.GetSimple<IBinaryService>();
            int id = await binaryService.AddBinaryAsync(new BinaryDto
            {
                ContentDisposition = "ContentDisposition",
                ContentType = "ContentType",
                FileName = "FileName",
                Name = "Name",
                Length = 2334,
            });


            ImageDto imageDto = new ImageDto()
            {
                IdBinary = id,
                Full = false,
                Height = 124,
                Width = 64,
            };

            imageDto = await questionService.AddImageAsync(imageDto);
            QuestionDto questionDto = await questionService.GetQuestionAsync(newId);
            questionDto.Images.Add(imageDto);

            await questionService.UpdateQuestionAsync(questionDto);

            questionDto = await questionService.GetQuestionAsync(newId);

            Assert.True(questionDto.Images.Count == 1);
        }
    }
}
