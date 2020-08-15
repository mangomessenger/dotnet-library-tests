using System;
using AutoMapper;
using FluentAssertions;
using NUnit.Framework;
using ServicesLibrary.Interfaces;
using ServicesLibrary.MapperFiles;
using ServicesLibrary.Models.Payload;
using ServicesLibrary.Services;

namespace ServicesTest.MessageTests.Delete
{
    [TestFixture]
    public class DeleteDirectMessageAsyncTest
    {
        private readonly IAuthService _authService = new AuthService();
        private static readonly Mapper Mapper = MapperFactory.GetMapperInstance();

        [Test]
        public void Delete_Direct_Message_Async_Test()
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

            // send message part
            var messageService = new MessageService(session.Result);
            var m1 = messageService.SendMessageAsync(chat.Result, "this is test message");

            // delete message part
            var deleteMessage = messageService.DeleteMessageAsync(m1.Result);
            deleteMessage.Result.Should().BeNullOrEmpty();
        }
    }
}