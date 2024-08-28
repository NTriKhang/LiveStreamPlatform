﻿using BackendNet.DAL;
using BackendNet.Models;
using BackendNet.Models.Submodel;
using BackendNet.Repositories.IRepositories;
using BackendNet.Repository;
using BackendNet.Services;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;

namespace BackendNet.Repositories
{
    public class RoomRepository : Repository<Rooms>, IRoomRepository
    {
        public RoomRepository(IMongoContext context) : base(context)
        {
            
        }
    }
}
