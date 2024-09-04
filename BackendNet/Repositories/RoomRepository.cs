using BackendNet.DAL;
using BackendNet.Models;
using BackendNet.Models.Submodel;
using BackendNet.Repositories.IRepositories;
using BackendNet.Repository;
using BackendNet.Services;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;
using System.Diagnostics;

namespace BackendNet.Repositories
{
    public class RoomRepository : Repository<Rooms>, IRoomRepository
    {
        private IUserRepository _userRepository;
        public RoomRepository(IMongoContext context
            , IUserRepository userRepository
            ) : base(context)
        {
            _userRepository = userRepository;
        }
        public async Task<Rooms> AddRoom(Rooms room)
        {
            using(var session = await _client.StartSessionAsync())
            {
                session.StartTransaction();
                try
                {
                    await _collection.InsertOneAsync(room);

                    var activity = new CurrentActivity();
                    activity.code = "Room";
                    activity.key = nameof(Rooms._id);
                    activity.value = room._id;
                    activity.role = "Owner";
                    activity.desc = $"Đang tham gia phòng {activity.value}";
                    var filterStudentDef = Builders<Users>.Filter.Eq(x => x.Id, room.Owner.user_id);
                    var updateStudentDef = Builders<Users>.Update.Set(x => x.CurrentActivity, activity);
                    var res2 = await _userRepository.UpdateByFilter(filterStudentDef, updateStudentDef);

                    await session.CommitTransactionAsync();

                    if (res2.ModifiedCount == 0)
                        return null;
                    return room;
                }
                catch (Exception)
                {
                    await session.AbortTransactionAsync();
                    throw;
                }
            }
        }
        public async Task<bool> AddStudentToRoom(string roomId, SubUser student)
        {
            using(var session = await _client.StartSessionAsync())
            {
                session.StartTransaction();
                try
                {
                    var filterRoomDef = Builders<Rooms>.Filter.Eq(x => x._id, roomId);
                    var updateRoomDef = Builders<Rooms>.Update.Push(x => x.Attendees, student);
                    var res1 = UpdateByFilter(filterRoomDef, updateRoomDef);

                    var activity = new CurrentActivity();
                    activity.code = "Room";
                    activity.key = nameof(Rooms._id);
                    activity.value = roomId;
                    activity.role = "Attendee";
                    activity.desc = $"Đang tham gia phòng {activity.value}";
                    var filterStudentDef = Builders<Users>.Filter.Eq(x => x.Id, student.user_id);
                    var updateStudentDef = Builders<Users>.Update.Set(x => x.CurrentActivity, activity);
                    var res2 = _userRepository.UpdateByFilter(filterStudentDef, updateStudentDef);

                    var res = await Task.WhenAll(res1, res2);

                    await session.CommitTransactionAsync();
                    return res[0].ModifiedCount > 0 && res[1].ModifiedCount > 0;
                }
                catch (Exception)
                {
                    await session.AbortTransactionAsync();
                    throw;
                }
            }

        }

        public async Task<bool> RemoveStudentFromRoom(string roomId, string studentId)
        {
            using (var session = await _client.StartSessionAsync())
            {
                session.StartTransaction();
                try
                {
                    FilterDefinition<Rooms> filterRoomDef = Builders<Rooms>.Filter.Eq(x => x._id, roomId);
                    var updateRoomDef = Builders<Rooms>.Update.PullFilter(
                        x => x.Attendees,
                        Builders<SubUser>.Filter.Eq(x => x.user_id, studentId)
                        );
                    var res1 = UpdateByFilter(filterRoomDef, updateRoomDef);

                    var filterStudentDef = Builders<Users>.Filter.Eq(x => x.Id, studentId);
                    var updateStudentDef = Builders<Users>.Update.Unset(x => x.CurrentActivity);
                    var res2 = _userRepository.UpdateByFilter(filterStudentDef, updateStudentDef);

                    var res = await Task.WhenAll(res1, res2);

                    await session.CommitTransactionAsync();
                    return res[0].ModifiedCount > 0 && res[1].ModifiedCount > 0;
                }
                catch (Exception)
                {
                    await session.AbortTransactionAsync();
                    throw;
                }
            }
        }
    }
}
