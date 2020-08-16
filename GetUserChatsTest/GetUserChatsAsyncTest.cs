using System;
using System.Linq;
using AutoMapper;
using FluentAssertions;
using NUnit.Framework;
using ServicesLibrary.Interfaces;
using ServicesLibrary.MapperFiles;
using ServicesLibrary.Models.Payload;
using ServicesLibrary.Services;
using static ServicesLibrary.ChatTypes.TypesOfChat;

namespace ServicesTest.GetUserChatsTest
{
    [TestFixture]
    public class GetUserChatsAsyncTest
    {
        private readonly IAuthService _authService = new AuthService();
        private static readonly Mapper Mapper = MapperFactory.GetMapperInstance();

        [Test]
        public void Get_User_Chats_Async_Test()
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

            // create chat part
            var chatService = new DirectChatService(session.Result);

            var createChatPayload = new CreateDirectChatPayload
            {
                Username = "petrokolosov"
            };

            var chat = chatService.CreateChatAsync(createChatPayload);

            // check chat data
            chat.Result.ChatType.Should().Be(DirectChat);
            chat.Result.Members.Count.Should().Be(2);
            chat.Result.Members[0].Username.Should().Be("petrokolosov");
            chat.Result.Members[1].Name.Should().Be(name);
            chat.Result.UpdatedAt.Should().BeGreaterThan(0);

            var userChatServices = new UserChatsService(session.Result);
            var userChats = userChatServices.GetUserChatsAsync();
            userChats.Result.Should().NotBeNull();
            userChats.Result.DirectChats.Should().NotBeNull();
            userChats.Result.DirectChats.Count.Should().Be(1);
            userChats.Result.DirectChats.First().Members[0].Username.Should().Be("petrokolosov");
        }
    }
}