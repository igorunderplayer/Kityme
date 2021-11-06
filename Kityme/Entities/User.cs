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
        public float RewardMultiplier { get; set; }
        public List<Cat> Cats { get; private set; } = new();
        public DateTime DailyTimestamp { get; private set; }

        // Parte dos gatos la

        public DateTime ShowTimestamp;
        public DateTime YTVideoTimestamp;


        public User (ulong id)
        {
            ID = id;
            RewardMultiplier = 1.0f;
            Money = 0;

            DailyTimestamp = DateTime.Now.AddDays(-1);
            ShowTimestamp = DateTime.Now.AddDays(-7);
            YTVideoTimestamp = DateTime.Now.AddHours(-12);
        }

        public void AddMoney(double qtd)
            => Money += qtd;

        public void RemoveMoney(double qtd)
            => Money -= qtd;

        public void UpdateDailyTimestamp(DateTime time)
            => DailyTimestamp = time;
    }
}