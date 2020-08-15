using System;
using AutoMapper;
using FluentAssertions;
using NUnit.Framework;
using ServicesLibrary.ChatTypes;
using ServicesLibrary.Interfaces;
using ServicesLibrary.MapperFiles;
using ServicesLibrary.Models.Payload;
using ServicesLibrary.Services;

namespace ServicesTest.MessageTests.Put
{
    [TestFixture]
    public class UpdateDirectMessageAsyncTest
    {
        private readonly IAuthService _authService = new AuthService();
        private static readonly Mapper Mapper = MapperFactory.GetMapperInstance();

        [Test]
        public void Update_Channel_Message_Async_Test()
        {
            // send code part
            var phone = new Random().Next(500000000, 900000000).ToString();
            var countryCode = "PL";
            var fingerPrint = Faker.Lorem.Sentence();

            var sendCodePayload = new SendCodePayload(phone, countryCode, fingerPrint);
            var authRequest = _authService.SendCodeAsync(sendCodePayload);
            authRequest.Result.Should().NotBeNull();
            authRequest.Result.PhoneNumber.Should().Be("+48" + phone);

            // register part
            var name = Faker.Name.FullName();
            var phoneCode = 22222;

            var registerPayload = Mapper.Map<RegisterPayload>(authRequest.Result);
            registerPayload.Name = name;
            registerPayload.PhoneCode = phoneCode;
            registerPayload.TermsOfServiceAccepted = true;
            var session = _authService.RegisterAsync(registerPayload);

            // check session data
            session.Result.User.Id.ToString().Length.Should().BeGreaterThan(5);
            session.Result.User.Name.Should().Be(name);
            session.Result.User.Username.Should().BeNull();
            session.Result.User.Bio.Should().BeNull();
            session.Result.User.PhotoUrl.Should().BeNull();
            session.Result.User.Verified.Should().BeFalse();
            session.Result.Tokens.AccessToken.Should().NotBeNullOrEmpty();
            session.Result.Tokens.RefreshToken.Should().NotBeNullOrEmpty();

            var chatService = new DirectChatService(session.Result);
            var createChatPayload = new CreateDirectChatPayload
            {
                Username = "petrokolosov"
            };
            
            var chat = chatService.CreateChatAsync(createChatPayload);
            chat.Result.ChatType.Should().Be(TypesOfChat.DirectChat);
            chat.Result.Members.Count.Should().Be(2);
            chat.Result.Members[0].Username.Should().Be("petrokolosov");
            chat.Result.Members[1].Name.Should().Be(name);
            chat.Result.UpdatedAt.Should().BeGreaterThan(0);
            
            var messageService = new MessageService(session.Result);
            var m1 = messageService.SendMessageAsync(chat.Result, "this is test message");
            m1.Result.MessageText.Should().Be("this is test message");

            var id = m1.Result.Id;

            var updateMessage = messageService.UpdateMessageAsync(m1.Result, "this is updated message");
            updateMessage.Result.Should().BeNullOrEmpty();
            var messageById = messageService.GetMessageByIdAsync(id);
            messageById.Result.MessageText.Should().Be("this is updated message");
        }
    }
}