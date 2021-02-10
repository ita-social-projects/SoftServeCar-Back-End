﻿using System.Collections.Generic;
using System.Linq;
using Car.Data.Entities;
using Car.Data.Interfaces;
using Car.Domain.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Car.Domain.Services.Implementation
{
    public class ChatService : IChatService
    {
        private readonly IUnitOfWork<User> unitOfWorkUser;
        private readonly IUnitOfWork<Chat> unitOfWorkChat;

        public ChatService(IUnitOfWork<User> unitOfWorkUser, IUnitOfWork<Chat> unitOfWorkChat)
        {
            this.unitOfWorkUser = unitOfWorkUser;
            this.unitOfWorkChat = unitOfWorkChat;
        }

        public Chat GetChatById(int chatId)
        {
            return unitOfWorkChat.GetRepository().Query(u => u.Receiver).FirstOrDefault(p => p.Id == chatId);
        }

        public Chat AddChat(Chat chat)
        {
            var newChat = new Chat
            {
                Id = chat.Id,
                Name = chat.Name,
                Receiver = new User(),
            };

            var addedChat = unitOfWorkChat.GetRepository().Add(newChat);
            unitOfWorkUser.SaveChanges();
            return addedChat;
        }

        public User AddUserToChat(int userId, int chatId)
        {
            var user = unitOfWorkUser.GetRepository().GetById(userId);
            var chat = unitOfWorkChat.GetRepository().Query(receiver => receiver.Receiver).FirstOrDefault(chat => chat.Id == chatId);
            chat.Receiver = user;
            unitOfWorkChat.SaveChanges();
            return user;
        }

        public List<Chat> GetUsersChats(int userId)
        {
            var user = unitOfWorkUser.GetRepository().Query().Include(chat => chat.Chats).ThenInclude(receiver => receiver.Receiver).FirstOrDefault(user => user.Id == userId);
            return user?.Chats.ToList();
        }
    }
}