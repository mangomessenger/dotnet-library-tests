using System;
using AutoMapper;
using FluentAssertions;
using NUnit.Framework;
using ServicesLibrary.ChatTypes;
using ServicesLibrary.Interfaces;
using ServicesLibrary.MapperFiles;
using ServicesLibrary.Models.Payload;
using ServicesLibrary.Services;

namespace ServicesTest.MessageTests.Delete
{
    [TestFixture]
    public class DeleteDirectMessageTest
    {
        private readonly IAuthService _authService = new AuthService();
        private static readonly Mapper Mapper = MapperFactory.GetMapperInstance();
        
        [Test]
        public void Delete_Direct_Message_Test()
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

            var chatService = new ChatService(session);
            var createChatPayload = new CreateDirectChatPayload
            {
                Username = "dnldcode"
            };
            var directChat = chatService.CreateDirectChat(createChatPayload);
            directChat.ChatType.Should().Be(TypesOfChat.DirectChat);
            directChat.Members.Count.Should().Be(2);
            directChat.Members[0].Username.Should().Be("dnldcode");
            directChat.Members[1].Name.Should().Be(name);
            directChat.UpdatedAt.Should().BeGreaterThan(0);

            var messageService = new MessageService(session);
            var message = messageService.SendMessage(directChat, "hello its test");

            message.Id.Should().BeGreaterThan(0);
            message.ChatId.Should().Be(directChat.Id);
            message.MessageText.Should().Be("hello its test");
            message.CreatedAt.Should().BeGreaterThan(0);
            message.UpdatedAt.Should().BeGreaterThan(0);
            
            var deleteMessage = messageService.DeleteMessage(message);
            deleteMessage.Should().BeNullOrEmpty();
        }
    }
}