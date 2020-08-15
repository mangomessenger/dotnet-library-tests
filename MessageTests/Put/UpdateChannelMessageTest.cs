﻿using System;
using System.Collections.Generic;
using AutoMapper;
using FluentAssertions;
using NUnit.Framework;
using ServicesLibrary.ChatTypes;
using ServicesLibrary.Interfaces;
using ServicesLibrary.MapperFiles;
using ServicesLibrary.Models.Payload;
using ServicesLibrary.Services;

namespace ServicesTest.MessageTests.Put
{
    [TestFixture]
    public class UpdateChannelMessageTest
    {
        private readonly IAuthService _authService = new AuthService();
        private static readonly Mapper Mapper = MapperFactory.GetMapperInstance();

        [Test]
        public void Update_Channel_Message_Test()
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

            var channelServices = new ChannelService(session);
            var channelPayload = new CreateCommunityPayload
            {
                Title = "WSB the best",
                Usernames = new List<string> {"dnldcode", "arslanbek", "petrokolosov"}
            };

            var channel = channelServices.CreateChat(channelPayload);
            channel.Title.Should().Be("WSB the best");
            channel.Description.Should().BeNull();
            channel.Creator.Name.Should().Be(name);
            channel.ChatType.Should().Be(TypesOfChat.Channel);
            channel.Tag.Should().BeNull();
            channel.PhotoUrl.Should().BeNull();
            channel.MembersCount.Should().Be(4);
            channel.Members[3].Name.Should().Be(name);
            channel.Members[2].Username.Should().Be("petrokolosov");
            channel.Members[1].Username.Should().Be("dnldcode");
            channel.Members[0].Username.Should().Be("arslanbek");
            channel.Verified.Should().BeFalse();
            channel.UpdatedAt.Should().BeGreaterThan(0);

            var messageService = new MessageService(session);
            var message = messageService.SendMessage(channel, "this is test message");
            message.MessageText.Should().Be("this is test message");
            message = messageService.SendMessage(channel, "this is another test message");
            message.MessageText.Should().Be("this is another test message");

            var messageId = message.Id;
            var updateMessage = messageService.UpdateMessage(message, "this is updated message");
            updateMessage.Should().BeNullOrEmpty();
            var updatedMessage = messageService.GetMessageById(messageId);
            updatedMessage.MessageText.Should().Be("this is updated message");
        }

        [Test]
        public void Update_Channel_Message_Async_Test()
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
            
            var channelServices = new ChannelService(session.Result);
            var channelPayload = new CreateCommunityPayload
            {
                Title = "WSB the best",
                Usernames = new List<string> {"dnldcode", "arslanbek", "petrokolosov"}
            };

            var channel = channelServices.CreateChatAsync(channelPayload);
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
            
            var messageService = new MessageService(session.Result);
            var message = messageService.SendMessageAsync(channel.Result, "this is test message");
            message.Result.MessageText.Should().Be("this is test message");
            
            var updateMessage = messageService.UpdateMessageAsync(message.Result, "this is updated message");
            updateMessage.Result.Should().BeNullOrEmpty();
        }
    }
}