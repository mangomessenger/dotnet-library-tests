using System;
using AutoMapper;
using FluentAssertions;
using NUnit.Framework;
using ServicesLibrary.ChatTypes;
using ServicesLibrary.Interfaces;
using ServicesLibrary.MapperFiles;
using ServicesLibrary.Models.Payload;
using ServicesLibrary.Services;

namespace ServicesTest.MessageTests.Get
{
    [TestFixture]
    public class GetDirectChatMessagesAsync
    {
        private readonly IAuthService _authService = new AuthService();
        private static readonly Mapper Mapper = MapperFactory.GetMapperInstance();

        [Test]
        public void Get_Direct_Chat_Messages_Async()
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
            
            // create direct chat part
            var chatService = new DirectChatService(session.Result);
            var createChatPayload = new CreateDirectChatPayload
            {
                Username = "petrokolosov"
            };
            
            var directChat = chatService.CreateChatAsync(createChatPayload);
            directChat.Result.ChatType.Should().Be(TypesOfChat.DirectChat);
            directChat.Result.Members.Count.Should().Be(2);
            directChat.Result.Members[0].Username.Should().Be("petrokolosov");
            directChat.Result.Members[1].Name.Should().Be(name);
            directChat.Result.UpdatedAt.Should().BeGreaterThan(0);

            // send messages to direct chat part
            var messageService = new MessageService(session.Result);
            var m1 = messageService.SendMessageAsync(directChat.Result, "this is test message");
            m1.Result.MessageText.Should().Be("this is test message");

            var m2 = messageService.SendMessageAsync(directChat.Result, "this is another test message");
            m2.Result.MessageText.Should().Be("this is another test message");
            
            // get messages from direct chat
            var directChatMessages = messageService.GetMessagesAsync(directChat.Result);
            directChatMessages.Result.Count.Should().Be(2);
            directChatMessages.Result[0].MessageText.Should().Be("this is test message");
            directChatMessages.Result[1].MessageText.Should().Be("this is another test message");
        }
    }
}