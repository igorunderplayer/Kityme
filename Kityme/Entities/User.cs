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
        public Cats Cats { get; set; }
        public DateTime DailyTimestamp { get; private set; }

        public User (ulong id)
        {
            ID = id;
            GlobalRewardMultiplier = 1.0f;
            Money = 0;

            this.Cats = new Cats();

            DailyTimestamp = DateTime.Now.AddDays(-13);
        }

        public void AddMoney(double qtd)
            => Money += qtd;

        public void RemoveMoney(double qtd)
            => Money -= qtd;

        public void UpdateDailyTimestamp(DateTime time)
            => DailyTimestamp = time;

        public User Reset(bool resetAll = false)
        {
            Money = 0;

            this.Cats.Reset(resetAll);

            DailyTimestamp = DateTime.Now.AddDays(-13);

            return this;
        }
    }

    public class Cats
    {
        public ulong Quantity { get; private set; }
        public ulong TotalBought { get; private set; }

        public Cats ()
        {
            this.Quantity = 1;
            this.TotalBought = 1;
        }

        public Cats AddCats(ulong qtd)
        {
            this.Quantity += qtd;
            this.TotalBought += qtd;
            return this;
        }

        public Cats RemoveCats (ulong qtd)
        {
            this.Quantity -= qtd;
            return this;
        }

        public Cats Reset(bool resetAll = false)
        {
            this.Quantity = 1;
            if (resetAll)
                this.TotalBought = 1;

            return this;
        }
    }
}