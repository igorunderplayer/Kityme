using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace Kityme.Entities
{
    [BsonIgnoreExtraElements]
    public class User
    {
        [BsonId]
        public ulong ID { get; private set; }
        public double Money { get; private set; }
        public float GlobalRewardMultiplier { get; set; }
        public DateTime DailyTimestamp { get; private set; }

        public User (ulong id)
        {
            ID = id;
            GlobalRewardMultiplier = 1.0f;
            Money = 0;

            DailyTimestamp = DateTime.Now.AddDays(-13);
        }

        public void AddMoney(double qtd)
            => Money += qtd;

        public void RemoveMoney(double qtd)
            => Money -= qtd;

        public void UpdateDailyTimestamp(DateTime time)
            => DailyTimestamp = time;
    }
}