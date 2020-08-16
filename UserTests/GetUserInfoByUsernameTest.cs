using System;
using AutoMapper;
using FluentAssertions;
using NUnit.Framework;
using ServicesLibrary.Interfaces;
using ServicesLibrary.MapperFiles;
using ServicesLibrary.Models.Payload;
using ServicesLibrary.Services;

namespace ServicesTest.UserTests
{
    [TestFixture]
    public class GetUserInfoByUsernameTest
    {
        private readonly IAuthService _authService = new AuthService();
        private static readonly Mapper Mapper = MapperFactory.GetMapperInstance();
        
        [Test]
        public void Get_User_Info_By_Username_Test()
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
            
            var userService = new UserService(session.Result);
            var userInfo = userService.GetUserInfo("petrokolosov");
            userInfo.Result.Username.Should().Be("petrokolosov");
            userInfo.Result.Name.Should().Be("Petro");
        }
    }
}