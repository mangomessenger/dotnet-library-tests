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

namespace ServicesTest.MessageTests
{
    [TestFixture]
    public class GroupMessageTest
    {
        private readonly IAuthService _authService = new AuthService();
        private static readonly Mapper Mapper = MapperFactory.GetMapperInstance();
        
        
        [Test]
        public void SendGroupMessageTest()
        {
            // send code part
            var phone = new Random().Next(500000000, 900000000).ToString();
            var countryCode = "PL";
            var fingerPrint = Faker.Lorem.Sentence();

            var sendCodePayload = new SendCodePayload(phone, countryCode, fingerPrint);
            var authRequest = _authService.SendCode(sendCodePayload);
            authRequest.Should().NotBeNull();
            authRequest.PhoneNumber.Should().Be("+48" + phone);

            // register part
            var name = Faker.Name.FullName();
            var phoneCode = 22222;

            var registerPayload = Mapper.Map<RegisterPayload>(authRequest);
            registerPayload.Name = name;
            registerPayload.PhoneCode = phoneCode;
            registerPayload.TermsOfServiceAccepted = true;
            var session = _authService.Register(registerPayload);

            // check session data
            session.User.Id.ToString().Length.Should().BeGreaterThan(5);
            session.User.Name.Should().Be(name);
            session.User.Username.Should().BeNull();
            session.User.Bio.Should().BeNull();
            session.User.PhotoUrl.Should().BeNull();
            session.User.Verified.Should().BeFalse();
            session.Tokens.AccessToken.Should().NotBeNullOrEmpty();
            session.Tokens.RefreshToken.Should().NotBeNullOrEmpty();
            
            var groupService = new GroupService(session);
            var groupPayload = new CreateCommunityPayload
            {
                Title = "WSB the best",
                Usernames = new List<string> {"dnldcode", "arslanbek", "petrokolosov"}
            };

            var group = groupService.CreateGroup(groupPayload);
            group.Title.Should().Be("WSB the best");
            group.Description.Should().BeNull();
            group.MembersCount.Should().Be(4);
            group.ChatType.Should().Be(TypesOfChat.Group);
            group.PhotoUrl.Should().BeNull();
            group.Creator.Name.Should().Be(name);
            group.Members[3].Name.Should().Be(name);
            group.Members[2].Username.Should().Be("petrokolosov");
            group.Members[1].Username.Should().Be("dnldcode");
            group.Members[0].Username.Should().Be("arslanbek");
            group.UpdatedAt.Should().BeGreaterThan(0);
            
            var messageService = new MessageService(session);
            var groupMessage = messageService.SendMessage(group, "this is test message");
            groupMessage.MessageText.Should().Be("this is test message");
            groupMessage = messageService.SendMessage(group, "this is another test message");
            groupMessage.MessageText.Should().Be("this is another test message");
        }
    }
}