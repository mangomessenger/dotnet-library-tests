using System;
using System.Collections.Generic;
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
    public class CreateChannelAsyncTest
    {
        private readonly IAuthService _authService = new AuthService();
        private static readonly Mapper Mapper = MapperFactory.GetMapperInstance();

        [Test]
        public void Create_Channel_Async_Test()
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

            var channelServices = new ChannelService(session.Result);
            
            var channelPayload = new CreateCommunityPayload
            {
                Title = "WSB the best",
                Usernames = new List<string> {"dnldcode", "arslanbek", "petrokolosov"}
            };

            var channel = channelServices.CreateChannelAsync(channelPayload);
            
            // check channel data
            channel.Result.Title.Should().Be("WSB the best");
            channel.Result.Description.Should().BeNull();
            channel.Result.Creator.Name.Should().Be(name);
            channel.Result.ChatType.Should().Be(TypesOfChat.Channel);
            channel.Result.Tag.Should().BeNull();
            channel.Result.PhotoUrl.Should().BeNull();
            channel.Result.MembersCount.Should().Be(4);
            channel.Result.Members[3].Name.Should().Be(name);
            channel.Result.Members[2].Username.Should().Be("petrokolosov");
            channel.Result.Members[1].Username.Should().Be("dnldcode");
            channel.Result.Members[0].Username.Should().Be("arslanbek");
            channel.Result.Verified.Should().BeFalse();
            channel.Result.UpdatedAt.Should().BeGreaterThan(0);
        }
    }
}