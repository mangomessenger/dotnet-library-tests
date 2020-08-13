using System;
using AutoMapper;
using FluentAssertions;
using NUnit.Framework;
using ServicesLibrary.Interfaces;
using ServicesLibrary.MapperFiles;
using ServicesLibrary.Models;

namespace ServicesTest.AuthTests
{
    [TestFixture]
    public class LoginTest
    {
        private readonly IAuthService _authService = new ServicesLibrary.Services.AuthService();
        private static readonly Mapper Mapper = MapperFactory.GetMapperInstance();

        [Test]
        public void Login_Valid_Test()
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
            
            // logout
            var logout = _authService.Logout(session);
            logout.Should().BeTrue();

            // login again
            authRequest = _authService.SendCode(sendCodePayload);
            var loginPayload = Mapper.Map<LoginPayload>(authRequest);
            loginPayload.PhoneCode = 22222;
            var loginSession = _authService.Login(loginPayload);
            loginSession.Should().NotBeNull();
            loginSession.User.Should().NotBeNull();
            loginSession.User.Id.ToString().Length.Should().BeGreaterThan(5);
            loginSession.User.Name.Should().Be(name);
            loginSession.User.Username.Should().BeNull();
            loginSession.User.Bio.Should().BeNull();
            loginSession.User.PhotoUrl.Should().BeNull();
            loginSession.User.Verified.Should().BeFalse();
            loginSession.Tokens.AccessToken.Should().NotBeNullOrEmpty();
            loginSession.Tokens.RefreshToken.Should().NotBeNullOrEmpty();
        }
    }
}