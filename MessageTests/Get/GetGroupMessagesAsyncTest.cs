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
    public class GetGroupMessagesAsyncTest
    {
        private readonly IAuthService _authService = new AuthService();
        private static readonly Mapper Mapper = MapperFactory.GetMapperInstance();

        [Test]
        public void Get_Group_Messages_Async_Test()
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

            // create group part
            var groupService = new GroupService(session.Result);

            var groupPayload = new CreateCommunityPayload
            {
                Title = "WSB the best",
                Usernames = new List<string> {"dnldcode", "arslanbek", "petrokolosov"}
            };

            var group = groupService.CreateChatAsync(groupPayload);

            // send group messages part
            var messageService = new MessageService(session.Result);
            var m1 = messageService.SendMessageAsync(group.Result, "this is test message");
            m1.Result.MessageText.Should().Be("this is test message");

            var m2 = messageService.SendMessageAsync(group.Result, "this is another test message");
            m2.Result.MessageText.Should().Be("this is another test message");

            // get group messages part
            var groupMessages = messageService.GetMessagesAsync(group.Result);
            groupMessages.Result.Count.Should().Be(2);
            groupMessages.Result[0].MessageText.Should().Be("this is test message");
            groupMessages.Result[1].MessageText.Should().Be("this is another test message");
        }
    }
}