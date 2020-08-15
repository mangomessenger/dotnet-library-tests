using System;
using FluentAssertions;
using NUnit.Framework;
using ServicesLibrary.Exceptions.Auth;
using ServicesLibrary.Interfaces;
using ServicesLibrary.Models.Payload;
using ServicesLibrary.Services;

namespace ServicesTest.AuthTests
{
    [TestFixture]
    public class SendCodeTest
    {
        private readonly IAuthService _authService = new AuthService();

        [Test]
        public void Send_Code_Test()
        {
            var phone = new Random().Next(500000000, 900000000).ToString();
            var countryCode = "PL";
            var fingerPrint = Faker.Lorem.Sentence();

            var sendCodePayload = new SendCodePayload(phone, countryCode, fingerPrint);

            var authRequest = _authService.SendCode(sendCodePayload);
            authRequest.Should().NotBeNull();
            authRequest.PhoneNumber.Should().Be("+48" + phone);
            authRequest.IsNew.Should().BeTrue();
            authRequest.CountryCode.Should().Be(countryCode);
            authRequest.PhoneCodeHash.Should().NotBe(null);
            authRequest.PhoneCodeHash.Should().NotBe(string.Empty);
        }

        [Test]
        public void Send_Code_Async_Test()
        {
            var phone = new Random().Next(500000000, 900000000).ToString();
            var countryCode = "PL";
            var fingerPrint = Faker.Lorem.Sentence();

            var sendCodePayload = new SendCodePayload(phone, countryCode, fingerPrint);
            var authRequest = _authService.SendCodeAsync(sendCodePayload);
            authRequest.Result.Should().NotBeNull();
            authRequest.Result.PhoneNumber.Should().Be("+48" + phone);
            authRequest.Result.IsNew.Should().BeTrue();
            authRequest.Result.CountryCode.Should().Be(countryCode);
            authRequest.Result.PhoneCodeHash.Should().NotBe(null);
            authRequest.Result.PhoneCodeHash.Should().NotBe(string.Empty);
        }
    }
}