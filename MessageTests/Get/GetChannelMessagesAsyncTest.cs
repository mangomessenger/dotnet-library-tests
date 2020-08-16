﻿using System;
using System.Collections.Generic;
using AutoMapper;
using FluentAssertions;
using NUnit.Framework;
using ServicesLibrary.Interfaces;
using ServicesLibrary.MapperFiles;
using ServicesLibrary.Models.Payload;
using ServicesLibrary.Services;

namespace ServicesTest.MessageTests.Get
{
    [TestFixture]
    public class GetChannelMessagesAsyncTest
    {
        private readonly IAuthService _authService = new AuthService();
        private static readonly Mapper Mapper = MapperFactory.GetMapperInstance();
        
        [Test]
        public void Get_Channel_Messages_Async_Test()
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

            // create channel part
            var channelServices = new ChannelService(session.Result);
            var channelPayload = new CreateCommunityPayload
            {
                Title = "WSB the best",
                Usernames = new List<string> {"dnldcode", "arslanbek", "petrokolosov"}
            };

            var channel = channelServices.CreateChannelAsync(channelPayload);

            // send message part
            var messageService = new MessageService(session.Result);
            var m1 = messageService.SendMessageAsync(channel.Result, "this is test message");
            m1.Result.MessageText.Should().Be("this is test message");
            
            var m2 = messageService.SendMessageAsync(channel.Result, "this is another test message");
            m2.Result.MessageText.Should().Be("this is another test message");
            
            var channelMessages = messageService.GetMessagesAsync(channel.Result);
            channelMessages.Result.Count.Should().Be(2);
            channelMessages.Result[0].MessageText.Should().Be("this is test message");
            channelMessages.Result[1].MessageText.Should().Be("this is another test message");
        }
    }
}