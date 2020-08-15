using System;
using System.Collections.Generic;
using AutoMapper;
using FluentAssertions;
using NUnit.Framework;
using ServicesLibrary.ChatTypes;
using ServicesLibrary.Interfaces;
using ServicesLibrary.MapperFiles;
using ServicesLibrary.Models.Payload;
using ServicesLibrary.Services;

namespace ServicesTest.ChatTests
{
    [TestFixture]
    public class CreateGroupAsyncTest
    {
        private readonly IAuthService _authService = new AuthService();
        private static readonly Mapper Mapper = MapperFactory.GetMapperInstance();

        [Test]
        public void Create_Group_Async_Test()
        {
            var phone = new Random().Next(500000000, 900000000).ToString();
            var countryCode = "PL";
            var fingerPrint = Faker.Lorem.Sentence();

            // send code part
            var sendCodePayload = new SendCodePayload(phone, countryCode, fingerPrint);
            var authRequest = _authService.SendCodeAsync(sendCodePayload);

            // register part
            var name = Faker.Name.FullName();
            var phoneCode = 22222;

            var registerPayload = Mapper.Map<RegisterPayload>(authRequest.Result);
            registerPayload.Name = name;
            registerPayload.PhoneCode = phoneCode;
            registerPayload.TermsOfServiceAccepted = true;
            var session = _authService.RegisterAsync(registerPayload);
            
            // create group part
            var groupService = new GroupService(session.Result);
            
            var groupPayload = new CreateCommunityPayload
            {
                Title = "WSB the best",
                Usernames = new List<string> {"dnldcode", "arslanbek", "petrokolosov"}
            };

            var group = groupService.CreateChatAsync(groupPayload);
            
            // check group data
            group.Result.Title.Should().Be("WSB the best");
            group.Result.Description.Should().BeNull();
            group.Result.MembersCount.Should().Be(4);
            group.Result.ChatType.Should().Be(TypesOfChat.Group);
            group.Result.PhotoUrl.Should().BeNull();
            group.Result.Creator.Name.Should().Be(name);
            group.Result.Members[3].Name.Should().Be(name);
            group.Result.Members[2].Username.Should().Be("petrokolosov");
            group.Result.Members[1].Username.Should().Be("dnldcode");
            group.Result.Members[0].Username.Should().Be("arslanbek");
            group.Result.UpdatedAt.Should().BeGreaterThan(0);
        }
    }
}