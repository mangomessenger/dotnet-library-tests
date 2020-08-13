using System;
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
    public class DirectChatCreateTest
    {
        private readonly IAuthService _authService = new AuthService();
        private static readonly Mapper Mapper = MapperFactory.GetMapperInstance();

        [Test]
        public void CreateDirectChatTest()
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
            var chat = chatService.CreateDirectChat(createChatPayload);
            chat.ChatType.Should().Be(TypesOfChat.DirectChat);
            chat.Members.Count.Should().Be(2);
            chat.Members[0].Username.Should().Be("dnldcode");
            chat.Members[1].Name.Should().Be(name);
            chat.UpdatedAt.Should().BeGreaterThan(0);
        }
    }
}