using System;
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
    public class RefreshTokensAsyncTest
    {
        private readonly IAuthService _authService = new AuthService();
        private static readonly Mapper Mapper = MapperFactory.GetMapperInstance();

        [Test]
        public void Refresh_Tokens_Async_Test()
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

            // refresh token part
            var tokenPayload = new RefreshTokensPayload(session.Result.Tokens.RefreshToken, fingerPrint);
            var token = _authService.RefreshTokensAsync(tokenPayload);
            token.Result.AccessToken.Should().NotBeNullOrEmpty();
            token.Result.RefreshToken.Should().NotBeNullOrEmpty();
        }
    }
}