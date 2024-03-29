﻿using System;
using AutoMapper;
using FluentAssertions;
using NUnit.Framework;
using ServicesLibrary.Interfaces;
using ServicesLibrary.MapperFiles;
using ServicesLibrary.Models.Payload;
using ServicesLibrary.Services;

namespace ServicesTest.AuthTests
{
    [TestFixture]
    public class LoginAsyncTest
    {
        private readonly IAuthService _authService = new AuthService();
        private static readonly Mapper Mapper = MapperFactory.GetMapperInstance();

        [Test]
        public void Login_Async_Test()
        {
            // send code part
            var phone = new Random().Next(500000000, 900000000).ToString();
            var countryCode = "PL";
            var fingerPrint = Faker.Lorem.Sentence();

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

            // logout
            _authService.LogoutAsync(session.Result);

            // login again
            authRequest = _authService.SendCodeAsync(sendCodePayload);
            var loginPayload = Mapper.Map<LoginPayload>(authRequest.Result);
            loginPayload.PhoneCode = 22222;
            var loginSession = _authService.LoginAsync(loginPayload);
            loginSession.Result.Should().NotBeNull();
            loginSession.Result.User.Should().NotBeNull();
            loginSession.Result.User.Id.ToString().Length.Should().BeGreaterThan(5);
            loginSession.Result.User.Name.Should().Be(name);
            loginSession.Result.User.Username.Should().BeNull();
            loginSession.Result.User.Bio.Should().BeNull();
            loginSession.Result.User.PhotoUrl.Should().BeNull();
            loginSession.Result.User.Verified.Should().BeFalse();
            loginSession.Result.Tokens.AccessToken.Should().NotBeNullOrEmpty();
            loginSession.Result.Tokens.RefreshToken.Should().NotBeNullOrEmpty();
        }
    }
}